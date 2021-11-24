using System;
using System.Collections.Generic;
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

        
        public static string ByteArrayToString(byte[] data)
        {
            if (data is null) return "*empty*";
            var result = new StringBuilder();
            foreach (var b in data)
                result.Append(string.Format("{0:X2} ", b));
            return result.ToString().Trim();
        }


        public static bool IsNumeric(string text)  => new Regex("^[0-9]+").IsMatch(text);

    }
}
