using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using dnTrace.Utils;
using PInvoke;

namespace dnTrace.ProcessData
{
    [DebuggerDisplay("Name={Name}, Path={FileName}, IsManaged={IsManaged}, Is64Bit={Is64Bit}, AdminRequired={IsAccessDenied}")]
    internal sealed class ProcessInfo
    {
        public List<ModuleInfo> NativeModules { get; set; } = new List<ModuleInfo>();
        private readonly List<string> runtimes = new List<string>();

        public IReadOnlyCollection<string> Runtimes => runtimes;

        public int ProcessId { get; }
        public int ParentProcessId { get; private set; }
        public string Name { get; }
        public Runtimes ExpectedRuntime { get; }
        public bool IsAccessDenied { get; }
        public bool Is64Bit { get; }
        public string FileName { get; }
        public string MainWindowTitle { get; set; }

        public bool IsManaged
        {
            get
            {
                if (ExpectedRuntime == ProcessData.Runtimes.Unknown)
                    return runtimes.Count > 0;

                return true;
            }
        }

        private static readonly uint ProcessQueryLimitedInformation =
            Environment.OSVersion.Version.Major >= 6
                ? Kernel32.ProcessAccess.PROCESS_QUERY_LIMITED_INFORMATION
                : Kernel32.ProcessAccess.PROCESS_QUERY_INFORMATION;

        public ProcessInfo(Kernel32.PROCESSENTRY32 entry)
        {
            Name = entry.ExeFile;
            ProcessId = entry.th32ProcessID;
            ParentProcessId = entry.th32ParentProcessID;
            ExpectedRuntime = ProcessUtil.GetExpectedProcessRuntimes(ProcessId);

            using (var handle = Kernel32.OpenProcess(ProcessQueryLimitedInformation | Kernel32.ProcessAccess.PROCESS_VM_READ, false, ProcessId))
            {
                if (!handle.IsInvalid)
                {
                    Is64Bit = ProcessUtil.Is64BitProcess(handle);
                    FileName = ProcessUtil.GetProcessFileName(handle);
                    runtimes.AddRange(ProcessUtil.GetProcessRuntimes(handle));
                }
                else
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    switch (lastWin32Error)
                    {
                        case 0x5:  // ERROR_ACCESS_DENIED
                        case 0x57: // ERROR_INVALID_PARAMETER
                            IsAccessDenied = true;
                            break;
                    }
                }
            }
        }

        public void AddRuntime(string runtime)
        {
            if (runtime == null)
                throw new ArgumentNullException(nameof(runtime));

            if (runtimes.Contains(runtime))
                return;

            runtimes.Add(runtime);
        }

        public void GetModules()
        {
            var list = ProcessUtil.GetProcessModules(ProcessId);
            if (list == null)
                return;

            NativeModules = list.ToList();
        }
    }
}