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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.model1 = new FrontWork.Model();
            this.jsonRESTSynchronizer1 = new FrontWork.JsonRESTSynchronizer();
            this.configuration1 = new FrontWork.Configuration();
            this.SuspendLayout();
            // 
            // model1
            // 
            this.model1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.model1.Configuration = null;
            this.model1.FirstSelectionRange = null;
            this.model1.Location = new System.Drawing.Point(396, 99);
            this.model1.Name = "model1";
            this.model1.SelectionRange = new FrontWork.Range[0];
            this.model1.Size = new System.Drawing.Size(180, 180);
            this.model1.TabIndex = 4;
            // 
            // jsonRESTSynchronizer1
            // 
            this.jsonRESTSynchronizer1.Configuration = null;
            this.jsonRESTSynchronizer1.Location = new System.Drawing.Point(818, 200);
            this.jsonRESTSynchronizer1.Margin = new System.Windows.Forms.Padding(0);
            this.jsonRESTSynchronizer1.Model = null;
            this.jsonRESTSynchronizer1.Name = "jsonRESTSynchronizer1";
            this.jsonRESTSynchronizer1.Size = new System.Drawing.Size(180, 180);
            this.jsonRESTSynchronizer1.TabIndex = 5;
            // 
            // configuration1
            // 
            this.configuration1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.configuration1.ConfigurationString = resources.GetString("configuration1.ConfigurationString");
            this.configuration1.Location = new System.Drawing.Point(415, 334);
            this.configuration1.MethodListeners = new FrontWork.ModeMethodListenerNamePair[0];
            this.configuration1.Mode = "default";
            this.configuration1.Name = "configuration1";
            this.configuration1.Size = new System.Drawing.Size(150, 150);
            this.configuration1.TabIndex = 6;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1381, 668);
            this.Controls.Add(this.configuration1);
            this.Controls.Add(this.jsonRESTSynchronizer1);
            this.Controls.Add(this.model1);
            this.Name = "Form2";
            this.Text = "Form";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private FrontWork.Model model1;
        private FrontWork.JsonRESTSynchronizer jsonRESTSynchronizer1;
        private FrontWork.Configuration configuration1;
    }
}