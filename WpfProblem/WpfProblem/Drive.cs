using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace WpfProblem.Structures
{
	public enum DriveDataType
	{
		DRIVE_UNKNOWN,
		DRIVE_NO_ROOT_DIR,
		DRIVE_REMOVABLE,
		DRIVE_FIXED,
		DRIVE_REMOTE,
		DRIVE_CDROM,
		DRIVE_RAMDISK,
	};

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	public struct Drive
	{
		public byte driveLetter;
		[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string volumeName;
		public uint serialNumber;
		public DriveDataType type;
		public ulong totalSize;
		public ulong freeSpace;
	};
}
