﻿using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

// ReSharper disable InconsistentNaming

namespace Mordor.Process.Internal.Win32
{
    internal static unsafe partial class NativeMethods
    {
        [Flags]
        internal enum StartupFlags : uint
        {
            None = 0x0,
            UseCountChars = 0x00000008,
            UseFillAttribute = 0x00000010,
            UseHotKey = 0x00000200,
            UsePosition = 0x00000004,
            UseShowWindow = 0x00000001,
            UseSize = 0x00000002,
            UseStdHandles = 0x00000100
        }

        internal enum ShowWindowOptions : uint
        {
            ForceMinimize = 11,
            Hide = 0,
            Maximize = 3,
            Minimize = 6,
            Restore = 9,
            Show = 5,
            ShowDefault = 10,
            ShowMaximized = 3 // ???
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct STARTUP_INFO : IDisposable
        {
            public uint cb;
            public char* lpReserved;
            public char* lpDesktop;
            public char* lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public StartupFlags dwFlags;
            public ShowWindowOptions wShowWindow;
            public ushort cbReserved2;
            public byte* lpReserved2;
            public int hStdInput;
            public int hStdOutput;
            public int hStdError;

            internal static uint SizeOf()
            {

                return (uint) sizeof(STARTUP_INFO);
            }

            public override string ToString()
            {
                return lpTitle->ToString();
            }

            public void Dispose()
            {
                CloseHandle(new IntPtr(hStdInput));
                CloseHandle(new IntPtr(hStdOutput));
                CloseHandle(new IntPtr(hStdError));
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SuspendThread(SafeThreadHandle handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(SafeThreadHandle safeThreadHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeProcessHandle GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeThreadHandle GetCurrentThread();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetStartupInfo(out STARTUP_INFO lpStartupInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool TerminateProcess(SafeProcessHandle handle, uint exitCode);        
    }
}
