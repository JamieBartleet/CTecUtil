using System;
using System.Text;
using Newtonsoft.Json;

namespace CTecUtil.StandardPanelDataTypes
{
    /// <summary>
    /// Defines a text value.
    /// </summary>
    public class Text
    {
        public Text(string value, int length) { Value = value; Length = length; }


        public string Value { get; set; }

        [JsonIgnore]
        public int    Length { get; set; }

        [JsonIgnore]
        public bool Centred { get; set; } = false;


        /// <summary>
        /// Convert Text to a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray() => ToByteArray(Centred);
        public byte[] ToByteArray(bool centred) => ByteArrayProcessing.StringToByteArray(Value, Length, centred);


        /// <summary>
        /// Parse a string from the byte data.
        /// </summary>
        /// <param name="data">Byte array from which to read the data.</param>
        /// <param name="typeCheck">Delegate function to check that the data's type code (i.e. record type) is correct.</param>
        /// <param name="length">Maximum length of string to extract.</param>
        /// <param name="trim">Trim any leading and trailing white space (default is True).</param>
        /// <param name="startOffset">Index of start of data.<br/>Default is 2, which allows for the first 2 bytes being the type code.</param>
        /// <returns></returns>
        public static Text Parse(byte[] data, Func<byte[],bool> typeCheck, int length, bool trim = true, int startOffset = 2)
        {
            if (!typeCheck?.Invoke(data) ?? false)
                return null;
            return new(ExtractString(data, length, trim, startOffset), length);
        }

        
        /// <summary>
        /// Parse the byte data as a string of up to the specified length.
        /// </summary>
        /// <param name="data">Byte array from which to read the string.</param>
        /// <param name="length">Maximum length of string to extract.</param>
        /// <param name="trim">Trim any leading and trailing white space (default is True).</param>
        /// <param name="startOffset">Index of start of string data.</param>
        /// <returns></returns>
        public static string ExtractString(byte[] data, int length, bool trim = true, int startOffset = 2)
        {
            try
            {
                StringBuilder result = new();
                for (int i = startOffset; i < length + startOffset && i <= data[1] + 1; i++)
                    if (data[i] > 0)
                        result.Append((char)data[i]);
                return trim ? result.ToString().Trim() : result.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(nameof(ExtractString) + " failed: " + ex.ToString());
                return "";
            }
        }
    }
}
