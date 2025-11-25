// SmartRecordMatcher - Address Similarity WinForms sample
// Single-file sample for clarity. Paste into a new WinForms (.NET 6/7/8) project Program.cs
// Required NuGet packages: ClosedXML
// AppData/weights.json should be added to project and set Copy if newer

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace SmartRecordMatcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private Button btnLoadLeft;
        private Button btnLoadRight;
        private TextBox txtLeftPath;
        private TextBox txtRightPath;
        private Button btnCompute;
        private ProgressBar progressBar;
        private DataGridView dgvResults;
        private Label lblStatus;

        private List<RowRecord> leftRecords = new();
        private List<RowRecord> rightRecords = new();

        private AddressSimilarityEngine similarityEngine;

        public MainForm()
        {
            InitializeComponent();

            var storage = new StorageService();
            var config = storage.LoadWeightConfig();
            similarityEngine = new AddressSimilarityEngine(config);
        }

        private void InitializeComponent()
        {
            this.Text = "Address Similarity - SmartRecordMatcher";
            this.Size = new Size(1000, 700);

            btnLoadLeft = new Button() { Text = "Load File 1 (Left)", Location = new Point(20, 20), Size = new Size(150, 30) };
            btnLoadRight = new Button() { Text = "Load File 2 (Right)", Location = new Point(20, 60), Size = new Size(150, 30) };
            txtLeftPath = new TextBox() { Location = new Point(190, 20), Width = 760, ReadOnly = true };
            txtRightPath = new TextBox() { Location = new Point(190, 60), Width = 760, ReadOnly = true };
            btnCompute = new Button() { Text = "Compute Similarities", Location = new Point(20, 110), Size = new Size(200, 36) };
            progressBar = new ProgressBar() { Location = new Point(240, 110), Width = 400, Height = 36 };
            lblStatus = new Label() { Location = new Point(660, 110), Width = 290, Text = "Status: Ready" };

            dgvResults = new DataGridView() { Location = new Point(20, 160), Width = 930, Height = 480 };
            dgvResults.ReadOnly = true;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.RowHeadersVisible = false;

            this.Controls.Add(btnLoadLeft);
            this.Controls.Add(btnLoadRight);
            this.Controls.Add(txtLeftPath);
            this.Controls.Add(txtRightPath);
            this.Controls.Add(btnCompute);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblStatus);
            this.Controls.Add(dgvResults);

            btnLoadLeft.Click += BtnLoadLeft_Click;
            btnLoadRight.Click += BtnLoadRight_Click;
            btnCompute.Click += BtnCompute_Click;

            // Setup columns
            // Setup columns
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add("LeftId", "Left Id");
            dgvResults.Columns.Add("LeftAddress", "Left Address");
            dgvResults.Columns.Add("BestRightId", "Best Right Id");
            dgvResults.Columns.Add("BestRightAddress", "Best Right Address");
            dgvResults.Columns.Add("Similarity", "Similarity (%)");
            dgvResults.Columns.Add("Reason", "Reason");
        }

        private void BtnLoadLeft_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx;*.xls";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            txtLeftPath.Text = ofd.FileName;
            leftRecords = ExcelReaderService.ReadSimpleAddressFile(ofd.FileName);
            lblStatus.Text = $"Loaded {leftRecords.Count} left records";
        }

        private void BtnLoadRight_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx;*.xls";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            txtRightPath.Text = ofd.FileName;
            rightRecords = ExcelReaderService.ReadSimpleAddressFile(ofd.FileName);
            lblStatus.Text = $"Loaded {rightRecords.Count} right records";
        }

        private async void BtnCompute_Click(object sender, EventArgs e)
        {
            if (leftRecords == null || rightRecords == null || leftRecords.Count == 0 || rightRecords.Count == 0)
            {
                MessageBox.Show("Please load both files (Left and Right) first.", "Missing files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvResults.Rows.Clear();
            progressBar.Minimum = 0;
            progressBar.Maximum = leftRecords.Count;
            progressBar.Value = 0;
            lblStatus.Text = "Computing...";

            // Run computation on background thread
            var results = await Task.Run(() => ComputeAll(leftRecords, rightRecords));

            // Show only best matches (top 1)
            // Show only best matches (top 1)
            foreach (var r in results)
            {
                var bestId = r.BestMatch?.Id ?? "";
                var bestAddr = r.BestMatch?.OriginalAddress ?? "";
                dgvResults.Rows.Add(
                    r.Left.Id,
                    r.Left.OriginalAddress,
                    bestId,
                    bestAddr,
                    (r.BestScore * 100).ToString("F2"),
                    r.Reason ?? ""
                );
            }


            lblStatus.Text = $"Done. Processed {results.Count} left records.";
        }

        private List<ComparisonResult> ComputeAll(List<RowRecord> left, List<RowRecord> right)
        {
            var outList = new List<ComparisonResult>();
            int i = 0;
            foreach (var l in left)
            {
                ComparisonResult best = new() { Left = l, BestScore = 0.0, BestMatch = null, Reason = "" };
                foreach (var r in right)
                {
                    var score = similarityEngine.ComputeAddressSimilarity(l.Address, r.Address);
                    if (score > best.BestScore)
                    {
                        best.BestScore = score;
                        best.BestMatch = r;
                        best.Reason = similarityEngine.ExplainLastReason();
                    }
                }
                outList.Add(best);
                i++;
                this.Invoke((Action)(() => progressBar.Value = i));
            }
            return outList;
        }
    }

    #region Models
    public class RowRecord
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string OriginalAddress { get; set; }
    }

    public class ComparisonResult
    {
        public RowRecord Left { get; set; }          // یا AddressRecord اگر در پروژه‌ات آن نام را استفاده می‌کنی — نام را در همه جا یکسان کن
        public RowRecord BestMatch { get; set; }
        public double BestScore { get; set; }
        public string Reason { get; set; }
    }

    #endregion

    #region Excel Reader
    public static class ExcelReaderService
{
    // Expecting a simple sheet: first column = Id, second column = Address (header row optional)
    public static List<RowRecord> ReadSimpleAddressFile(string path)
    {
        var outList = new List<RowRecord>();
        using var wb = new XLWorkbook(path);
        var ws = wb.Worksheets.First();
        var firstRow = ws.FirstRowUsed();
        int startRow = firstRow.RowNumber();
        // Try to detect header: if first row contains non-numeric id, treat as header
        var maybeId = ws.Cell(startRow, 1).GetString();
        bool hasHeader = !Regex.IsMatch(maybeId, "^\\d+$");
        int r = hasHeader ? startRow + 1 : startRow;
        for (; r <= ws.LastRowUsed().RowNumber(); r++)
        {
            var id = ws.Cell(r, 1).GetString();
            var addr = ws.Cell(r, 2).GetString();
            if (string.IsNullOrWhiteSpace(id) && string.IsNullOrWhiteSpace(addr)) continue;
            outList.Add(new RowRecord { Id = id, Address = addr, OriginalAddress = addr });
        }
        return outList;
    }
}
#endregion

#region Storage & WeightConfig
public class WeightConfig
{
    // top-level weights for address composition (how to combine subcomponents)
    public double TokenSimilarityWeight { get; set; } = 0.5;
    public double StructuralWeight { get; set; } = 0.3;
    public double EditDistanceWeight { get; set; } = 0.2;

    // token importance dictionary (optional)
    public Dictionary<string, double> TokenImportance { get; set; } = new();

    // structural subweights
    public double CityWeight { get; set; } = 0.25;
    public double MainStreetWeight { get; set; } = 0.35;
    public double SubStreetWeight { get; set; } = 0.2;
    public double AlleyWeight { get; set; } = 0.1;
    public double NumberWeight { get; set; } = 0.1;
}

public class StorageService
{
    private readonly string weightFile;
    public StorageService()
    {
        var appPath = Application.StartupPath;
        var appDataDir = Path.Combine(appPath, "AppData");
        if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
        weightFile = Path.Combine(appDataDir, "weights.json");
        // if not exists, create default
        if (!File.Exists(weightFile))
        {
            var def = new WeightConfig();
            // seed some token importance examples
            def.TokenImportance["آزادی"] = 0.9;
            def.TokenImportance["انقلاب"] = 0.8;
            def.TokenImportance["وليعصر"] = 0.7;
            SaveWeightConfig(def);
        }
    }

    public WeightConfig LoadWeightConfig()
    {
        var json = File.ReadAllText(weightFile);
        return JsonSerializer.Deserialize<WeightConfig>(json) ?? new WeightConfig();
    }

    public void SaveWeightConfig(WeightConfig config)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(weightFile, JsonSerializer.Serialize(config, opts));
    }
}
    #endregion

    #region Address Similarity Engine
    public class AddressSimilarityEngine
    {
        private readonly WeightConfig weights;
        private readonly AddressNormalizer normalizer = new();
        private string lastReason = ""; // اضافه شد

        public AddressSimilarityEngine(WeightConfig weights)
        {
            this.weights = weights;
        }

        public double ComputeAddressSimilarity(string aRaw, string bRaw)
        {
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

            // ذخیره دلیل در lastReason
            lastReason = $"TokenSim={tokenSim:F2}, Structural={structuralSim:F2}, Edit={editSim:F2}";

            return Math.Clamp(combined, 0.0, 1.0);
        }

        // متد عمومی برای دریافت دلیل
        public string ExplainLastReason()
        {
            return lastReason;
        }

        private double TokenJaccardWeighted(List<string> aTokens, List<string> bTokens)
        {
            var sa = new HashSet<string>(aTokens);
            var sb = new HashSet<string>(bTokens);
            if (sa.Count == 0 && sb.Count == 0) return 1.0;
            if (sa.Count == 0 || sb.Count == 0) return 0.0;

            double intersectionWeight = 0.0;
            double unionWeight = 0.0;

            var all = new HashSet<string>(sa.Union(sb));
            foreach (var t in all)
            {
                double imp = 1.0;
                if (weights.TokenImportance != null && weights.TokenImportance.TryGetValue(t, out var v)) imp = v;
                bool inA = sa.Contains(t);
                bool inB = sb.Contains(t);
                if (inA || inB) unionWeight += imp;
                if (inA && inB) intersectionWeight += imp;
            }

            return unionWeight == 0 ? 0.0 : intersectionWeight / unionWeight;
        }

        private double StructuralSimilarity(List<string> aTokens, List<string> bTokens)
        {
            double score = 0.0;
            double maxScore = 0.0;

            if (aTokens.Count > 0 && bTokens.Count > 0)
            {
                maxScore += weights.CityWeight;
                if (aTokens[0] == bTokens[0]) score += weights.CityWeight;
            }

            var aNonNum = aTokens.Where(t => !IsNumericToken(t)).ToList();
            var bNonNum = bTokens.Where(t => !IsNumericToken(t)).ToList();
            double mainMatch = 0.0;
            foreach (var t in aNonNum)
            {
                if (bNonNum.Contains(t)) mainMatch = Math.Max(mainMatch, 1.0);
            }
            maxScore += weights.MainStreetWeight;
            score += mainMatch * weights.MainStreetWeight;

            var aSub = aNonNum.Skip(1).ToList();
            var bSub = bNonNum.Skip(1).ToList();
            double subMatch = 0.0;
            if (aSub.Count > 0 && bSub.Count > 0)
            {
                var common = aSub.Intersect(bSub).Count();
                subMatch = Math.Min(1.0, (double)common / Math.Max(1, Math.Min(aSub.Count, bSub.Count)));
            }
            maxScore += weights.SubStreetWeight;
            score += subMatch * weights.SubStreetWeight;

            var aNum = aTokens.FirstOrDefault(t => IsNumericToken(t));
            var bNum = bTokens.FirstOrDefault(t => IsNumericToken(t));
            maxScore += weights.NumberWeight;
            if (!string.IsNullOrEmpty(aNum) && !string.IsNullOrEmpty(bNum) && aNum == bNum) score += weights.NumberWeight;

            maxScore += weights.AlleyWeight;
            var alleyKeywords = new[] { "کوچه", "کوچ", "بن بست", "بن‌بست", "پلاک" };
            bool aHasAlley = aTokens.Any(t => alleyKeywords.Contains(t));
            bool bHasAlley = bTokens.Any(t => alleyKeywords.Contains(t));
            if (aHasAlley && bHasAlley) score += weights.AlleyWeight;

            double denom = weights.CityWeight + weights.MainStreetWeight + weights.SubStreetWeight + weights.AlleyWeight + weights.NumberWeight;
            return denom == 0 ? 0.0 : score / denom;
        }

        private static bool IsNumericToken(string t) => Regex.IsMatch(t, @"^\d+$");

        private double EditSimilarity(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) && string.IsNullOrWhiteSpace(b)) return 1.0;
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return 0.0;
            double jw = JaroWinklerDistance(a, b);
            double lev = 1.0 - ((double)LevenshteinDistance(a, b) / Math.Max(a.Length, b.Length));
            return (jw * 0.6) + (lev * 0.4);
        }

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
    #endregion

    #region Address Normalizer
    public class AddressNormalizer
{
    // Basic normalization for Persian addresses
    private static readonly Dictionary<char, char> CharMap = new()
    {
        ['ي'] = 'ی',
        ['ك'] = 'ک'
    };

    private static readonly string[] StopWords = new[] {
            "پلاک","پلاک:","واحد","طبقه","خیابان","خیابان:","خیابان‌ها","بلوار","کوچه","کوچه:","کوی","بن‌بست","بن‌بست:" };

    public string Normalize(string raw)
    {
        if (raw == null) return "";
        var s = raw.Trim().ToLowerInvariant();
        // map arabic y/k to persian
        s = new string(s.Select(c => CharMap.ContainsKey(c) ? CharMap[c] : c).ToArray());
        // remove punctuation
        s = Regex.Replace(s, "[\\-_,;:\\(\\)\\\\/]+", " ");
        // normalize whitespace
        s = Regex.Replace(s, "\\s+", " ").Trim();
        // convert persian digits to latin digits
        s = PersianDigitsToEnglish(s);
        return s;
    }

    public List<string> Tokenize(string normalized)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(normalized)) return tokens;

        var parts = Regex.Split(normalized, "\\s+");
        foreach (var p in parts)
        {
            var t = p.Trim();
            if (string.IsNullOrEmpty(t)) continue;
            // remove common stopwords but keep their info (we may want to detect presence)
            tokens.Add(t);
        }
        return tokens;
    }

    private string PersianDigitsToEnglish(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c >= 0x06F0 && c <= 0x06F9)
            {
                sb.Append((char)('0' + (c - 0x06F0)));
            }
            else if (c >= 0x0660 && c <= 0x0669)
            {
                sb.Append((char)('0' + (c - 0x0660)));
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
    #endregion
}
