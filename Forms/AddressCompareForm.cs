using SmartRecordMatcher.Engine;
using SmartRecordMatcher.Models;
using SmartRecordMatcher.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartRecordMatcher.Forms
{
    public partial class AddressCompareForm : Form
    {
        private List<RowRecord> leftRecords = new();
        private List<RowRecord> rightRecords = new();
        private AddressSimilarityEngine similarityEngine;

        public AddressCompareForm()
        {
            InitializeComponent();

            var storage = new StorageService();
            var config = storage.LoadWeightConfig();
            similarityEngine = new AddressSimilarityEngine(config);

            btnLoadLeft.Click += BtnLoadLeft_Click;
            btnLoadRight.Click += BtnLoadRight_Click;
            btnCompute.Click += BtnCompute_Click;
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
            if (leftRecords.Count == 0 || rightRecords.Count == 0)
            {
                MessageBox.Show("Please load both files first.");
                return;
            }

            dgvResults.Rows.Clear();
            progressBar.Minimum = 0;
            progressBar.Maximum = leftRecords.Count;
            progressBar.Value = 0;
            lblStatus.Text = "Computing...";

            var results = await Task.Run(() => ComputeAll(leftRecords, rightRecords));

            dgvResults.Columns.Clear();
            dgvResults.Columns.Add("LeftId", "Left Id");
            dgvResults.Columns.Add("LeftAddress", "Left Address");
            dgvResults.Columns.Add("BestRightId", "Best Right Id");
            dgvResults.Columns.Add("BestRightAddress", "Best Right Address");
            dgvResults.Columns.Add("Similarity", "Similarity (%)");
            dgvResults.Columns.Add("Reason", "Reason");

            foreach (var r in results)
            {
                dgvResults.Rows.Add(
                    r.Left.Id,
                    r.Left.OriginalAddress,
                    r.BestMatch?.Id ?? "",
                    r.BestMatch?.OriginalAddress ?? "",
                    (r.BestScore * 100).ToString("F2"),
                    r.Reason ?? ""
                );
            }

            lblStatus.Text = $"Done. Processed {results.Count} records.";
        }

        private List<ComparisonResult> ComputeAll(List<RowRecord> left, List<RowRecord> right)
        {
            var outList = new List<ComparisonResult>();
            int total = left.Count;
            int i = 0;

            // موتور جدید
            var cfg = new WeightConfig();
            var similarityEngine = new AddressSimilarityEnginePro(cfg);

            foreach (var l in left)
            {
                var best = new ComparisonResult
                {
                    Left = l,
                    BestScore = 0,
                    BestMatch = null,
                    Reason = ""
                };

                foreach (var r in right)
                {
                    double score = similarityEngine.Compute(l.OriginalAddress, r.OriginalAddress);

                    if (score > best.BestScore)
                    {
                        best.BestScore = score;
                        best.BestMatch = r;
                        best.Reason = similarityEngine.LastReason;
                    }
                }

                outList.Add(best);

                // UI progress
                i++;
                this.Invoke((Action)(() =>
                {
                    progressBar.Value = Math.Min(i, total);
                }));
            }

            return outList;
        }


        private void btnLoadLeft_Click_1(object sender, EventArgs e)
        {

        }

        private void btnCompute_Click_1(object sender, EventArgs e)
        {

        }
    }
}
