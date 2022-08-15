using System;
using WpfProblem.Structures;

namespace WpfProblem.ViewModels
{
	public class FileFolderViewModel
	{
		public FileFolder FileFolder { get; }

		public FileFolderViewModel(FileFolder fileFolder)
		{
			FileFolder = fileFolder;
		}

		public string Name
		{
			get 
			{ 
				return FileFolder.path.Substring(FileFolder.path.LastIndexOf(@"\") + 1); 
			}
		}
		public string Type
		{
			get
			{
				return FileFolder.size == 0 ? "Folder" : "File";
			}
		}
		public string DateModified
		{
			get
			{
				if (FileFolder.lastwrite == 0)
					return String.Empty;

				return DateTime.FromFileTime(FileFolder.lastwrite).ToString();
			}
		}
		public string Size 
		{ 
			get 
			{
				if (FileFolder.size == 0)
					return string.Empty;
				return ParseSize(FileFolder.size); 
			} 
		}
		private string ParseSize(ulong size)
		{
			if (size >= 1000) // 1kb
			{
				if (size >= 1000000) // 1mb
				{
					if (size >= 1000000000) // 1gb
					{
						return (size / 1000000000).ToString() + " GB";
					}

					return (size / 1000000).ToString() + " MB";
				}

				return (size / 1000).ToString() + " KB";
			}

			return size + " bytes";
		}
	}
}