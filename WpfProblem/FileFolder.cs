using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfProblem.Structures
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	public struct FileFolder
	{
		[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string path { get; set; }
		public long lastwrite { get; set; }
		public ulong size { get; set; }
	};
}
