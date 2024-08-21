using System;

namespace CTecUtil.StandardPanelDataTypes
{
    public class Integer
    {
        public Integer(int value) => Value = value;


        public int Value { get; set; }


        /// <summary>
        /// Convert Integer to a four-byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray() => ByteArrayProcessing.IntToByteArray(Value, 4);


        /// <summary>
        /// Parse an Integer from the byte data.
        /// </summary>
        /// <param name="data">Byte array from which to read the data.</param>
        /// <param name="typeCheck">Delegate function to check that the data's type code (i.e. record type) is correct.</param>
        /// <param name="length"></param>
        /// <param name="startOffset">Index of start of data.<br/>Default is 2, which allows for the first 2 bytes being the type code.</param>
        /// <returns></returns>
        public static Integer Parse(byte[] data, Func<byte[], bool> typeCheck, int length, int startOffset = 2)
        {
            if (!typeCheck?.Invoke(data)??false)
                return null;

            var result = new Integer(0);

            try
            {
                int intBytes = 0;
                for (int tmp = int.MaxValue; tmp > 0; tmp >>= 8)
                    intBytes++;

                var endOffset = Math.Min(length, Math.Min(data.Length, intBytes)) + startOffset - 1;

                for (int i = endOffset; i >= startOffset; i--)
                {
                    result.Value <<= 8;
                    result.Value += data[i];
                }
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(Parse) + " failed: " + ex.ToString());
                return result;
            }
        }
    }
}
