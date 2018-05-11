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
            this.basicView1 = new FrontWork.BasicView();
            this.configuration1 = new FrontWork.Configuration();
            this.pagerView1 = new FrontWork.PagerView();
            this.SuspendLayout();
            // 
            // basicView1
            // 
            this.basicView1.Configuration = this.configuration1;
            this.basicView1.Font = new System.Drawing.Font("黑体", 10F);
            this.basicView1.ItemsPerRow = 3;
            this.basicView1.Location = new System.Drawing.Point(66, 35);
            this.basicView1.Margin = new System.Windows.Forms.Padding(0);
            this.basicView1.Mode = "default";
            this.basicView1.Model = null;
            this.basicView1.Name = "basicView1";
            this.basicView1.Size = new System.Drawing.Size(883, 219);
            this.basicView1.TabIndex = 0;
            // 
            // configuration1
            // 
            this.configuration1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.configuration1.ConfigurationString = resources.GetString("configuration1.ConfigurationString");
            this.configuration1.Location = new System.Drawing.Point(585, 344);
            this.configuration1.MethodListeners = new FrontWork.ModeMethodListenerNamesPair[0];
            this.configuration1.Name = "configuration1";
            this.configuration1.Size = new System.Drawing.Size(180, 180);
            this.configuration1.TabIndex = 1;
            // 
            // pagerView1
            // 
            this.pagerView1.Location = new System.Drawing.Point(22, 221);
            this.pagerView1.Mode = "default";
            this.pagerView1.Name = "pagerView1";
            this.pagerView1.PageSize = ((long)(50));
            this.pagerView1.Size = new System.Drawing.Size(1280, 84);
            this.pagerView1.TabIndex = 2;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1381, 668);
            this.Controls.Add(this.pagerView1);
            this.Controls.Add(this.configuration1);
            this.Controls.Add(this.basicView1);
            this.Name = "Form2";
            this.Text = "Form";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private FrontWork.BasicView basicView1;
        private FrontWork.Configuration configuration1;
        private FrontWork.PagerView pagerView1;
    }
}