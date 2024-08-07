using System;

namespace CTecUtil.StandardPanelDataTypes
{
    public class IndexedBoolean
    {
        public IndexedBoolean(int index, bool value) { Index = index; Value = value; }


        public int  Index { get; set; }
        public bool Value { get; set; }


        /// <summary>
        /// Convert IndexedBoolean to a two-byte array, consisting of the index and 0 or 1
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray() => [ (byte)Index, (byte)(Value ? 1 : 0) ];


        /// <summary>
        /// Parse an IndexedBoolean from the byte data.
        /// </summary>
        /// <param name="data">Byte array from which to read the data.</param>
        /// <param name="typeCheck">Delegate function to check that the data's type code (i.e. record type) is correct.</param>
        /// <param name="startOffset">Index of start of data.<br/>Default is 2, which allows for the first 2 bytes being the type code.</param>
        /// <returns></returns>
        public static IndexedBoolean Parse(byte[] data, Func<byte[], bool> typeCheck, int startOffset = 2)
        {
            if (!typeCheck?.Invoke(data)??false)
                return null;
            return new(data.Length > startOffset ? data[startOffset] : 0, data.Length > startOffset + 1 ? data[startOffset + 1] > 0 : false);
        }
    }
}
