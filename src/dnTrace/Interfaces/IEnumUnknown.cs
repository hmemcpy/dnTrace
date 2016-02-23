using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace dnTrace.Interfaces
{
    /// <summary>
    /// Enumerates objects with the IUnknown interface. It can be used to enumerate through the objects in a component containing multiple objects.
    /// 
    /// </summary>
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000100-0000-0000-C000-000000000046")]
    [ComImport]
    public interface IEnumUnknown
    {
        /// <summary>
        /// Retrieves the specified number of items in the enumeration sequence.
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Next([In] uint elementArrayLength, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown), Out] object[] elementArray, out uint fetchedElementCount);

        /// <summary>
        /// Skips over the specified number of items in the enumeration sequence.
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Skip([In] uint count);

        /// <summary>
        /// Resets the enumeration sequence to the beginning.
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Reset();

        /// <summary>
        /// Creates a new enumerator that contains the same enumeration state as the current one.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// This method makes it possible to record a point in the enumeration sequence in order to return to that point at a later time. The caller must release this new enumerator separately from the first enumerator.
        /// </remarks>
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Clone([MarshalAs(UnmanagedType.Interface)] out IEnumUnknown enumerator);
    }
}