using System.Collections.Generic;

namespace SmartRecordMatcher.Models
{
    public class WeightConfig
    {
        // top level
        public double StructuralWeight { get; set; } = 0.45;
        public double TokenWeight { get; set; } = 0.35;
        public double EditWeight { get; set; } = 0.20;

        // field-level weights for structural layer
        public FieldWeights Fields { get; set; } = new FieldWeights();

        // optional importance map for tokens
        public Dictionary<string, double> TokenImportance { get; set; } = new Dictionary<string, double>();
    }

    public class FieldWeights
    {
        public double City { get; set; } = 0.15;
        public double Region { get; set; } = 0.10;
        public double Street { get; set; } = 0.35;
        public double SubStreet { get; set; } = 0.10;
        public double Alley { get; set; } = 0.10;
        public double Plaque { get; set; } = 0.10;
        public double Unit { get; set; } = 0.05;
    }
}
