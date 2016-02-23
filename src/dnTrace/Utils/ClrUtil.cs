using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using dnTrace.Interfaces;
using PInvoke;

namespace dnTrace.Utils
{
    internal static class ClrUtil
    {
        private static readonly IClrMetaHost ClrMetaHost = CreateClrMetaHost();

        private static IClrMetaHost CreateClrMetaHost()
        {
            object pClrMetaHost;
            HResult result = MSCorEE.CLRCreateInstance(MSCorEE.CLSID_CLRMetaHost, typeof(IClrMetaHost).GUID, out pClrMetaHost);
            if (result.Failed)
            {
                throw new Win32Exception();
            }

            return (IClrMetaHost)pClrMetaHost;
        }

        public static IEnumerable<string> GetProcessRuntimes(SafeHandle handle)
        {
            var buffer = new StringBuilder(1024);
            int num = 0;
            if (ClrMetaHost != null)
            {
                IEnumUnknown ppEnumerator;
                num = ClrMetaHost.EnumerateLoadedRuntimes(handle.DangerousGetHandle(), out ppEnumerator);
                if (num >= 0)
                    return ppEnumerator.Cast<IClrRuntimeInfo>().Select(rti =>
                    {
                        int bufferLength = buffer.Capacity;
                        rti.GetVersionString(buffer, ref bufferLength);
                        return buffer.ToString();
                    });
            }

            return Enumerable.Empty<string>();
        }

        public static string GetFileRuntime(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            var buffer = new StringBuilder(Kernel32.MAX_PATH);

            if (ClrMetaHost != null)
            {
                int valueLength = buffer.Capacity;
                HResult result = ClrMetaHost.GetVersionFromFile(filename, buffer, ref valueLength);
                return result.Succeeded ? buffer.ToString() : null;
            }

            return MSCorEE.GetFileVersion(filename);
        }
    }

    public static class EnumUnknownExtensions
    {
        private static IEnumerator<object> GetEnumerator(this IEnumUnknown enumerator)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException(nameof(enumerator));
            }

            uint count;
            do
            {
                var elementArray = new object[1];
                enumerator.Next(1, elementArray, out count);
                if (count == 1)
                {
                    yield return elementArray[0];
                }
            }
            while (count > 0);
        }

        public static IEnumerable<T> Cast<T>(this IEnumUnknown enumerator)
        {
            var e = enumerator.GetEnumerator();
            while (e.MoveNext())
            {
                yield return (T)e.Current;
            }
        }
    }


}