using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRecordMatcher.Models
{
    namespace SmartRecordMatcher.Models
    {
        public class WeightConfig
        {
            // -------- Top level weights --------
            public double StructuralWeight { get; set; } = 0.4;
            public double TokenWeight { get; set; } = 0.4;
            public double EditWeight { get; set; } = 0.2;

            // -------- Field-level structure weights --------
            public FieldWeights Fields { get; set; } = new();

            // -------- Token importance map --------
            public Dictionary<string, double> TokenImportance { get; set; } = new();
        }

        public class FieldWeights
        {
            public double City { get; set; } = 0.2;
            public double Region { get; set; } = 0.15;
            public double Street { get; set; } = 0.25;
            public double Alley { get; set; } = 0.15;
            public double Plaque { get; set; } = 0.15;
            public double Unit { get; set; } = 0.10;
        }
    }


}
