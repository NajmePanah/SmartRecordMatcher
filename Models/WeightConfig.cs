using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRecordMatcher.Models
{
    public class WeightConfig
    {
        public double TokenSimilarityWeight { get; set; } = 0.4;
        public double StructuralWeight { get; set; } = 0.4;
        public double EditDistanceWeight { get; set; } = 0.2;

        public double CityWeight { get; set; } = 0.2;
        public double MainStreetWeight { get; set; } = 0.3;
        public double SubStreetWeight { get; set; } = 0.2;
        public double AlleyWeight { get; set; } = 0.1;
        public double NumberWeight { get; set; } = 0.2;

        public Dictionary<string, double> TokenImportance { get; set; } = new();
    }

}
