using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CTecUtil
{
    public class Ranges
    {
        /// <summary>
        /// Set the Equation according to the parsed string, which will contain int-based equation elements, i.e. group or device numbers.<br/>
        /// E.g. "1-4, 7, 13-15" would parse to 1,2,3,4,7,13,14,15
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns>Sorted list</returns>
        public static List<int> ParseIntList(string stringValue)
        {
            if (!checkValidTokens(stringValue, '0', '9'))
                return null;

            List<int> result = new();

            foreach (var seg in splitSegments(stringValue))
            {
                var range = seg.Split("-", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (range.Length == 0)
                    continue;

                int rangeStart, rangeEnd;

                if (int.TryParse(range[0], out rangeStart))
                {
                    if (range.Length > 1)
                        int.TryParse(range[1], out rangeEnd);
                    else
                        rangeEnd = rangeStart;

                    if (rangeEnd >= rangeStart)
                        for (int i = rangeStart; i <= rangeEnd; i++)
                            result.Add(i);
                }
            }

            return NormaliseIntList(result);
        }


        /// <summary>
        /// Set the Equation according to the parsed string, which will contain character-based equation elements, i.e. area codes.<br/>
        /// E.g. "A-D, G, M-O" would parse to A,B,C,D,G,M,N,O
        /// </summary>
        /// <param name="stringValue"></param>          
        /// <param name="rangeList">A list of char ranges included in the equation</param>
        /// <returns></returns>
        public List<char> ParseCharList(string stringValue)
        {
            if (!checkValidTokens(stringValue, 'A', 'Z'))
            {
                return null;
            }

            List<char> result = new();

            foreach (var segment in splitSegments(stringValue))
            {
                var seg = segment;

                while (seg.Contains("-"))
                {
                    //strip any initial "-" and contained "--"
                    while (seg.StartsWith("-"))
                        seg = seg.Substring(1);

                    int idxDashDash;
                    while ((idxDashDash = seg.IndexOf("--")) > 0)
                        seg = seg.Remove(idxDashDash, 1);

                    if (!seg.Contains("-"))
                        break;

                    //get start of range
                    int idxDash = seg.IndexOf('-');
                    char rangeStart = seg[idxDash-1];
                    char rangeEnd = rangeStart;

                    //is there an end of range?
                    if (idxDash < seg.Length - 1)
                    {
                        rangeEnd = idxDash < seg.Length - 1 ? seg[idxDash + 1] : rangeStart;

                        if (rangeStart <= rangeEnd)
                            for (char c = rangeStart; c <= rangeEnd; c++)
                                result.Add(c);
                        else
                            for (char c = rangeStart; c <= rangeStart; c++)
                                result.Add(c);
                    }
                    else
                    {
                        result.Add(rangeStart);
                    }

                    if (rangeEnd >= rangeStart)
                        seg = seg.Substring(idxDash + 1);
                }

                foreach (var c in seg)
                    result.Add(c);
            }

            if (result.Count == 0 && !string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            return NormaliseCharList(result);
        }


        private static bool checkValidTokens(string stringValue, char lowLimit, char highLimit)
        {
            foreach (var _ in from c in stringValue
                              where (c < lowLimit || c > highLimit) && c != ','  && c != '-' && c != ' '
                              select new { })
                return false;

            return true;
        }

        private static string[] splitSegments(string stringValue) => stringValue.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);


        /// <summary>
        /// Removes any Duplicates and the result is sorted.
        /// </summary>
        internal static List<int> NormaliseIntList(List<int> list)
        {
            List<int> result = new();

            list.Sort();

            //copy non-duplicate valid values into result
            foreach (var e in list)
                if (!result.Contains(e))
                    result.Add(e);

            return result;
        }


        /// <summary>
        /// Removes any Duplicates and the result is sorted.
        /// </summary>
        internal static List<char> NormaliseCharList(List<char> list)
        {
            List<char> result = new();

            list.Sort();

            //copy non-duplicate valid values into result
            foreach (var e in list)
                if (!result.Contains(e))
                    result.Add(e);

            return result;
        }
    }
}
