using SmartRecordMatcher;
using SmartRecordMatcher.Models;
using SmartRecordMatcher.Utilities;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class AddressSimilarityEngine
{
    private readonly WeightConfig weights;
    private readonly AddressNormalizer normalizer = new();

    public string LastReason { get; private set; } = "";

    public AddressSimilarityEngine(WeightConfig weights)
    {
        this.weights = weights;
    }

    public double ComputeAddressSimilarity(string aRaw, string bRaw)
    {
        LastReason = "";

        var a = normalizer.Normalize(aRaw ?? "");
        var b = normalizer.Normalize(bRaw ?? "");

        var aTokens = normalizer.Tokenize(a);
        var bTokens = normalizer.Tokenize(b);

        double tokenSim = TokenJaccardWeighted(aTokens, bTokens);
        double structuralSim = StructuralSimilarity(aTokens, bTokens);
        double editSim = EditSimilarity(a, b);

        double combined =
            weights.TokenSimilarityWeight * tokenSim +
            weights.StructuralWeight * structuralSim +
            weights.EditDistanceWeight * editSim;

        LastReason = $"Token:{tokenSim:F2}, Struct:{structuralSim:F2}, Edit:{editSim:F2}";

        return Math.Clamp(combined, 0.0, 1.0);
    }

    private double TokenJaccardWeighted(List<string> aTokens, List<string> bTokens)
    {
        var setA = new HashSet<string>(aTokens);
        var setB = new HashSet<string>(bTokens);

        if (setA.Count == 0 && setB.Count == 0) return 1.0;
        if (setA.Count == 0 || setB.Count == 0) return 0.0;

        double intersection = setA.Intersect(setB).Count();
        double union = setA.Union(setB).Count();

        return intersection / union;
    }

    private double StructuralSimilarity(List<string> aTokens, List<string> bTokens)
    {
        // نمونه ساده: اگر اولین توکن (مثلاً شهر یا خیابان اصلی) یکی باشد، 0.5، در غیر اینصورت 0
        if (aTokens.Count > 0 && bTokens.Count > 0 && aTokens[0] == bTokens[0])
            return 0.5;
        return 0.0;
    }

    private double EditSimilarity(string a, string b)
    {
        // نمونه ساده: درصد شباهت بر اساس Levenshtein
        int lev = LevenshteinDistance(a, b);
        int maxLen = Math.Max(a.Length, b.Length);
        if (maxLen == 0) return 1.0;
        return 1.0 - ((double)lev / maxLen);
    }

    // تابع Levenshtein
    public static int LevenshteinDistance(string s, string t)
    {
        if (s == null) s = "";
        if (t == null) t = "";
        var n = s.Length;
        var m = t.Length;
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
}
