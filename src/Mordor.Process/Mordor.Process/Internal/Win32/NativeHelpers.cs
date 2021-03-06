﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mordor.Process.Internal.Win32
{
    internal static class NativeHelpers
    {
        public static async Task WaitForSafeHandleAsync(SafeHandle handle, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => WaitForSafeHandle(handle, timeout), cancellationToken);
        }

        public static void WaitForSafeHandle(SafeHandle handle, TimeSpan timeout)
        {
            ThrowInvalidHandleException(handle);

            EnsureSuccessWaitResult(NativeMethods.WaitForSingleObject(handle, (WaitTimeout)timeout));
        }

        public static unsafe int WaitAny(TimeSpan timeout, params SafeHandle[] handles)
        {
            if (handles.Length == 0)
                return -1;

            ThrowIfHandleCountOutOfRange(handles.Length);

            var ptrs = handles.Select(h => h.DangerousGetHandle()).ToArray();
            uint result;

            fixed (IntPtr* p0 = ptrs)
                result = NativeMethods.WaitForMultipleObjects(handles.Length, p0, false, (WaitTimeout) timeout);

            EnsureSuccessWaitResult(result);

            return (int) (result - (int) WaitResult.Signaled);
        }

        public static unsafe void WaitAll(TimeSpan timeout, params SafeHandle[] handles)
        {
            if (handles.Length == 0)
                return;

            var ptrs = handles.Select(h => h.DangerousGetHandle()).ToArray();
            uint result;

            fixed (IntPtr* p0 = ptrs)
                result = NativeMethods.WaitForMultipleObjects(handles.Length, p0, true, (WaitTimeout) timeout);

            EnsureSuccessWaitResult(result);
        }

        private static void EnsureSuccessWaitResult(WaitResult result)
        {
            EnsureSuccessWaitResult((uint) result);
        }

        private static void EnsureSuccessWaitResult(uint result)
        {
            if (result == unchecked((uint) WaitResult.Failed))
                ThrowLastWin32Exception();

            if (result == (int) WaitResult.TimedOut)
                throw new TimeoutException("The wait operation timed out.");

            if (result >= NativeMethods.MAXIMUM_WAIT_OBJECTS)
                throw new AbandonedMutexException((int) result - (int) WaitResult.Abandoned, null);
        }

        public static unsafe IEnumerable<string> GetProcessModuleNames(Process process)
        {
            var modules = new IntPtr[1024];
            int count;

            fixed (IntPtr *p0 = modules)
            {
                var size = (uint) (sizeof(IntPtr) * modules.Length);

                if (!NativeMethods.EnumProcessModules(process.SafeProcessHandle, p0, size, out var needed))
                    ThrowLastWin32Exception();

                count = (int) needed / sizeof(IntPtr);

                if (needed > size)
                {
                    Array.Resize(ref modules, count);

                    if (!NativeMethods.EnumProcessModules(process.SafeProcessHandle, p0, size, out _))
                        ThrowLastWin32Exception();
                }
            }

            var fileNames = new string[count];

            for (var i = 0; i < modules.Length; i++)
            {
                var fileName = new StringBuilder(NativeMethods.MAX_PATH);
                var result = NativeMethods.GetModuleFileName(modules[i], fileName, NativeMethods.MAX_PATH);

                fileNames[i] = result.ToString();
            }

            return fileNames;
        }

        public static void ThrowLastWin32Exception()
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        internal static void ThrowInvalidHandleException(SafeHandle handle)
        {
            if (!handle.IsInvalid)
                return;

            throw new ArgumentException("The handle is invalid.", nameof(handle));
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ThrowIfHandleCountOutOfRange(int count)
        {
            if (count > NativeMethods.MAXIMUM_WAIT_OBJECTS)
                throw new ArgumentOutOfRangeException("The maximum number of objects that can be waited on is " +
                                                      NativeMethods.MAXIMUM_WAIT_OBJECTS + ".");
        }
    }
}