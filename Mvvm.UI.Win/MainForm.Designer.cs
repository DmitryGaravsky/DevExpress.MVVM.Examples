namespace Mvvm.UI.Win {
    partial class MainForm {
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
            // btnUpdateTtile
            // 
            this.btnResetTtile.AutoSize = true;
            this.btnResetTtile.Location = new System.Drawing.Point(35, 90);
            this.btnResetTtile.Name = "btnUpdateTtile";
            this.btnResetTtile.Size = new System.Drawing.Size(138, 35);
            this.btnResetTtile.TabIndex = 0;
            this.btnResetTtile.Text = "Reset Title";
            // 
            // textBox1
            // 
            this.tbTitle.Location = new System.Drawing.Point(35, 53);
            this.tbTitle.Name = "textBox1";
            this.tbTitle.Size = new System.Drawing.Size(727, 31);
            this.tbTitle.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 529);
            this.Controls.Add(this.tbTitle);
            this.Controls.Add(this.btnResetTtile);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnResetTtile;
        private System.Windows.Forms.TextBox tbTitle;
    }
}

