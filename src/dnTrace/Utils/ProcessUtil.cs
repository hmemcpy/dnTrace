using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using dnTrace.ProcessData;
using Microsoft.Win32.SafeHandles;
using PInvoke;
using static PInvoke.Kernel32;
using static PInvoke.NTDll;

namespace dnTrace.Utils
{
    internal static class ProcessUtil
    {
        private const int SystemIdleProcessId = 0;
        private const int SystemProcessId = 4;

        public static bool Is64BitProcess(SafeObjectHandle handle) => !IsWow64Process(handle);

        /// <summary>
        /// A rough way to detect .NET processes. Works even if the target process (identified by processId) is elevated and ours isn't.
        /// </summary>
        public static Runtimes GetExpectedProcessRuntimes(int processId)
        {
            Func<string, bool> readSection = sectionName =>
            {
                NTSTATUS status;
                SafeNTObjectHandle handle = null;
                try
                {
                    var objectAttributes = OBJECT_ATTRIBUTES.Create();
                    objectAttributes.Attributes = OBJECT_ATTRIBUTES.ObjectHandleAttributes.OBJ_CASE_INSENSITIVE;
                    var accessMask = (ACCESS_MASK)1;
                    status = NtOpenSection(out handle, accessMask, objectAttributes);
                }
                finally
                {
                    handle?.Close();
                }

                return status.ToHResult().Succeeded;
            };

            var runtimes = Runtimes.Unknown;
            if (readSection($"\\BaseNamedObjects\\Cor_Private_IPCBlock_v4_{processId}"))
                runtimes |= Runtimes.Clr4;

            if (readSection($"\\BaseNamedObjects\\Cor_Private_IPCBlock_{processId}"))
                runtimes |= Runtimes.Clr2;

            return runtimes;
        }

        public static IEnumerable<ProcessInfo> GetAllProcesses()
        {
            using (var hSnapshot = CreateToolhelp32Snapshot(CreateToolhelp32SnapshotFlags.TH32CS_SNAPPROCESS, 0))
            {
                if (hSnapshot.IsInvalid)
                    throw new Win32Exception();

                return Process32Enumerate(hSnapshot)
                            .Where(entry => !IsSystemProcess(entry.th32ProcessID))
                            .Select(entry => new ProcessInfo(entry))
                            .ToList()
                            .AsReadOnly();
            }
        }

        public static string GetProcessFileName(SafeObjectHandle hProcess) => QueryFullProcessImageName(hProcess);

        public static IEnumerable<ModuleInfo> GetProcessModules(int processId)
        {
            if (IsSystemProcess(processId))
                return Enumerable.Empty<ModuleInfo>();

            var flags = CreateToolhelp32SnapshotFlags.TH32CS_SNAPMODULE |
                        CreateToolhelp32SnapshotFlags.TH32CS_SNAPMODULE32;

            using (var hSnapshot = CreateToolhelp32Snapshot(flags, processId))
            {
                if (hSnapshot.IsInvalid)
                {
                    var hr = new HResult(Marshal.GetHRForLastWin32Error());
                    switch (hr.Value)
                    {
                        case HResult.Code.E_ACCESSDENIED:
                        case HResult.Code.E_INVALIDARG:
                        case (HResult.Code)0x8007012b: // E_PARTIAL_COPY
                            break;
                        default:
                            hr.ThrowOnFailure();
                            break;
                    }

                    return Enumerable.Empty<ModuleInfo>();
                }

                return Module32Enumerate(hSnapshot)
                    .Select(entry => new ModuleInfo(entry))
                    .ToList()
                    .AsReadOnly();
            }
        }

        public static IEnumerable<string> GetProcessRuntimes(SafeHandle handle) => ClrUtil.GetProcessRuntimes(handle).ToList();

        public static string GetFileRuntime(string path) => ClrUtil.GetFileRuntime(path);

        private static bool IsSystemProcess(int processId) => processId == SystemProcessId || processId == SystemIdleProcessId;
    }
}