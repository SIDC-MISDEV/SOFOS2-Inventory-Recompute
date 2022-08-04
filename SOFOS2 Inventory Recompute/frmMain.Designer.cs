namespace SOFOS2_Inventory_Recompute
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnUpdateTransValue = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsDB = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblUpdateZero = new System.Windows.Forms.Label();
            this.btnUpdateCost = new System.Windows.Forms.Button();
            this.lblCostProgress = new System.Windows.Forms.Label();
            this.lblUpdateCostMasterData = new System.Windows.Forms.Label();
            this.btnUpdateCostBatchTwo = new System.Windows.Forms.Button();
            this.btnLast = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnUpdateTransValue
            // 
            this.btnUpdateTransValue.Location = new System.Drawing.Point(59, 50);
            this.btnUpdateTransValue.Name = "btnUpdateTransValue";
            this.btnUpdateTransValue.Size = new System.Drawing.Size(325, 56);
            this.btnUpdateTransValue.TabIndex = 1;
            this.btnUpdateTransValue.Text = "#1 Update Transaction Value per Transaction";
            this.btnUpdateTransValue.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsDB});
            this.statusStrip1.Location = new System.Drawing.Point(0, 633);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(464, 25);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsDB
            // 
            this.tsDB.Name = "tsDB";
            this.tsDB.Size = new System.Drawing.Size(151, 20);
            this.tsDB.Text = "toolStripStatusLabel1";
            // 
            // lblUpdateZero
            // 
            this.lblUpdateZero.Location = new System.Drawing.Point(59, 334);
            this.lblUpdateZero.Name = "lblUpdateZero";
            this.lblUpdateZero.Size = new System.Drawing.Size(325, 79);
            this.lblUpdateZero.TabIndex = 22;
            // 
            // btnUpdateCost
            // 
            this.btnUpdateCost.Location = new System.Drawing.Point(59, 112);
            this.btnUpdateCost.Name = "btnUpdateCost";
            this.btnUpdateCost.Size = new System.Drawing.Size(325, 56);
            this.btnUpdateCost.TabIndex = 2;
            this.btnUpdateCost.Text = "#2 Update cost every transaction";
            this.btnUpdateCost.UseVisualStyleBackColor = true;
            // 
            // lblCostProgress
            // 
            this.lblCostProgress.Location = new System.Drawing.Point(59, 171);
            this.lblCostProgress.Name = "lblCostProgress";
            this.lblCostProgress.Size = new System.Drawing.Size(325, 93);
            this.lblCostProgress.TabIndex = 23;
            // 
            // lblUpdateCostMasterData
            // 
            this.lblUpdateCostMasterData.Location = new System.Drawing.Point(59, 264);
            this.lblUpdateCostMasterData.Name = "lblUpdateCostMasterData";
            this.lblUpdateCostMasterData.Size = new System.Drawing.Size(325, 43);
            this.lblUpdateCostMasterData.TabIndex = 24;
            // 
            // btnUpdateCostBatchTwo
            // 
            this.btnUpdateCostBatchTwo.Location = new System.Drawing.Point(59, 468);
            this.btnUpdateCostBatchTwo.Name = "btnUpdateCostBatchTwo";
            this.btnUpdateCostBatchTwo.Size = new System.Drawing.Size(325, 56);
            this.btnUpdateCostBatchTwo.TabIndex = 25;
            this.btnUpdateCostBatchTwo.Text = "#3 Update cost every transaction for wrong sorting of date";
            this.btnUpdateCostBatchTwo.UseVisualStyleBackColor = true;
            // 
            // btnLast
            // 
            this.btnLast.Location = new System.Drawing.Point(62, 530);
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(325, 56);
            this.btnLast.TabIndex = 26;
            this.btnLast.Text = "#4 Update Remaining Items not fixed in #2 and #3";
            this.btnLast.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(62, 589);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(325, 31);
            this.label1.TabIndex = 27;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 658);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLast);
            this.Controls.Add(this.btnUpdateCostBatchTwo);
            this.Controls.Add(this.lblUpdateCostMasterData);
            this.Controls.Add(this.lblCostProgress);
            this.Controls.Add(this.btnUpdateCost);
            this.Controls.Add(this.lblUpdateZero);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnUpdateTransValue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUpdateTransValue;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsDB;
        public System.Windows.Forms.Label lblUpdateZero;
        private System.Windows.Forms.Button btnUpdateCost;
        public System.Windows.Forms.Label lblCostProgress;
        public System.Windows.Forms.Label lblUpdateCostMasterData;
        private System.Windows.Forms.Button btnUpdateCostBatchTwo;
        private System.Windows.Forms.Button btnLast;
        public System.Windows.Forms.Label label1;
    }
}

