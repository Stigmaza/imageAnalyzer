
namespace FO.CLS.UTIL
{
    partial class FOEXCEL
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
            if(disposing && (components != null))
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
            this.pnlExcelMsg = new System.Windows.Forms.Panel();
            this.lblExcelMsg = new System.Windows.Forms.Label();
            this.lblExcelCnt = new System.Windows.Forms.Label();
            this.pnlExcelMsg.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlExcelMsg
            // 
            this.pnlExcelMsg.BackColor = System.Drawing.Color.DimGray;
            this.pnlExcelMsg.Controls.Add(this.lblExcelMsg);
            this.pnlExcelMsg.Controls.Add(this.lblExcelCnt);
            this.pnlExcelMsg.Location = new System.Drawing.Point(3, 4);
            this.pnlExcelMsg.Name = "pnlExcelMsg";
            this.pnlExcelMsg.Size = new System.Drawing.Size(592, 302);
            this.pnlExcelMsg.TabIndex = 29;
            // 
            // lblExcelMsg
            // 
            this.lblExcelMsg.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblExcelMsg.ForeColor = System.Drawing.Color.White;
            this.lblExcelMsg.Location = new System.Drawing.Point(19, 29);
            this.lblExcelMsg.Name = "lblExcelMsg";
            this.lblExcelMsg.Size = new System.Drawing.Size(523, 77);
            this.lblExcelMsg.TabIndex = 1;
            this.lblExcelMsg.Tag = "";
            this.lblExcelMsg.Text = "EXCEL EXPROT COMPLETE";
            this.lblExcelMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblExcelCnt
            // 
            this.lblExcelCnt.Font = new System.Drawing.Font("맑은 고딕", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblExcelCnt.ForeColor = System.Drawing.Color.White;
            this.lblExcelCnt.Location = new System.Drawing.Point(19, 147);
            this.lblExcelCnt.Name = "lblExcelCnt";
            this.lblExcelCnt.Size = new System.Drawing.Size(523, 104);
            this.lblExcelCnt.TabIndex = 0;
            this.lblExcelCnt.Tag = "";
            this.lblExcelCnt.Text = "- / -";
            this.lblExcelCnt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FOEXCEL
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 308);
            this.Controls.Add(this.pnlExcelMsg);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FOEXCEL";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FO";
            this.TopMost = true;
            this.pnlExcelMsg.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlExcelMsg;
        private System.Windows.Forms.Label lblExcelMsg;
        private System.Windows.Forms.Label lblExcelCnt;
    }
}