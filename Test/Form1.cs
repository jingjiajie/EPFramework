using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using EditPanelFramework;

namespace Test
{
    public partial class Form1 : Form,IMethodListener
    {
        private IModel model;
        private IView tableLayoutView,reoGridView;
        private JsonRESTSynchronizer adapter;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string strMetadata = @"[{
                                      mode:'default',
                                      fields:[{name:'id',displayName:'ID',visible:false},
                                              {name:'name',displayName:'姓名',association:'nameAssociation'},
                                              {name:'password',displayName:'密码',association:'nameAssociation'},
                                              {name:'role',displayName:'角色'},
                                              {name:'authorityString',displayName:'权限字符串'}]
                                  }]";
            this.model = new Model(strMetadata);
            this.tableLayoutView = new TableLayoutPanelView(this.tableLayoutPanel1);
            this.tableLayoutView.SetMetaDataFromJson(strMetadata, this);
            this.tableLayoutView.Model = this.model;
            this.reoGridView = new ReoGridWorksheetView(this.reoGridControl1);
            this.reoGridView.SetMetaDataFromJson(strMetadata, this);
            this.reoGridView.Model = this.model;
            this.adapter = new JsonRESTSynchronizer();
            this.adapter.Model = this.model;
            this.adapter.SetPullAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/{}",HTTPMethod.GET,"$data");
            this.adapter.SetUpdateAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/",HTTPMethod.PUT,"$data");
            this.adapter.SetAddAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/", HTTPMethod.POST, "$data");
            this.adapter.SetRemoveAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/{mapProperty($data,'id')}", HTTPMethod.DELETE, null);
            this.adapter.SetPushFinishedCallback(()=>
            {
                MessageBox.Show("保存成功！","提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            this.adapter.SetPushFailedCallback((res, err) =>
            {
                string message;
                if (res != null)
                {
                    message = new StreamReader(res.GetResponseStream()).ReadToEnd();
                }
                else
                {
                    message = err.Message;
                }
                MessageBox.Show("保存失败：" + message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.adapter.PushToServer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.adapter.PullFromServer();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.model.RemoveRows(this.model.FirstSelectionRange.Row, this.model.FirstSelectionRange.Rows);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.model.InsertRow(0,null);
        }

        public AssociationItem[] NameAssociation(string str)
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://localhost.fiddler:9000/ledger/WMS_Template/person/{}");
            req.Method = "GET";
            req.Timeout = 100000;
            var res = req.GetResponse();
            string resStr = new StreamReader(res.GetResponseStream()).ReadToEnd();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var dics = serializer.Deserialize<Dictionary<string, object>[]>(resStr);
            return (from dic in dics select new AssociationItem() { Word = dic["name"].ToString() }).ToArray();
            //return new AssociationItem[] { new AssociationItem() { Word = str } };
        }
    }
}
