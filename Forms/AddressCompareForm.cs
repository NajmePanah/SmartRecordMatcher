using SmartRecordMatcher.Engine;
using SmartRecordMatcher.Models;
using SmartRecordMatcher.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartRecordMatcher.Forms
{
    public partial class AddressCompareForm : Form
    {
        // data
        private List<RowRecord> leftRecords = new();
        private List<RowRecord> rightRecords = new();

        // step state
        private int currentLeftIndex = 0;
        private int topN = 10;

        // engine & config
        private WeightConfig cfg;
        private AddressSimilarityEnginePro engine;

        // precomputed grouped results (optional caching)
        private List<ComparisonBundle> groupedResults = new();

        public AddressCompareForm()
        {
            InitializeComponent();

            // wire event handlers (designer file does NOT attach clicks)
            btnLoadLeft.Click += BtnLoadLeft_Click;
            btnLoadRight.Click += BtnLoadRight_Click;
            btnCompute.Click += BtnCompute_Click;
            btnNext.Click += BtnNext_Click;

            // try load config
            try
            {
                cfg = new StorageService().LoadWeightConfig() ?? new WeightConfig();
            }
            catch
            {
                cfg = new WeightConfig();
            }

            engine = new AddressSimilarityEnginePro(cfg);
        }

        // -------------------------
        // Load left file
        // -------------------------
        private void BtnLoadLeft_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx;*.xls|CSV Files|*.csv|All Files|*.*";
            ofd.Title = "Select left file (records to match)";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            txtLeftPath.Text = ofd.FileName;
            try
            {
                leftRecords = ExcelReaderService.ReadSimpleAddressFile(ofd.FileName) ?? new List<RowRecord>();
                lblStatus.Text = $"Loaded LEFT: {leftRecords.Count} rows";
                currentLeftIndex = 0;
                groupedResults.Clear();
                btnNext.Enabled = false;
                lblCurrentLeft.Text = "Current Left Record: -";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read left file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -------------------------
        // Load right file
        // -------------------------
        private void BtnLoadRight_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Excel Files|*.xlsx;*.xls|CSV Files|*.csv|All Files|*.*";
            ofd.Title = "Select right file (candidate records)";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            txtRightPath.Text = ofd.FileName;
            try
            {
                rightRecords = ExcelReaderService.ReadSimpleAddressFile(ofd.FileName) ?? new List<RowRecord>();
                lblStatus.Text = $"Loaded RIGHT: {rightRecords.Count} rows";
                groupedResults.Clear();
                btnNext.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read right file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -------------------------
        // Compute all groups (async)
        // -------------------------
        private async void BtnCompute_Click(object sender, EventArgs e)
        {
            if (leftRecords == null || leftRecords.Count == 0)
            {
                MessageBox.Show("Please load the left file first.", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (rightRecords == null || rightRecords.Count == 0)
            {
                MessageBox.Show("Please load the right file first.", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // prepare UI
            dgvResults.Rows.Clear();
            dgvResults.Columns.Clear();
            dgvResults.Columns.Add("RightID", "Right ID");
            dgvResults.Columns.Add("RightAddr", "Right Address");
            dgvResults.Columns.Add("Score", "Similarity (%)");
            dgvResults.Columns.Add("Reason", "Reason");

            AdjustGridColumns();

            progressBar.Minimum = 0;
            progressBar.Maximum = leftRecords.Count;
            progressBar.Value = 0;
            lblStatus.Text = "Computing similarity groups... (this may take a while)";

            // compute in background
            groupedResults = await Task.Run(() => ComputeGrouped(leftRecords, rightRecords));

            lblStatus.Text = $"Computed {groupedResults.Count} groups.";
            currentLeftIndex = 0;
            btnNext.Enabled = groupedResults.Count > 0;
            ShowCurrentLeftRecord();
        }

        // -------------------------
        // Compute grouped results (synchronous - called inside Task.Run)
        // -------------------------
        private List<ComparisonBundle> ComputeGrouped(List<RowRecord> left, List<RowRecord> right)
        {
            var list = new List<ComparisonBundle>();
            int total = left.Count;
            int i = 0;

            // local engine instance to avoid cross-thread issues with LastReason
            var localEngine = new AddressSimilarityEnginePro(cfg);

            foreach (var l in left)
            {
                var scores = new List<ComparisonResult>();

                foreach (var r in right)
                {
                    double sc = localEngine.Compute(l.OriginalAddress ?? "", r.OriginalAddress ?? "");
                    scores.Add(new ComparisonResult
                    {
                        Left = l,
                        BestMatch = r,
                        BestScore = sc,
                        Reason = localEngine.LastReason
                    });
                }

                var top = scores.OrderByDescending(x => x.BestScore).Take(topN).ToList();

                list.Add(new ComparisonBundle
                {
                    LeftRecord = l,
                    Candidates = top
                });

                i++;
                // update progress on UI thread
                this.Invoke((Action)(() =>
                {
                    progressBar.Value = Math.Min(i, total);
                    lblStatus.Text = $"Computing... {i}/{total}";
                }));
            }

            return list;
        }

        // -------------------------
        // Show current left record and its top-N matches
        // -------------------------
        private void ShowCurrentLeftRecord()
        {
            if (groupedResults == null || groupedResults.Count == 0)
            {
                lblCurrentLeft.Text = "Current Left Record: -";
                dgvResults.Rows.Clear();
                return;
            }

            if (currentLeftIndex < 0) currentLeftIndex = 0;
            if (currentLeftIndex >= groupedResults.Count) currentLeftIndex = groupedResults.Count - 1;

            var group = groupedResults[currentLeftIndex];

            lblCurrentLeft.Text = $"Left [{currentLeftIndex + 1}/{groupedResults.Count}]  ID={group.LeftRecord?.Id ?? "-"}  {group.LeftRecord?.OriginalAddress ?? ""}";

            dgvResults.Rows.Clear();
            foreach (var c in group.Candidates)
            {
                dgvResults.Rows.Add(
                    c.BestMatch?.Id ?? "",
                    c.BestMatch?.OriginalAddress ?? "",
                    (c.BestScore * 100).ToString("F2"),
                    c.Reason ?? ""
                );
            }

            //lblStatus.Text = $"Showing {currentLeftIndex + 1}/{groupedResults.Count}: ";
            lblCurrentLeft.Text = $"Left [{currentLeftIndex + 1}/{groupedResults.Count}]  Address={group.LeftRecord?.OriginalAddress ?? "-"}";
            AdjustGridColumns();
        }

        // -------------------------
        // Next button
        // -------------------------
        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (groupedResults == null || groupedResults.Count == 0) return;

            if (currentLeftIndex < groupedResults.Count - 1)
            {
                currentLeftIndex++;
                ShowCurrentLeftRecord();
            }
            else
            {
                MessageBox.Show("Reached end of left records.", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // -------------------------
        // helper: adjust columns
        // -------------------------
        private void AdjustGridColumns()
        {
            try
            {
                if (dgvResults.Columns.Contains("RightAddr"))
                    dgvResults.Columns["RightAddr"].Width = 420;
                if (dgvResults.Columns.Contains("Reason"))
                    dgvResults.Columns["Reason"].Width = 520;
                if (dgvResults.Columns.Contains("Score"))
                    dgvResults.Columns["Score"].Width = 120;
                if (dgvResults.Columns.Contains("RightID"))
                    dgvResults.Columns["RightID"].Width = 100;
            }
            catch
            {
                // ignore sizing errors
            }
        }
    }

    // simple helper types (kept local to form file)
    public class ComparisonBundle
    {
        public RowRecord LeftRecord { get; set; }
        public List<ComparisonResult> Candidates { get; set; } = new();
    }
}
