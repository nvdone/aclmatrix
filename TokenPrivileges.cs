//NVD ACLMatrix
//Copyright © 2016-2021, Nikolay Dudkin

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Runtime.InteropServices;

namespace aclmatrix
{
	static class TokenPrivileges
	{
		#region Native
		const UInt32 TOKEN_QUERY = 0x0008;
		const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
		const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
		const UInt32 ERROR_SUCCESS = 0x0;

		[StructLayout(LayoutKind.Sequential)]
		struct LUID
		{
			public UInt32 LowPart;
			public Int32 HighPart;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct TOKEN_PRIVILEGES
		{
			public int PrivilegeCount;
			public LUID Luid;
			public UInt32 Attributes;
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, [MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, UInt32 BufferLengthInBytes, IntPtr PreviousState, IntPtr ReturnLengthInBytes);

		[DllImport("advapi32.dll")]
		static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, ref LUID lpLuid);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll")]
		static extern UInt32 GetLastError();

		#endregion

		public static bool SetBackupPrivilege()
		{
			IntPtr processHandle = GetCurrentProcess();
			IntPtr tokenHandle;
			TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES();
			LUID luid = new LUID();

			if (!OpenProcessToken(processHandle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out tokenHandle))
				return false;

			if (!LookupPrivilegeValue(null, "SeBackupPrivilege", ref luid))
			{
				CloseHandle(tokenHandle);
				return false;
			}

			tp.PrivilegeCount = 1;
			tp.Luid = luid;
			tp.Attributes = SE_PRIVILEGE_ENABLED;

			if (!AdjustTokenPrivileges(tokenHandle, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
			{
				CloseHandle(tokenHandle);
				return false;
			}

			if(GetLastError() != ERROR_SUCCESS)
			{
				CloseHandle(tokenHandle);
				return false;
			}

			CloseHandle(tokenHandle);
			return true;
		}
	}
}
