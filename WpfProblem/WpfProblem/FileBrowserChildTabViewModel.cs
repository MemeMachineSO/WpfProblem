using FileExplorerModuleServer.Enums;
using FileExplorerModuleServer.Models;
using FileExplorerModuleServer.Stuctures;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ModularLib.NET;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace FileExplorerModuleServer.ViewModels
{
	public class FileBrowserChildTabViewModel : ViewModelBase
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		// these are the items that will be shown in the list view
		public ObservableCollection<DriveViewModel> Drives { get; set; } = new();
		public ObservableCollection<FileFolderViewModel> FileFolders { get; set; } = new();

		private FileBrowserTabViewModel _FileExplorerTab;
		private ModularClient _client;
		private string _ModuleName;

		private string _header;
		private string _path;
		private bool _DrivesListViewEnabled = true;
		private bool _FileFolderListViewEnabled = false;
		private bool _isLoading = false;

		public string Header
		{
			get => _header;
			set => Set(ref _header, value);
		}
		public string TextBoxPath
		{
			get => _path;
			set => Set(ref _path, value);
		}
		public bool DrivesListViewEnabled
		{
			get { return this._DrivesListViewEnabled; }
			set
			{
				this._DrivesListViewEnabled = value;
				this.NotifyPropertyChanged(nameof(DrivesListViewEnabled));
			}
		}
		public bool FileFolderListViewEnabled
		{
			get { return this._FileFolderListViewEnabled; }
			set
			{
				this._FileFolderListViewEnabled = value;
				this.NotifyPropertyChanged(nameof(FileFolderListViewEnabled));
			}
		}
		public bool IsLoading
		{
			get { return this._isLoading; }
			set
			{
				this._isLoading = value;
				this.NotifyPropertyChanged(nameof(IsLoading));
			}
		}

		public FileBrowserChildTabViewModel()
		{
			Header = "+";
		}

		public FileBrowserChildTabViewModel(FileBrowserTabViewModel FileExplorerTab)
		{
			_FileExplorerTab = FileExplorerTab;
			_client = _FileExplorerTab.client;
			_ModuleName = _FileExplorerTab.moduleName;
		}

		public ICommand MouseDoubleClickCommand
			=> new RelayCommand<object>(e => MouseDoubleClicked(e));

		private void MouseDoubleClicked(object commandParameter)
		{
			ListView listView = commandParameter as ListView;

			DriveViewModel selectedDrive = null;
			FileFolderViewModel SelectedFileFolderViewModel = null;
			if (listView.SelectedItem is DriveViewModel)
				selectedDrive = listView.SelectedItem as DriveViewModel;
			else
				SelectedFileFolderViewModel = listView.SelectedItem as FileFolderViewModel;

			if (String.IsNullOrEmpty(TextBoxPath)) // we are in root directory
			{
				IsLoading = true;
				DrivesListViewEnabled = false;
				FileFolderListViewEnabled = false;

				_client.Send(_ModuleName, (byte)FileExplorerCommands.BrowseDirectory, selectedDrive.GetPath(), ExecuteBrowseDirectoryCommand);
				TextBoxPath = selectedDrive.GetPath();
				Header = TextBoxPath;
			}
			else
			{
				if (SelectedFileFolderViewModel.Name == "..") // Go up a directory
				{
					if (TextBoxPath.EndsWith(":\\")) // We are at the root of the drive
					{
						IsLoading = true;
						DrivesListViewEnabled = false;
						FileFolderListViewEnabled = false;

						_client.Send(_ModuleName, (byte)FileExplorerCommands.GetDrives, ExecuteUpdateDrivesCommand);
					}
					else
					{
						TextBoxPath = TextBoxPath.Substring(0, TextBoxPath.LastIndexOf("\\"));

						if (TextBoxPath.EndsWith(":")) // Make sure the '\\' is still at the end if it's a drive
							TextBoxPath += "\\";

						Header = TextBoxPath;

						IsLoading = true;
						DrivesListViewEnabled = false;
						FileFolderListViewEnabled = false;

						_client.Send(_ModuleName, (byte)FileExplorerCommands.BrowseDirectory, TextBoxPath, ExecuteBrowseDirectoryCommand);
					}
				}
				else
				{
					if (SelectedFileFolderViewModel.Type == "File")
					{
						DownloadFile(SelectedFileFolderViewModel.FileFolder, $"servers\\{_client.GetClientInformation().Server.Name}\\clients\\{_client.GetClientInformation().ComputerUser.Replace("/", "-")}\\Data\\File Explorer\\Downloads\\{SelectedFileFolderViewModel.Name}");
					}
					else
					{
						IsLoading = true;
						DrivesListViewEnabled = false;
						FileFolderListViewEnabled = false;

						if (!TextBoxPath.EndsWith("\\"))
							TextBoxPath += "\\";

						_client.Send(_ModuleName, (byte)FileExplorerCommands.BrowseDirectory, TextBoxPath + SelectedFileFolderViewModel.Name, ExecuteBrowseDirectoryCommand);
						TextBoxPath += SelectedFileFolderViewModel.Name;
					}
				}
			}
		}

		public ICommand MouseDownCommand_Drives
			=> new RelayCommand(() =>
			{
				Debug.WriteLine("Mouse down on Drives ListView");

				IsLoading = true;
				DrivesListViewEnabled = false;
				FileFolderListViewEnabled = false;

				_client.Send(_ModuleName, (byte)FileExplorerCommands.GetDrives, ExecuteUpdateDrivesCommand);
			});

		public ICommand MouseDownCommand_FileFolders
			=> new RelayCommand(() =>
			{
				Debug.WriteLine("Mouse down on FileFolders ListView");

				IsLoading = true;
				DrivesListViewEnabled = false;
				FileFolderListViewEnabled = false;

				_client.Send(_ModuleName, (byte)FileExplorerCommands.BrowseDirectory, TextBoxPath, ExecuteBrowseDirectoryCommand);
			});

		public ICommand DownloadCommand
			=> new RelayCommand<object>(e => ExecuteDownloadDirectoryCommand(e));

		public ICommand UploadCommand
			=> new RelayCommand<object>(e => ExecuteUploadCommand(e));

		public void ExecuteUploadCommand(object commandParameter)
		{
			//OpenFileDialog ofd = new OpenFileDialog();
			//if (ofd.ShowDialog() == true)
			//{
			//	long fileSize = new FileInfo(ofd.FileName).Length;

			//	ulong transferId = (ulong)new Random().NextInt64();

			//	UploadRequest uploadRequest = new();
			//	uploadRequest.path = Path.Combine(_path, Path.GetFileName(ofd.FileName));
			//	uploadRequest.size = (ulong)fileSize;
			//	uploadRequest.uploadId = transferId;

			//	long cookie = _client.CreateCookie();
			//	_client.Send(_ModuleName, (byte)FileExplorerCommands.CreateUpload, uploadRequest, null, cookie);

			//	var transfer = new DownloadViewModel();
			//	transfer.Name = Path.GetFileName(ofd.FileName);
			//	transfer.SizeInBytes = (ulong)fileSize;
			//	//transfer.Size = fileFolder.Size;

			//	transfer.fileStream = File.OpenRead(ofd.FileName);

			//	_FileExplorerTab._MainViewModel.FileTransferTab.Uploads.Add(cookie, transfer);
			//	_FileExplorerTab._MainViewModel.FileTransferTab.UploadQueue.Add(transfer);

			//	for (ulong bytesRead = 0; bytesRead < (ulong)fileSize;)
			//	{
			//		ulong ChunkSize = Math.Min(1024000, transfer.SizeInBytes - bytesRead);

			//		UploadChunk chunk = new UploadChunk();
			//		chunk.upload_id = transferId;
			//		chunk.chunk_size = ChunkSize;
			//		chunk.chunk_offset = bytesRead;

			//		byte[] FileChunkBytes = new byte[ChunkSize];
			//		int bytesReady = transfer.fileStream.Read(FileChunkBytes, 0, (int)ChunkSize);

			//		int UploadChunkStructSize = Marshal.SizeOf(typeof(UploadChunk));
			//		byte[] bytes = new byte[(ulong)UploadChunkStructSize + ChunkSize];

			//		// Structure to bytes and copy to 'bytes' array
			//		IntPtr UploadChunkPointer = IntPtr.Zero;
			//		UploadChunkPointer = Marshal.AllocHGlobal(UploadChunkStructSize);
			//		Marshal.StructureToPtr(chunk, UploadChunkPointer, true);
			//		Marshal.Copy(UploadChunkPointer, bytes, 0, UploadChunkStructSize);
			//		Marshal.FreeHGlobal(UploadChunkPointer);

			//		Buffer.BlockCopy(FileChunkBytes, 0, bytes, UploadChunkStructSize, (int)ChunkSize);

			//		_client.Send(
			//				_ModuleName, (byte)FileExplorerCommands.UploadFile, bytes);

			//		bytesRead += ChunkSize;
			//	}
			//}
		}

		public void ExecuteDownloadDirectoryCommand(object commandParameter)
		{
			FileFolderViewModel SelectedFileFolder = commandParameter as FileFolderViewModel;

			if (SelectedFileFolder.Type == "Folder")
			{
				_client.Send(
					_ModuleName, (byte)FileExplorerCommands.BrowseDirectoryRecursive, TextBoxPath + "\\" + SelectedFileFolder.Name, ExecuteDownloadDirectory);
			}
			else
			{

			}
		}

		void DownloadFile(FileFolder FileFolder, string path)
		{
			Download download = new();
			download.Name = Path.GetFileName(FileFolder.path);
			download.SizeInBytes = FileFolder.size;
			download.fileStream = File.Create(path);
			download.RemotePath = FileFolder.path;

			var transfer = new DownloadViewModel(download);
			download.DownloadUpdated += transfer.DownloadUpdated;

			_FileExplorerTab._MainViewModel.FileTransferTab.DownloadManager.Download(download);
			_FileExplorerTab._MainViewModel.FileTransferTab.DownloadQueue.Add(transfer);
		}

		public void ExecuteUpdateDrivesCommand(ModularPacket commandParameter)
		{
			if (!(commandParameter is ModularPacket packet))
			{
				return;
			}
			//_drives.Clear();
			Debug.WriteLine("Drives! Drive size is " + Marshal.SizeOf(typeof(Drive)));

			Header = "This PC";
			TextBoxPath = String.Empty;
			Drives.Clear();

			foreach (DriveViewModel drive in ModularPacketManager.UpdateDrives(packet))
			{
				Drives.Add(drive);
			}

			IsLoading = false;
			DrivesListViewEnabled = true;
			FileFolderListViewEnabled = false;
		}

		public void ExecuteBrowseDirectoryCommand(ModularPacket commandParameter)
		{
			if (!(commandParameter is ModularPacket packet))
				return;

			string path = ModularPacketManager.GetDirectory(packet).path;
			string directoryName = new DirectoryInfo(path).Name;
			Header = directoryName;

			FileFolders.Clear();
			
			FileFolders.Add(new FileFolderViewModel(new FileFolder() { path = ".." }));
			foreach (FileFolder folder in ModularPacketManager.BrowseDirectory(packet))
			{
				FileFolders.Add(new FileFolderViewModel(folder));
			}

			IsLoading = false;
			DrivesListViewEnabled = false;
			FileFolderListViewEnabled = true;
		}

		public void ExecuteDownloadDirectory(ModularPacket commandParameter)
		{
			if (!(commandParameter is ModularPacket packet))
				return;

			string directoryPath = ModularPacketManager.GetDirectory(packet).path;
			Debug.WriteLine($"Directory is: {Path.GetDirectoryName(directoryPath)}");

			string RelativeDirectoryPath = directoryPath.Substring(0, directoryPath.LastIndexOf(@"\") + 1);

			foreach (FileFolder fileFolder in ModularPacketManager.BrowseDirectory(packet))
			{
				string relativePath = fileFolder.path.Replace(RelativeDirectoryPath, string.Empty);

				string localPath = $"servers\\{_client.GetClientInformation().Server.Name}\\clients\\{_client.GetClientInformation().ComputerUser.Replace("/", "-")}\\Data\\File Explorer\\Downloads\\{relativePath}";

				Directory.CreateDirectory(Directory.GetParent(localPath).FullName);

				if (fileFolder.size > 0)
				{
					DownloadFile(fileFolder, localPath);

					Debug.WriteLine(localPath);
				}
			}
		}
	}
}