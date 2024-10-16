using System;

namespace CTecUtil.StandardPanelDataTypes
{
    public class IndexedText
    {
        public IndexedText(int index, string value, int length) { Index = index; Value = value; Length = length; }


        public int    Index  { get; set; }

        public string Value  { get; set; }

        public int    Length { get; set; }


        /// <summary>
        /// Convert IndextText to a byte array
        /// </summary>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        public byte[] ToByteArray() => ByteArrayProcessing.CombineByteArrays([ (byte)(Index + 1) ], ByteArrayProcessing.StringToByteArray(Value, Length));
#else
        public byte[] ToByteArray() => ByteArrayProcessing.CombineByteArrays(new byte[] { (byte)Index }, ByteArrayProcessing.StringToByteArray(Value, Length));
#endif

        /// <summary>
        /// Parse an index and string from the byte data.
        /// </summary>
        /// <param name="data">Byte array from which to read the data.</param>
        /// <param name="typeCheck">Delegate function to check the data's command code (i.e. record type) is correct.</param>
        /// <param name="length">Maximum length of string to extract.</param>
        /// <param name="startOffset">Index of start of data.<br/>Default is 2, which allows for the first 2 bytes being the type code.</param>
        /// <returns></returns>
        public static IndexedText Parse(byte[] data, Func<byte[], bool> typeCheck, int? requestedIndex, int length, int startOffset = 2)
        {
            if (!typeCheck?.Invoke(data) ?? false)
                return null;

            var stringValue = "";
            if (data.Length > startOffset)
                stringValue = Text.ExtractString(data, length, true, startOffset);
            return new(requestedIndex??0, stringValue, length);
        }
    }
}
