namespace Test
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.basicView1 = new FrontWork.BasicView();
            this.configuration = new FrontWork.Configuration();
            this.model1 = new FrontWork.Model();
            this.jsonRESTSynchronizer1 = new FrontWork.JsonRESTSynchronizer();
            this.reoGridView1 = new FrontWork.ReoGridView();
            this.pagerWidget1 = new FrontWork.PagerView();
            this.searchWidget1 = new FrontWork.SearchView();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("黑体", 10F);
            this.button1.Location = new System.Drawing.Point(825, 211);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(160, 71);
            this.button1.TabIndex = 1;
            this.button1.Text = "查询";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("黑体", 10F);
            this.button2.Location = new System.Drawing.Point(1007, 211);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(160, 69);
            this.button2.TabIndex = 3;
            this.button2.Text = "保存修改";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("黑体", 10F);
            this.button3.Location = new System.Drawing.Point(1007, 130);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(160, 68);
            this.button3.TabIndex = 4;
            this.button3.Text = "增行";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("黑体", 10F);
            this.button4.Location = new System.Drawing.Point(1007, 286);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(160, 68);
            this.button4.TabIndex = 5;
            this.button4.Text = "删除";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // basicView1
            // 
            this.basicView1.Configuration = this.configuration;
            this.basicView1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.basicView1.Location = new System.Drawing.Point(9, 81);
            this.basicView1.Margin = new System.Windows.Forms.Padding(0);
            this.basicView1.Model = this.model1;
            this.basicView1.Name = "basicView1";
            this.basicView1.Size = new System.Drawing.Size(813, 295);
            this.basicView1.TabIndex = 13;
            // 
            // configuration
            // 
            this.configuration.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("configuration.BackgroundImage")));
            this.configuration.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.configuration.ConfigurationString = resources.GetString("configuration.ConfigurationString");
            this.configuration.Location = new System.Drawing.Point(1114, 388);
            this.configuration.Mode = "default";
            this.configuration.Name = "configuration";
            this.configuration.Size = new System.Drawing.Size(160, 160);
            this.configuration.TabIndex = 10;
            // 
            // model1
            // 
            this.model1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("model1.BackgroundImage")));
            this.model1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.model1.Configuration = this.configuration;
            this.model1.FirstSelectionRange = null;
            this.model1.Location = new System.Drawing.Point(975, 382);
            this.model1.Name = "model1";
            this.model1.SelectionRange = new FrontWork.Range[0];
            this.model1.Size = new System.Drawing.Size(160, 160);
            this.model1.TabIndex = 11;
            this.model1.Load += new System.EventHandler(this.model1_Load);
            // 
            // jsonRESTSynchronizer1
            // 
            this.jsonRESTSynchronizer1.Location = new System.Drawing.Point(819, 382);
            this.jsonRESTSynchronizer1.Margin = new System.Windows.Forms.Padding(0);
            this.jsonRESTSynchronizer1.Model = this.model1;
            this.jsonRESTSynchronizer1.Name = "jsonRESTSynchronizer1";
            this.jsonRESTSynchronizer1.Size = new System.Drawing.Size(180, 180);
            this.jsonRESTSynchronizer1.TabIndex = 12;
            this.jsonRESTSynchronizer1.Load += new System.EventHandler(this.jsonRESTSynchronizer1_Load);
            // 
            // reoGridView1
            // 
            this.reoGridView1.Configuration = this.configuration;
            this.reoGridView1.Location = new System.Drawing.Point(12, 388);
            this.reoGridView1.Model = this.model1;
            this.reoGridView1.Name = "reoGridView1";
            this.reoGridView1.Size = new System.Drawing.Size(807, 326);
            this.reoGridView1.TabIndex = 9;
            this.reoGridView1.Load += new System.EventHandler(this.reoGridView1_Load);
            // 
            // pagerWidget1
            // 
            this.pagerWidget1.Configuration = null;
            this.pagerWidget1.CurrentPage = ((long)(0));
            this.pagerWidget1.Location = new System.Drawing.Point(-2, 720);
            this.pagerWidget1.Name = "pagerWidget1";
            this.pagerWidget1.Size = new System.Drawing.Size(1069, 73);
            this.pagerWidget1.TabIndex = 7;
            this.pagerWidget1.TotalPage = ((long)(0));
            // 
            // searchWidget1
            // 
            this.searchWidget1.Configuration = this.configuration;
            this.searchWidget1.Location = new System.Drawing.Point(-2, 1);
            this.searchWidget1.Name = "searchWidget1";
            this.searchWidget1.Size = new System.Drawing.Size(1260, 77);
            this.searchWidget1.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1509, 793);
            this.Controls.Add(this.basicView1);
            this.Controls.Add(this.jsonRESTSynchronizer1);
            this.Controls.Add(this.model1);
            this.Controls.Add(this.configuration);
            this.Controls.Add(this.reoGridView1);
            this.Controls.Add(this.pagerWidget1);
            this.Controls.Add(this.searchWidget1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "FrontWork框架测试";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDoubleClick);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private FrontWork.SearchView searchWidget1;
        private FrontWork.PagerView pagerWidget1;
        private FrontWork.ReoGridView reoGridView1;
        private FrontWork.Configuration configuration;
        private FrontWork.Model model1;
        private FrontWork.JsonRESTSynchronizer jsonRESTSynchronizer1;
        private FrontWork.BasicView basicView1;
    }
}

