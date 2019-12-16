namespace CreateBuffer
{
    partial class ScrollableMessageBox
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
            this.showText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // showText
            // 
            this.showText.AutoSize = true;
            this.showText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.showText.Location = new System.Drawing.Point(13, 13);
            this.showText.Name = "showText";
            this.showText.Size = new System.Drawing.Size(18, 20);
            this.showText.TabIndex = 0;
            this.showText.Text = ">";
            // 
            // ScrollableMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.showText);
            this.Name = "ScrollableMessageBox";
            this.Text = "ScrollableMessageBox";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label showText;
    }
}