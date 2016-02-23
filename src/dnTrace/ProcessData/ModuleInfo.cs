using System;
using System.Diagnostics;
using dnTrace.Utils;
using PInvoke;

namespace dnTrace.ProcessData
{
    [DebuggerDisplay("Name={Name}, Path={Path}, IsManaged={IsManaged}")]
    internal sealed class ModuleInfo
    {
        public int ProcessId { get; }

        public string Name { get; private set; }

        public string Path { get; }

        public string Runtime { get; }

        public bool IsManaged => Runtime != null;

        public ModuleInfo(Kernel32.MODULEENTRY32 entry)
        {
            ProcessId = entry.th32ProcessID;
            Name = entry.Module;
            Path = DeviceToFilePath(entry.ExePath);
            Runtime = ProcessUtil.GetFileRuntime(Path);
        }

        private static string DeviceToFilePath(string path)
        {
            if (path.StartsWith("\\??\\"))
                return path.Substring(4);
            return path;
        }
    }
}