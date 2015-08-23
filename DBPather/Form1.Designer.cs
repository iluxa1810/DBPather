namespace DBPather
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.startButton = new System.Windows.Forms.Button();
            this.textPatchPath = new System.Windows.Forms.TextBox();
            this.textDbPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBdFileDialog = new System.Windows.Forms.Button();
            this.btnPthFileDialog = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(230, 330);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(179, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Патчить";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // textPatchPath
            // 
            this.textPatchPath.Enabled = false;
            this.textPatchPath.Location = new System.Drawing.Point(93, 52);
            this.textPatchPath.Name = "textPatchPath";
            this.textPatchPath.Size = new System.Drawing.Size(449, 20);
            this.textPatchPath.TabIndex = 1;
            // 
            // textDbPath
            // 
            this.textDbPath.Enabled = false;
            this.textDbPath.Location = new System.Drawing.Point(93, 26);
            this.textDbPath.Name = "textDbPath";
            this.textDbPath.Size = new System.Drawing.Size(449, 20);
            this.textDbPath.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Путь к Базе:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Путь к Патчу:";
            // 
            // btnBdFileDialog
            // 
            this.btnBdFileDialog.Location = new System.Drawing.Point(548, 23);
            this.btnBdFileDialog.Name = "btnBdFileDialog";
            this.btnBdFileDialog.Size = new System.Drawing.Size(27, 23);
            this.btnBdFileDialog.TabIndex = 5;
            this.btnBdFileDialog.Text = "...";
            this.btnBdFileDialog.UseVisualStyleBackColor = true;
            this.btnBdFileDialog.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnPthFileDialog
            // 
            this.btnPthFileDialog.Location = new System.Drawing.Point(548, 49);
            this.btnPthFileDialog.Name = "btnPthFileDialog";
            this.btnPthFileDialog.Size = new System.Drawing.Size(27, 23);
            this.btnPthFileDialog.TabIndex = 6;
            this.btnPthFileDialog.Text = "...";
            this.btnPthFileDialog.UseVisualStyleBackColor = true;
            this.btnPthFileDialog.Click += new System.EventHandler(this.button3_Click);
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(64, 114);
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            this.logBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.logBox.Size = new System.Drawing.Size(511, 210);
            this.logBox.TabIndex = 7;
            this.logBox.Text = resources.GetString("logBox.Text");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(284, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Сообщения";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(628, 365);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.btnPthFileDialog);
            this.Controls.Add(this.btnBdFileDialog);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textDbPath);
            this.Controls.Add(this.textPatchPath);
            this.Controls.Add(this.startButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.TextBox textPatchPath;
        private System.Windows.Forms.TextBox textDbPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBdFileDialog;
        private System.Windows.Forms.Button btnPthFileDialog;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.Label label3;
    }
}

