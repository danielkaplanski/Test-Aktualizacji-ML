namespace testowanie
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblUpdateInfo = new Label();
            updateBtn = new Button();
            lblCurrentVersion = new Label();
            label1 = new Label();
            SuspendLayout();
            // 
            // lblUpdateInfo
            // 
            lblUpdateInfo.AutoSize = true;
            lblUpdateInfo.Location = new Point(255, 238);
            lblUpdateInfo.Name = "lblUpdateInfo";
            lblUpdateInfo.Size = new Size(180, 15);
            lblUpdateInfo.TabIndex = 0;
            lblUpdateInfo.Text = "Dostepna jest nowa aktualizacja !";
            // 
            // updateBtn
            // 
            updateBtn.Location = new Point(294, 283);
            updateBtn.Name = "updateBtn";
            updateBtn.Size = new Size(75, 23);
            updateBtn.TabIndex = 1;
            updateBtn.Text = "Aktualizuj";
            updateBtn.UseVisualStyleBackColor = true;
            updateBtn.Click += updateBtn_Click;
            // 
            // lblCurrentVersion
            // 
            lblCurrentVersion.AutoSize = true;
            lblCurrentVersion.Location = new Point(319, 367);
            lblCurrentVersion.Name = "lblCurrentVersion";
            lblCurrentVersion.Size = new Size(38, 15);
            lblCurrentVersion.TabIndex = 2;
            lblCurrentVersion.Text = "label2";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(240, 63);
            label1.Name = "label1";
            label1.Size = new Size(102, 15);
            label1.TabIndex = 3;
            label1.Text = "Najnowsza Wersja";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(label1);
            Controls.Add(lblCurrentVersion);
            Controls.Add(updateBtn);
            Controls.Add(lblUpdateInfo);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblUpdateInfo;
        private Button updateBtn;
        private Label lblCurrentVersion;
        private Label label1;
    }
}
