using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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


        public static byte[] SubArray(byte[] data, int startByte, int? endByte = null)
        {
            endByte = endByte ?? data.Length - 1;
            var count = endByte.Value - startByte + 1;
            var result = new byte[count];
            Buffer.BlockCopy(data, startByte, result, 0, count);
            return result;
        }


        public static byte[] StringToByteArray(string value, bool centred = false) => StringToByteArray(value, value.Length, centred);

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


        public static byte[] IntStrToByteArray(string value, int length)
        {
            int num;
            if (int.TryParse(value, out num))
                return IntToByteArray(num, length);
            return new byte[] { };
        }


        public static byte[] IntToByteArray(int value, int length)
        {
            var temp = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(temp);

            var result = new byte[length];

            for (int i = 0; i < length; i++)
                result[i] = (byte)(i < temp.Length ? temp[i] : 0);

            return result;
        }


        /// <summary>
        /// Converts a byte array to an old-school ASCII text string, i.e. chars 0x40 to 0x7E plus CR and LF (any CRLF or LFCR combos are replaced with a single LF).<br/>
        /// Any other characters are replaced by a placeholder.
        /// </summary>
        public static string ByteArrayToString(byte[] data) => data != null ? ByteArrayToString(data, 0, data.Length - 1) : "";


        /// <summary>
        /// Converts a byte array to an old-school ASCII text string, i.e. chars 0x40 to 0x7E plus CR and LF (any CRLF or LFCR combos are replaced with a single LF).<br/>
        /// Bytes are space separated. Any other characters are replaced by a placeholder.
        /// </summary>
        public static string ByteArrayToFormattedString(byte[] data)
        {
            if (data is null)
                return "";
            
            var str = ByteArrayToString(data, 0, data.Length - 1);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i += 2)
            {
                sb.Append(" " + str[i]);
                if (i + 1 < str.Length)
                    sb.Append(str[i + 1]);
            }

            return sb.ToString().Trim();
        }


        /// <summary>
        /// Converts a byte array to an old-school ASCII text string, i.e. chars 0x40 to 0x7E plus CR and LF (any CRLF or LFCR combos are replaced with a single LF).<br/>
        /// Any other characters are replaced by a placeholder.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startByte">Start index of the required string</param>
        /// <param name="endByte">End index of the required string</param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] data, int startByte, int endByte)
        {
            if (data is null) return "*null*";
            var result = new StringBuilder();
            for (int i = startByte; i <= endByte; i++)
                result.Append(data[i] switch { >0x1f and <0x7f or 0x0a or 0x0d => (char)data[i], _ => '·' });
            return result.ToString().Replace("\r\n", "\n").Replace("\n\r", "\n");
        }


        public static string ByteArrayToHexString(byte[] data)
        {
            if (data is null) return "*null*";
            if (data.Length == 0) return "*empty*";
            var result = new StringBuilder();
            foreach (var b in data)
                result.Append(string.Format("{0:X2} ", b));
            return result.ToString().Trim();
        }


        /// <summary>
        /// Find the first occurrence of targetByte in data starting from startIndex (default is the start of data).
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int IndexOf(byte[] data, byte targetByte, int startIndex = 0)
        {
            if (startIndex >= 0)
                for (int i = startIndex; i < data.Length; i++)
                    if (data[i] == targetByte)
                        return i;
            return -1;
        }


        /// <summary>
        /// Find the last occurrence of targetByte in data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int LastIndexOf(byte[] data, byte targetByte)
        {
            for (int i = data.Length - 1; i > 0; i--)
                if (data[i] == targetByte)
                    return i;
            return -1;
        }
    }
}
