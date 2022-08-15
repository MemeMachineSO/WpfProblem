using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfProblem.Structures;
using WpfProblem.ViewModels;

namespace WpfProblem
{
    public class ChildTabViewModel : ObservableObject
    {
        public ObservableCollection<DriveViewModel> Drives { get; set; } = new();
        public ObservableCollection<FileFolderViewModel> FileFolders { get; set; } = new();

        private string _path;
        private string _header = "This PC";
        private bool _DrivesListViewEnabled = true;
        private bool _FileFolderListViewEnabled = false;
        private bool _isLoading = false;

        public string TextBoxPath
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }
        public bool DrivesListViewEnabled
        {
            get => _DrivesListViewEnabled;
            set => SetProperty(ref _DrivesListViewEnabled, value);
        }
        public bool FileFolderListViewEnabled
        {
            get => _FileFolderListViewEnabled;
            set => SetProperty(ref _FileFolderListViewEnabled, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ChildTabViewModel(bool NewTabButton = false)
        {
            if(NewTabButton)
            {
                Header = "+";
                return;
            }

            foreach(Drive drive in GetDrives())
            {
                Drives.Add(new DriveViewModel(drive));
            }
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

            if (string.IsNullOrEmpty(TextBoxPath)) // we are in root directory
            {
                BrowseDirectory(selectedDrive.GetPath());
            }
            else
            {
                if (SelectedFileFolderViewModel.Name == "..") // Go up a directory
                {
                    if (TextBoxPath.EndsWith(":\\")) // We are at the root of the drive
                    {
                        BrowseDrives();
                    }
                    else
                    {
                        TextBoxPath = TextBoxPath.Substring(0, TextBoxPath.LastIndexOf("\\"));

                        if (TextBoxPath.EndsWith(":")) // Make sure the '\\' is still at the end if it's a drive
                            TextBoxPath += "\\";

                        Header = TextBoxPath;

                        BrowseDirectory(TextBoxPath);
                    }
                }
                else
                {
                    if (SelectedFileFolderViewModel.Type == "File")
                    {
                       //
                    }
                    else
                    {
                        if (!TextBoxPath.EndsWith("\\"))
                            TextBoxPath += "\\";

                        BrowseDirectory(TextBoxPath + SelectedFileFolderViewModel.Name);
                    }
                }
            }
        }

        void BrowseDirectory(string path)
        {
            FileFolders.Clear();

            TextBoxPath = path;
            Header = TextBoxPath;

            IsLoading = true;
            DrivesListViewEnabled = false;
            FileFolderListViewEnabled = false;

            FileFolders.Add(new FileFolderViewModel(new FileFolder { path = ".." }));

            foreach (FileFolder fileFolder in GetFilesAndFolders(path))
            {
                FileFolders.Add(new FileFolderViewModel(fileFolder));
            }

            IsLoading = false;
            DrivesListViewEnabled = false;
            FileFolderListViewEnabled = true; 
        }

        void BrowseDrives()
        {
            Drives.Clear();

            IsLoading = true;
            DrivesListViewEnabled = false;
            FileFolderListViewEnabled = false;

            TextBoxPath = string.Empty;
            Header = "This PC";

            foreach (Drive drive in GetDrives())
            {
                Drives.Add(new DriveViewModel(drive));
            }

            IsLoading = false;
            DrivesListViewEnabled = true;
            FileFolderListViewEnabled = false;
        }

        IEnumerable<Drive> GetDrives()
        {
            foreach(DriveInfo driveInfo in DriveInfo.GetDrives())
            {
                Drive drive = new();
                drive.driveLetter = Convert.ToByte(driveInfo.Name[0]);
                drive.volumeName = driveInfo.VolumeLabel;
                drive.totalSize = (ulong)driveInfo.TotalSize;
                drive.freeSpace = (ulong)driveInfo.AvailableFreeSpace;
                //drive.freeSpace = (ulong)driveInfo.AvailableFreeSpace;

                yield return drive;
            }
        }

        IEnumerable<FileFolder> GetFilesAndFolders(string path)
        {
            foreach (string filePath in Directory.GetFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly))
            {
                FileFolder fileFolder = new();
                try
                {
                    
                    fileFolder.path = filePath;
                    fileFolder.size = (ulong)new FileInfo(filePath).Length;
                    fileFolder.lastwrite = new FileInfo(filePath).CreationTime.ToFileTime();
                }
                catch
                {

                }

                yield return fileFolder;
            }
        }
    }
}
