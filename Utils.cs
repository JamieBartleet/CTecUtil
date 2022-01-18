using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CTecUtil
{
    public class Utils
    {
        /// <summary>
        /// Combine multiple byte arrays
        /// </summary>
        public static byte[] CombineByteArrays(params byte[][] arrays)
        {
            byte[] result = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }
            return result;
        }


        public static byte[] StringToByteArray(string value, int length, bool centred = false)
        {
            var result = new byte[length];
            var strLen = Math.Min(value.Length, length);
            var strStart = centred ? length / 2 - (strLen + 1) / 2 : 0;
            var strEnd   = strStart + strLen;

            //pad prefix if required
            for (int i = 0; i < strStart; i++)
                result[i] = (byte)' ';

            Buffer.BlockCopy(Encoding.ASCII.GetBytes(value), 0, result, strStart, strLen);

            //pad to length
            for (int i = strEnd; i < length; i++)
                result[i] = (byte)' ';

            return result;
        }

        
        public static string ByteArrayToString(byte[] data)
        {
            if (data is null) return "";
            var result = new StringBuilder();
            foreach (var b in data)
                result.Append(b switch { 0x0a => '\n', 0x0d => '\r', >0x1f and <0x7f => (char)b, _ => '▫' });
            return result.ToString();
        }

        
        public static string ByteArrayToHexString(byte[] data)
        {
            if (data is null) return "*empty*";
            var result = new StringBuilder();
            foreach (var b in data)
                result.Append(string.Format("{0:X2} ", b));
            return result.ToString().Trim();
        }


        public static bool IsNumeric(string text) => new Regex("^[0-9]+").IsMatch(text);
    }
}
