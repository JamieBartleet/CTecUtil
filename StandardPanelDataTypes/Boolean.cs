using System;

namespace CTecUtil.StandardPanelDataTypes
{
    public class Boolean
    {
        public Boolean(bool value) => Value = value;


        public bool Value { get; set; }


        /// <summary>
        /// Convert Boolean to byte array consisting of 1 byte (0=false, 1=true).
        /// </summary>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        public byte[] ToByteArray() => [ (byte)(Value ? 1 : 0) ];
#else
        public byte[] ToByteArray() => new byte[] { (byte)(Value ? 1 : 0) };
#endif


        /// <summary>
        /// Parse a Boolean from the byte data.
        /// </summary>
        /// <param name="data">Byte array from which to read the data.</param>
        /// <param name="typeCheck">Delegate function to check that the data's type code (i.e. record type) is correct.</param>
        /// <param name="startOffset">Index of start of data.<br/>Default is 2, which allows for the first 2 bytes being the type code.</param>
        /// <returns></returns>
        public static Boolean Parse(byte[] data, Func<byte[], bool> typeCheck, int startOffset = 2)
        {
            if (!typeCheck?.Invoke(data)??false)
                return null;
            return new(data.Length > startOffset + 1 ? data[startOffset] > 0 : false);
        }
    }
}
