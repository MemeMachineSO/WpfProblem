using System;
using WpfProblem.Structures;

namespace WpfProblem.ViewModels
{
	public class DriveViewModel
	{
		Drive _drive;
		public DriveViewModel(Drive drive)
		{
			_drive = drive;
		}
		public string Name 
		{
			get { return $"{_drive.volumeName} ({Convert.ToChar(_drive.driveLetter)}:)"; }
		}
		public string Type
		{
			get
			{
				switch (_drive.type)
				{
					case DriveDataType.DRIVE_UNKNOWN:
						return "Unknown";
					case DriveDataType.DRIVE_NO_ROOT_DIR:
						return "Invalid root path/Not available";
					case DriveDataType.DRIVE_REMOVABLE:
						return "Removable";
					case DriveDataType.DRIVE_FIXED:
						return "Fixed";
					case DriveDataType.DRIVE_REMOTE:
						return "Network";
					case DriveDataType.DRIVE_CDROM:
						return "CD-ROM";
					case DriveDataType.DRIVE_RAMDISK:
						return "RAMDISK";
					default:
						return "Error";
				}
			}
		}
		public string TotalSize { get { return ParseSize(_drive.totalSize); } }
		public string FreeSpace { get { return ParseSize(_drive.freeSpace); } }

		public string GetPath()
		{
			return Convert.ToChar(_drive.driveLetter) + ":\\";
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
