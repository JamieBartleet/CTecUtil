using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTecUtil
{
    public class ByteArrayProcessing
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


        /// <summary>
        /// Converts a byte array to an old-school ASCII text string, i.e. chars 0x40 to 0x7E plus CR and LF (any CRLF or LFCR combos are replaced with a single LF).<br/>
        /// Any other characters are replaced by a placeholder.
        /// </summary>
        public static string ByteArrayToString(byte[] data)
        {
            if (data is null) return "";
            var result = new StringBuilder();
            foreach (var b in data)
                result.Append(b switch { >0x1f and <0x7f or 0x0a or 0x0d => (char)b, _ => '·' });
            return result.ToString().Replace("\r\n", "\n").Replace("\n\r", "\n");
        }

        
        public static string ByteArrayToHexString(byte[] data)
        {
            if (data is null) return "*empty*";
            var result = new StringBuilder();
            foreach (var b in data)
                result.Append(string.Format("{0:X2} ", b));
            return result.ToString().Trim();
        }
    }
}
