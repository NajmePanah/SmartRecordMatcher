using System.Drawing;
using System.Windows.Forms;

namespace SmartRecordMatcher.Forms
{
    partial class AddressCompareForm
    {
        private System.ComponentModel.IContainer components = null;

        private DataGridView dgvResults;
        private Button btnLoadLeft;
        private Button btnLoadRight;
        private Button btnCompute;
        private Button btnNext;
        private ProgressBar progressBar;
        private TextBox txtLeftPath;
        private TextBox txtRightPath;
        private Label lblStatus;
        private Label lblCurrentLeft;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            dgvResults = new DataGridView();
            btnLoadLeft = new Button();
            btnLoadRight = new Button();
            btnCompute = new Button();
            btnNext = new Button();
            progressBar = new ProgressBar();
            txtLeftPath = new TextBox();
            txtRightPath = new TextBox();
            lblStatus = new Label();
            lblCurrentLeft = new Label();

            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            SuspendLayout();

            // dgvResults
            dgvResults.AllowUserToAddRows = false;
            dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Location = new Point(20, 200);
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.Size = new Size(1040, 500);
            dgvResults.TabIndex = 0;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResults.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvResults.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;

            // btnLoadLeft
            btnLoadLeft.Location = new Point(20, 20);
            btnLoadLeft.Size = new Size(150, 30);
            btnLoadLeft.Name = "btnLoadLeft";
            btnLoadLeft.Text = "Load File 1 (Left)";

            // btnLoadRight
            btnLoadRight.Location = new Point(20, 60);
            btnLoadRight.Size = new Size(150, 30);
            btnLoadRight.Name = "btnLoadRight";
            btnLoadRight.Text = "Load File 2 (Right)";

            // txtLeftPath
            txtLeftPath.Location = new Point(190, 20);
            txtLeftPath.ReadOnly = true;
            txtLeftPath.Size = new Size(870, 23);
            txtLeftPath.Name = "txtLeftPath";

            // txtRightPath
            txtRightPath.Location = new Point(190, 60);
            txtRightPath.ReadOnly = true;
            txtRightPath.Size = new Size(870, 23);
            txtRightPath.Name = "txtRightPath";

            // btnCompute
            btnCompute.Location = new Point(20, 110);
            btnCompute.Size = new Size(200, 36);
            btnCompute.Name = "btnCompute";
            btnCompute.Text = "Start Step Comparison";

            // btnNext
            btnNext.Location = new Point(240, 110);
            btnNext.Size = new Size(150, 36);
            btnNext.Name = "btnNext";
            btnNext.Text = "Next Record →";
            btnNext.Enabled = false;

            // progressBar
            progressBar.Location = new Point(410, 110);
            progressBar.Size = new Size(300, 36);
            progressBar.Name = "progressBar";

            // lblStatus
            lblStatus.Location = new Point(730, 110);
            lblStatus.Size = new Size(330, 36);
            lblStatus.Name = "lblStatus";
            lblStatus.Text = "Status: Ready";

            // lblCurrentLeft
            lblCurrentLeft.Location = new Point(20, 160);
            lblCurrentLeft.Size = new Size(1040, 30);
            lblCurrentLeft.Name = "lblCurrentLeft";
            lblCurrentLeft.Text = "Current Left Record: -";
            lblCurrentLeft.AutoEllipsis = true;

            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 740);

            Controls.Add(lblCurrentLeft);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnNext);
            Controls.Add(btnCompute);
            Controls.Add(txtRightPath);
            Controls.Add(txtLeftPath);
            Controls.Add(btnLoadRight);
            Controls.Add(btnLoadLeft);
            Controls.Add(dgvResults);

            Name = "AddressCompareForm";
            Text = "Address Similarity - SmartRecordMatcher";

            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
