using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartRecordMatcher.Models;
using SmartRecordMatcher.Services;
using SmartRecordMatcher.Utilities;

namespace SmartRecordMatcher.Engine
{
    public class AddressSimilarityEnginePro
    {
        private readonly WeightConfig cfg;
        private readonly AddressNormalizer normalizer = new();
        private readonly AddressParser parser = new();
        public string LastReason { get; private set; } = "";

        public AddressSimilarityEnginePro(WeightConfig config)
        {
            cfg = config ?? new WeightConfig();
        }


        public double Compute(string aRaw, string bRaw)
        {
            LastReason = "";

            var aNorm = normalizer.Normalize(aRaw ?? "");
            var bNorm = normalizer.Normalize(bRaw ?? "");

            var aTokens = normalizer.Tokenize(aNorm);
            var bTokens = normalizer.Tokenize(bNorm);

            var aNums = aTokens.Where(t => Regex.IsMatch(t, @"^\d+$")).ToList();
            var bNums = bTokens.Where(t => Regex.IsMatch(t, @"^\d+$")).ToList();

            // structural: parse fields and compare
            var fa = parser.Parse(aRaw ?? "");
            var fb = parser.Parse(bRaw ?? "");
            double structSim = StructuralSimilarity(fa, fb, aTokens, bTokens);

            // token similarities
            double tokenJ = TokenJaccardWeighted(aTokens, bTokens);
            double tokenSet = TokenSetRatio(aTokens, bTokens);
            double tokenSim = Math.Max(tokenJ, tokenSet);

            // edit/sequence similarity
            double editSim = EditSimilarityPro(aNorm, bNorm, aTokens, bTokens);

            // phonetic
            double phonetic = PhoneticSimilarity(aTokens, bTokens);

            // numeric proximity (plaque)
            double numeric = NumericTokenSim(aNums, bNums);

            // combine with cfg top-level weights
            double wStruct = cfg.StructuralWeight;
            double wToken = cfg.TokenWeight;
            double wEdit = cfg.EditWeight;
            // normalize
            var sum = wStruct + wToken + wEdit;
            if (sum <= 0) { wStruct = 0.45; wToken = 0.35; wEdit = 0.2; sum = wStruct + wToken + wEdit; }
            wStruct /= sum; wToken /= sum; wEdit /= sum;

            // final: add some small contributions from phonetic and numeric inside token/edit
            double combined = (wStruct * structSim) + (wToken * tokenSim) + (wEdit * editSim);
            // bump by phonetic/numeric small bonuses
            combined = Math.Min(1.0, combined + 0.05 * phonetic + 0.05 * numeric);

            LastReason = $"Struct:{structSim:F2},Token:{tokenSim:F2}(J:{tokenJ:F2},TS:{tokenSet:F2}),Edit:{editSim:F2},Ph:{phonetic:F2},Num:{numeric:F2}";
            return Math.Clamp(combined, 0.0, 1.0);
        }

        // ---------- Structural similarity ----------
        private double StructuralSimilarity(AddressFields a, AddressFields b, List<string> aTokens, List<string> bTokens)
        {
            double s = 0.0;
            double wsum = 0.0;
            var fw = cfg.Fields;

            // city
            wsum += fw.City;
            s += fw.City * FieldStringSim(a.City, b.City);

            // region
            wsum += fw.Region;
            s += fw.Region * FieldStringSim(a.Region, b.Region);

            // street
            wsum += fw.Street;
            s += fw.Street * StreetSim(a.Street, b.Street);

            // substreet
            wsum += fw.SubStreet;
            s += fw.SubStreet * FieldStringSim(a.SubStreet, b.SubStreet);

            // alley
            wsum += fw.Alley;
            s += fw.Alley * FieldStringSim(a.Alley, b.Alley);

            // plaque
            wsum += fw.Plaque;
            s += fw.Plaque * NumericTokenSingleSim(a.Plaque, b.Plaque);

            // unit
            wsum += fw.Unit;
            s += fw.Unit * NumericTokenSingleSim(a.Unit, b.Unit);

            return wsum == 0 ? 0.0 : s / wsum;
        }

        private double FieldStringSim(string x, string y)
        {
            if (string.IsNullOrWhiteSpace(x) && string.IsNullOrWhiteSpace(y)) return 1.0;
            if (string.IsNullOrWhiteSpace(x) || string.IsNullOrWhiteSpace(y)) return 0.0;
            return JaroWinklerDistance(x, y);
        }

        private double StreetSim(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return 1.0;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0.0;
            var at = normalizer.Tokenize(a);
            var bt = normalizer.Tokenize(b);
            double jw = JaroWinklerDistance(a, b);
            double ts = TokenSetRatio(at, bt);
            return 0.6 * jw + 0.4 * ts;
        }

        // ---------- Token helpers ----------
        private double TokenJaccardWeighted(List<string> aTokens, List<string> bTokens)
        {
            var sa = new HashSet<string>(aTokens);
            var sb = new HashSet<string>(bTokens);
            if (sa.Count == 0 && sb.Count == 0) return 1.0;
            if (sa.Count == 0 || sb.Count == 0) return 0.0;

            double interW = 0.0, unionW = 0.0;
            var all = new HashSet<string>(sa.Union(sb));
            foreach (var t in all)
            {
                double imp = 1.0;
                if (cfg.TokenImportance != null && cfg.TokenImportance.TryGetValue(t, out var v)) imp = Math.Max(0.01, v);
                bool inA = sa.Contains(t), inB = sb.Contains(t);
                if (inA || inB) unionW += imp;
                if (inA && inB) interW += imp;
            }
            return unionW == 0 ? 0.0 : interW / unionW;
        }

        private double TokenSetRatio(List<string> aTokens, List<string> bTokens)
        {
            string a = string.Join(" ", aTokens);
            string b = string.Join(" ", bTokens);
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return 1.0;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0.0;

            var setA = new HashSet<string>(aTokens);
            var setB = new HashSet<string>(bTokens);

            var common = setA.Intersect(setB).OrderBy(x => x).ToArray();
            var onlyA = setA.Except(setB).OrderBy(x => x).ToArray();
            var onlyB = setB.Except(setA).OrderBy(x => x).ToArray();

            string sortedCommon = string.Join(" ", common);
            string sortedA = string.Join(" ", common.Concat(onlyA));
            string sortedB = string.Join(" ", common.Concat(onlyB));

            double r1 = SimpleStringRatio(sortedCommon, sortedA);
            double r2 = SimpleStringRatio(sortedCommon, sortedB);
            double r3 = SimpleStringRatio(sortedA, sortedB);

            return Math.Max(r1, Math.Max(r2, r3));
        }

        private double SimpleStringRatio(string s1, string s2)
        {
            if (string.IsNullOrWhiteSpace(s1) && string.IsNullOrWhiteSpace(s2)) return 1.0;
            if (string.IsNullOrWhiteSpace(s1) || string.IsNullOrWhiteSpace(s2)) return 0.0;
            double jw = JaroWinklerDistance(s1, s2);
            int lev = LevenshteinDistance(s1, s2);
            double levSim = 1.0 - ((double)lev / Math.Max(1, Math.Max(s1.Length, s2.Length)));
            return 0.6 * jw + 0.4 * levSim;
        }

        // phonetic
        private double PhoneticSimilarity(List<string> aTokens, List<string> bTokens)
        {
            if (aTokens.Count == 0 || bTokens.Count == 0) return 0.0;
            var encA = aTokens.Select(t => PhoneticEncoder.Encode(t)).Where(x => !string.IsNullOrEmpty(x)).ToList();
            var encB = bTokens.Select(t => PhoneticEncoder.Encode(t)).Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (encA.Count == 0 || encB.Count == 0) return 0.0;

            int matches = 0;
            foreach (var ea in encA)
            {
                if (encB.Contains(ea)) matches++;
            }
            return (double)matches / Math.Max(1, Math.Min(encA.Count, encB.Count));
        }

        // numeric similarity for lists of numeric tokens
        private double NumericTokenSim(List<string> aNums, List<string> bNums)
        {
            if (aNums.Count == 0 && bNums.Count == 0) return 1.0;
            if (aNums.Count == 0 || bNums.Count == 0) return 0.0;

            int best = 0;
            foreach (var an in aNums)
            {
                foreach (var bn in bNums)
                {
                    if (an == bn) best++;
                    else
                    {
                        if (int.TryParse(an, out var ai) && int.TryParse(bn, out var bi))
                        {
                            var diff = Math.Abs(ai - bi);
                            if (diff == 0) best++;
                        }
                    }
                }
            }
            return (double)best / Math.Max(1, Math.Min(aNums.Count, bNums.Count));
        }

        // numeric single tokens (plaque/unit)
        private double NumericTokenSingleSim(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return 1.0;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0.0;
            if (a == b) return 1.0;
            if (int.TryParse(a, out var ai) && int.TryParse(b, out var bi))
            {
                var diff = Math.Abs(ai - bi);
                if (diff == 0) return 1.0;
                if (diff <= 2) return 0.6;
                if (diff <= 10) return 0.3;
                return 0.0;
            }
            return 0.0;
        }

        // EditSimilarityPro: combo JaroWinkler + TokenSet + Levenshtein
        private double EditSimilarityPro(string a, string b, List<string> aTokens, List<string> bTokens)
        {
            double jw = JaroWinklerDistance(a, b);
            double ts = TokenSetRatio(aTokens, bTokens);
            double levSim = 1.0 - ((double)LevenshteinDistance(a, b) / Math.Max(1, Math.Max(a.Length, b.Length)));
            return 0.4 * jw + 0.4 * ts + 0.2 * levSim;
        }

        // -------------------- classic helpers --------------------
        public static int LevenshteinDistance(string s, string t)
        {
            if (s == null) s = "";
            if (t == null) t = "";
            int n = s.Length, m = t.Length;
            if (n == 0) return m;
            if (m == 0) return n;
            var d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        public static double JaroWinklerDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return 1.0;
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;
            var jaro = JaroDistance(s1, s2);
            int prefix = 0;
            int maxPrefix = Math.Min(4, Math.Min(s1.Length, s2.Length));
            for (int i = 0; i < maxPrefix; i++)
            {
                if (s1[i] == s2[i]) prefix++; else break;
            }
            double scaling = 0.1;
            return jaro + (prefix * scaling * (1 - jaro));
        }

        public static double JaroDistance(string s1, string s2)
        {
            int s1Len = s1.Length;
            int s2Len = s2.Length;
            if (s1Len == 0) return s2Len == 0 ? 1 : 0;
            int matchDistance = Math.Max(s1Len, s2Len) / 2 - 1;
            var s1Matches = new bool[s1Len];
            var s2Matches = new bool[s2Len];
            int matches = 0;
            for (int i = 0; i < s1Len; i++)
            {
                int start = Math.Max(0, i - matchDistance);
                int end = Math.Min(i + matchDistance + 1, s2Len);
                for (int j = start; j < end; j++)
                {
                    if (s2Matches[j]) continue;
                    if (s1[i] != s2[j]) continue;
                    s1Matches[i] = true;
                    s2Matches[j] = true;
                    matches++;
                    break;
                }
            }
            if (matches == 0) return 0.0;
            double t = 0.0;
            int k = 0;
            for (int i = 0; i < s1Len; i++)
            {
                if (!s1Matches[i]) continue;
                while (!s2Matches[k]) k++;
                if (s1[i] != s2[k]) t += 0.5;
                k++;
            }
            double m = matches;
            return ((m / s1Len) + (m / s2Len) + ((m - t) / m)) / 3.0;
        }
    }
}
