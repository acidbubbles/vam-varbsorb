using System;
using System.Runtime.InteropServices;

namespace Varbsorb
{
    public class RecycleBin : IRecycleBin
    {
        public void Send(string path)
        {
            var shf = new SHFILEOPSTRUCT
            {
                wFunc = FO_DELETE,
                fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT,
                pFrom = $"{path}\0\0"
            };
            var result = SHFileOperation(ref shf);
            if (result != 0) throw new Exception($"Error while sending {path} to the recycle bin: {result}. See https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499- or https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shfileoperationa for the list of errors.");
        }

#pragma warning disable SA1307
#pragma warning disable SA1310
#pragma warning disable SA1313

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto/*, Pack = 1*/)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        public const int FO_DELETE = 0x0003;
        public const int FOF_ALLOWUNDO = 0x40;
        public const int FOF_NOCONFIRMATION = 0x10; // Don't prompt the user
        public const int FOF_NOERRORUI = 0x0400;
        public const int FOF_SILENT = 0x0004;

#pragma warning restore SA1307
#pragma warning restore SA1310
#pragma warning restore SA1313
    }

    public interface IRecycleBin
    {
        void Send(string path);
    }
}
