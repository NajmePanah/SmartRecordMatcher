using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartRecordMatcher.Models;
using SmartRecordMatcher.Utilities; // فرض: AddressNormalizer در این namespace است

namespace SmartRecordMatcher.Engine
{
    public class AddressSimilarityEngine
    {
        private readonly WeightConfig cfg;
        private readonly AddressNormalizer normalizer = new();
        public string LastReason { get; private set; } = "";

        // fallback weights (در صورت نبودن مقادیر در WeightConfig)
        private readonly double defaultStructuralWeight = 0.45;
        private readonly double defaultTokenWeight = 0.35;
        private readonly double defaultEditWeight = 0.20;

        public AddressSimilarityEngine(WeightConfig config)
        {
            cfg = config ?? new WeightConfig();
        }

        public double Compute(string leftRaw, string rightRaw)
        {
            LastReason = "";

            var aNorm = normalizer.Normalize(leftRaw ?? "");
            var bNorm = normalizer.Normalize(rightRaw ?? "");

            var aTokens = normalizer.Tokenize(aNorm);
            var bTokens = normalizer.Tokenize(bNorm);

            // numeric tokens (plaque / unit / any digits)
            var aNums = ExtractNumericTokens(aTokens);
            var bNums = ExtractNumericTokens(bTokens);

            // 1) Token similarity (weighted Jaccard)
            double tokenJ = TokenJaccardWeighted(aTokens, bTokens);

            // 2) Structural heuristics
            double structSim = StructuralSimilarity(aTokens, bTokens, aNums, bNums);

            // 3) Edit/sequence similarity (hybrid)
            double editSim = EditSimilarityPro(aNorm, bNorm, aTokens, bTokens);

            // Combine weights: try to use cfg properties if available, otherwise default
            double structuralWeight = GetCfgDouble(nameof(cfg.StructuralWeight), defaultStructuralWeight);
            double tokenWeight = GetCfgDouble(nameof(cfg.TokenWeight), defaultTokenWeight);
            double editWeight = GetCfgDouble(nameof(cfg.EditWeight), defaultEditWeight);

            // normalize top-level weights sum to 1
            double sumW = structuralWeight + tokenWeight + editWeight;
            if (sumW <= 0) { structuralWeight = defaultStructuralWeight; tokenWeight = defaultTokenWeight; editWeight = defaultEditWeight; sumW = structuralWeight + tokenWeight + editWeight; }
            structuralWeight /= sumW; tokenWeight /= sumW; editWeight /= sumW;

            double combined = structuralWeight * structSim + tokenWeight * tokenJ + editWeight * editSim;
            LastReason = $"Struct:{structSim:F2}, Token:{tokenJ:F2}, Edit:{editSim:F2}";

            return Math.Clamp(combined, 0.0, 1.0);
        }

        // --- helpers ---

        private double GetCfgDouble(string propName, double @default)
        {
            try
            {
                var prop = cfg.GetType().GetProperty(propName);
                if (prop != null)
                {
                    var v = prop.GetValue(cfg);
                    if (v is double d) return d;
                    if (v is float f) return f;
                    if (v is int i) return i;
                }
            }
            catch { /* ignore */ }
            return @default;
        }

        private List<string> ExtractNumericTokens(List<string> tokens)
        {
            return tokens.Where(t => Regex.IsMatch(t, @"^\d+$")).ToList();
        }

        // ----- Token weighted Jaccard (با حمایت از TokenImportance در cfg اگر موجود باشد) -----
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
                try
                {
                    var pi = cfg.GetType().GetProperty("TokenImportance");
                    if (pi != null)
                    {
                        var map = pi.GetValue(cfg) as IDictionary<string, double>;
                        if (map != null && map.TryGetValue(t, out var vv)) imp = Math.Max(0.01, vv);
                    }
                }
                catch { /* ignore */ }

                bool inA = sa.Contains(t), inB = sb.Contains(t);
                if (inA || inB) unionW += imp;
                if (inA && inB) interW += imp;
            }
            return unionW == 0 ? 0.0 : interW / unionW;
        }

        // ----- Structural similarity: اعداد (پلاک)، کلیدواژه‌ها، ترتیب توکن -----
        private double StructuralSimilarity(List<string> aTokens, List<string> bTokens, List<string> aNums, List<string> bNums)
        {
            double score = 0.0;
            double weightSum = 0.0;

            // 1) exact numeric (plaque) match — مهم
            double plaqueWeight = 1.5;
            weightSum += plaqueWeight;
            if (aNums.Count > 0 && bNums.Count > 0 && aNums.Intersect(bNums).Any()) score += plaqueWeight;

            // 2) first token match (often city or main area)
            double firstTokenWeight = 1.0;
            weightSum += firstTokenWeight;
            if (aTokens.Count > 0 && bTokens.Count > 0 && aTokens[0] == bTokens[0]) score += firstTokenWeight;

            // 3) presence of street keywords both sides (e.g. خیابان/بلوار/کوچه)
            double keywordWeight = 1.0;
            weightSum += keywordWeight;
            var keywords = new[] { "خیابان", "بلوار", "کوچه", "میدان", "خیابانها", "کوی" };
            bool aHas = aTokens.Any(t => keywords.Contains(t));
            bool bHas = bTokens.Any(t => keywords.Contains(t));
            if (aHas && bHas) score += keywordWeight;

            // 4) longest common token subsequence (order-insensitive)
            double lctWeight = 1.0;
            weightSum += lctWeight;
            var commons = aTokens.Intersect(bTokens).Count();
            double lctSim = Math.Min(1.0, (double)commons / Math.Max(1, Math.Min(aTokens.Count, bTokens.Count)));
            score += lctWeight * lctSim;

            // 5) token order similarity (penalize when all tokens same but order different moderately)
            double orderWeight = 0.5;
            weightSum += orderWeight;
            double orderSim = SequenceOrderSimilarity(aTokens, bTokens);
            score += orderWeight * orderSim;

            return weightSum == 0 ? 0.0 : (score / weightSum);
        }

        // مقایسه ترتیب توکن‌ها به صورت ساده (نسبت توکن‌هایی که در همان ترتیب نسبی هستند)
        private double SequenceOrderSimilarity(List<string> a, List<string> b)
        {
            if (a.Count == 0 || b.Count == 0) return 0.0;
            int matched = 0;
            int k = 0;
            for (int i = 0; i < a.Count && k < b.Count; i++)
            {
                for (int j = k; j < b.Count; j++)
                {
                    if (a[i] == b[j])
                    {
                        matched++;
                        k = j + 1;
                        break;
                    }
                }
            }
            return (double)matched / Math.Max(1, Math.Min(a.Count, b.Count));
        }

        // ----- EditSimilarityPro: ترکیبی از Jaro-Winkler، TokenSetRatio و Levenshtein -----
        private double EditSimilarityPro(string a, string b, List<string> aTokens, List<string> bTokens)
        {
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return 1.0;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0.0;

            double jw = JaroWinklerDistance(a, b);
            double ts = TokenSetRatio(aTokens, bTokens);
            double lev = 1.0 - ((double)LevenshteinDistance(a, b) / Math.Max(1, Math.Max(a.Length, b.Length)));

            // اوزان داخلی
            double wJ = 0.4, wTS = 0.4, wLev = 0.2;
            return (jw * wJ) + (ts * wTS) + (lev * wLev);
        }

        // TokenSetRatio (شبیه fuzzywuzzy)
        private double TokenSetRatio(List<string> aT, List<string> bT)
        {
            var setA = new HashSet<string>(aT);
            var setB = new HashSet<string>(bT);
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

        // ---------------- classic helpers ----------------
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
