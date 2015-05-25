namespace Mvvm.UI.Win {
    partial class SimpleBindingForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.btnResetTtile = new System.Windows.Forms.Button();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnResetTtile
            // 
            this.btnResetTtile.AutoSize = true;
            this.btnResetTtile.Location = new System.Drawing.Point(12, 36);
            this.btnResetTtile.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnResetTtile.Name = "btnResetTtile";
            this.btnResetTtile.Size = new System.Drawing.Size(69, 23);
            this.btnResetTtile.TabIndex = 0;
            this.btnResetTtile.Text = "Reset Title";
            // 
            // tbTitle
            // 
            this.tbTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.tbTitle.Location = new System.Drawing.Point(12, 12);
            this.tbTitle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(360, 20);
            this.tbTitle.TabIndex = 1;
            // 
            // SimpleBindingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 111);
            this.Controls.Add(this.tbTitle);
            this.Controls.Add(this.btnResetTtile);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "SimpleBindingForm";
            this.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnResetTtile;
        private System.Windows.Forms.TextBox tbTitle;
    }
}

