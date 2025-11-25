namespace SmartRecordMatcher.Forms
{
    partial class AddressCompareForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.Button btnLoadLeft;
        private System.Windows.Forms.Button btnLoadRight;
        private System.Windows.Forms.Button btnCompute;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtLeftPath;
        private System.Windows.Forms.TextBox txtRightPath;
        private System.Windows.Forms.Label lblStatus;

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
            dgvResults = new DataGridView();
            btnLoadLeft = new Button();
            btnLoadRight = new Button();
            btnCompute = new Button();
            progressBar = new ProgressBar();
            txtLeftPath = new TextBox();
            txtRightPath = new TextBox();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            SuspendLayout();
            // 
            // dgvResults
            // 
            dgvResults.AllowUserToAddRows = false;
            dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Location = new Point(23, 213);
            dgvResults.Margin = new Padding(3, 4, 3, 4);
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.RowHeadersWidth = 51;
            dgvResults.Size = new Size(1063, 640);
            dgvResults.TabIndex = 0;
            // 
            // btnLoadLeft
            // 
            btnLoadLeft.Location = new Point(23, 27);
            btnLoadLeft.Margin = new Padding(3, 4, 3, 4);
            btnLoadLeft.Name = "btnLoadLeft";
            btnLoadLeft.Size = new Size(171, 40);
            btnLoadLeft.TabIndex = 1;
            btnLoadLeft.Text = "Load File 1 (Left)";
            btnLoadLeft.UseVisualStyleBackColor = true;
            btnLoadLeft.Click += btnLoadLeft_Click_1;
            // 
            // btnLoadRight
            // 
            btnLoadRight.Location = new Point(23, 80);
            btnLoadRight.Margin = new Padding(3, 4, 3, 4);
            btnLoadRight.Name = "btnLoadRight";
            btnLoadRight.Size = new Size(171, 40);
            btnLoadRight.TabIndex = 2;
            btnLoadRight.Text = "Load File 2 (Right)";
            btnLoadRight.UseVisualStyleBackColor = true;
            // 
            // btnCompute
            // 
            btnCompute.Location = new Point(23, 147);
            btnCompute.Margin = new Padding(3, 4, 3, 4);
            btnCompute.Name = "btnCompute";
            btnCompute.Size = new Size(229, 48);
            btnCompute.TabIndex = 5;
            btnCompute.Text = "Compute Similarities";
            btnCompute.UseVisualStyleBackColor = true;
            btnCompute.Click += btnCompute_Click_1;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(274, 147);
            progressBar.Margin = new Padding(3, 4, 3, 4);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(457, 48);
            progressBar.TabIndex = 6;
            // 
            // txtLeftPath
            // 
            txtLeftPath.Location = new Point(217, 27);
            txtLeftPath.Margin = new Padding(3, 4, 3, 4);
            txtLeftPath.Name = "txtLeftPath";
            txtLeftPath.ReadOnly = true;
            txtLeftPath.Size = new Size(868, 27);
            txtLeftPath.TabIndex = 3;
            // 
            // txtRightPath
            // 
            txtRightPath.Location = new Point(217, 80);
            txtRightPath.Margin = new Padding(3, 4, 3, 4);
            txtRightPath.Name = "txtRightPath";
            txtRightPath.ReadOnly = true;
            txtRightPath.Size = new Size(868, 27);
            txtRightPath.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(754, 147);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(331, 48);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Status: Ready";
            // 
            // AddressCompareForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1143, 933);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnCompute);
            Controls.Add(txtRightPath);
            Controls.Add(txtLeftPath);
            Controls.Add(btnLoadRight);
            Controls.Add(btnLoadLeft);
            Controls.Add(dgvResults);
            Margin = new Padding(3, 4, 3, 4);
            Name = "AddressCompareForm";
            Text = "Address Similarity - SmartRecordMatcher";
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
