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
            dgvResults.Location = new Point(20, 160);
            dgvResults.Name = "dgvResults";
            dgvResults.ReadOnly = true;
            dgvResults.RowHeadersVisible = false;
            dgvResults.Size = new Size(930, 480);
            dgvResults.TabIndex = 0;
            // 
            // btnLoadLeft
            // 
            btnLoadLeft.Location = new Point(20, 20);
            btnLoadLeft.Name = "btnLoadLeft";
            btnLoadLeft.Size = new Size(150, 30);
            btnLoadLeft.TabIndex = 1;
            btnLoadLeft.Text = "Load File 1 (Left)";
            btnLoadLeft.UseVisualStyleBackColor = true;
            btnLoadLeft.Click += btnLoadLeft_Click_1;
            // 
            // btnLoadRight
            // 
            btnLoadRight.Location = new Point(20, 60);
            btnLoadRight.Name = "btnLoadRight";
            btnLoadRight.Size = new Size(150, 30);
            btnLoadRight.TabIndex = 2;
            btnLoadRight.Text = "Load File 2 (Right)";
            btnLoadRight.UseVisualStyleBackColor = true;
            // 
            // btnCompute
            // 
            btnCompute.Location = new Point(20, 110);
            btnCompute.Name = "btnCompute";
            btnCompute.Size = new Size(200, 36);
            btnCompute.TabIndex = 5;
            btnCompute.Text = "Compute Similarities";
            btnCompute.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(240, 110);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(400, 36);
            progressBar.TabIndex = 6;
            // 
            // txtLeftPath
            // 
            txtLeftPath.Location = new Point(190, 20);
            txtLeftPath.Name = "txtLeftPath";
            txtLeftPath.ReadOnly = true;
            txtLeftPath.Size = new Size(760, 23);
            txtLeftPath.TabIndex = 3;
            // 
            // txtRightPath
            // 
            txtRightPath.Location = new Point(190, 60);
            txtRightPath.Name = "txtRightPath";
            txtRightPath.ReadOnly = true;
            txtRightPath.Size = new Size(760, 23);
            txtRightPath.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(660, 110);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(290, 36);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Status: Ready";
            // 
            // AddressCompareForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 700);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
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
