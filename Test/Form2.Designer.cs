namespace Test
{
    partial class Form2
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
            this.configuration1 = new FrontWork.Configuration();
            this.SuspendLayout();
            // 
            // configuration1
            // 
            this.configuration1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.configuration1.ConfigurationString = null;
            this.configuration1.Location = new System.Drawing.Point(1226, 12);
            this.configuration1.MethodListeners = null;
            this.configuration1.Mode = "default";
            this.configuration1.Name = "configuration1";
            this.configuration1.Size = new System.Drawing.Size(180, 180);
            this.configuration1.TabIndex = 0;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1401, 674);
            this.Controls.Add(this.configuration1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private FrontWork.Configuration configuration1;
    }
}