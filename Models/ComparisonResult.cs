using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRecordMatcher.Models
{
    public class ComparisonResult
    {
        public RowRecord Left { get; set; }
        public RowRecord BestMatch { get; set; }
        public double BestScore { get; set; }
        public string Reason { get; set; }
    }

}
