namespace EventDev
{
    partial class FormEventDev
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
            this.btn_showTIme = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_noShow = new System.Windows.Forms.Button();
            this.btn_RegAgain = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_showTIme
            // 
            this.btn_showTIme.Location = new System.Drawing.Point(62, 114);
            this.btn_showTIme.Name = "btn_showTIme";
            this.btn_showTIme.Size = new System.Drawing.Size(89, 32);
            this.btn_showTIme.TabIndex = 0;
            this.btn_showTIme.Text = "更新当前时间";
            this.btn_showTIme.UseVisualStyleBackColor = true;
            this.btn_showTIme.Click += new System.EventHandler(this.btn_showTIme_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "RevitAPI HelloWorld";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 41);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(239, 21);
            this.textBox1.TabIndex = 2;
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(152, 162);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(89, 32);
            this.btn_OK.TabIndex = 3;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(252, 162);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(89, 32);
            this.btn_Cancel.TabIndex = 3;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_noShow
            // 
            this.btn_noShow.Location = new System.Drawing.Point(157, 114);
            this.btn_noShow.Name = "btn_noShow";
            this.btn_noShow.Size = new System.Drawing.Size(89, 32);
            this.btn_noShow.TabIndex = 0;
            this.btn_noShow.Text = "取消更新";
            this.btn_noShow.UseVisualStyleBackColor = true;
            this.btn_noShow.Click += new System.EventHandler(this.btn_noShow_Click);
            // 
            // btn_RegAgain
            // 
            this.btn_RegAgain.Location = new System.Drawing.Point(252, 114);
            this.btn_RegAgain.Name = "btn_RegAgain";
            this.btn_RegAgain.Size = new System.Drawing.Size(89, 32);
            this.btn_RegAgain.TabIndex = 0;
            this.btn_RegAgain.Text = "重新注册事件";
            this.btn_RegAgain.UseVisualStyleBackColor = true;
            this.btn_RegAgain.Click += new System.EventHandler(this.btn_RegAgain_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(353, 206);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_RegAgain);
            this.Controls.Add(this.btn_noShow);
            this.Controls.Add(this.btn_showTIme);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "欢迎参加橄榄山Revit二次开发培训";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_showTIme;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_noShow;
        private System.Windows.Forms.Button btn_RegAgain;
    }
}

