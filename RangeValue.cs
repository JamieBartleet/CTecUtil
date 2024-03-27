using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTecUtil
{
    /// <summary>
    /// Specifies an integer range object with Min, Max and Default values.
    /// </summary>
    public class RangeValue
    {
        public RangeValue(int min, int max, int defaultValue) { Min = min; Max = max; Default = defaultValue; }
        public int Min { get; set; }
        public int Max { get; set; }
        public int Default { get; set; }
    }
}
