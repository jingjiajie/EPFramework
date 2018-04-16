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
using FrontWork;

namespace Test
{
    public partial class Form1 : Form,IMethodListener
    {
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
            //this.synchronizer = new JsonRESTSynchronizer();
            //this.synchronizer.Model = this.model;
            //this.synchronizer.SetPullAPI(@"http://localhost.fiddler:9000/ledger/WMS_Template/person/{{""conditions"":$conditions,""orders"":$orders}}",HTTPMethod.GET,"$data");
            //this.synchronizer.SetUpdateAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/",HTTPMethod.PUT,"$data");
            //this.synchronizer.SetAddAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/", HTTPMethod.POST, "$data");
            //this.synchronizer.SetRemoveAPI("http://localhost.fiddler:9000/ledger/WMS_Template/person/{mapProperty($data,'id')}", HTTPMethod.DELETE, null);
            //this.synchronizer.SetPushFinishedCallback(()=>
            //{
            //    MessageBox.Show("保存成功！","提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //});

            //this.synchronizer.SetPushFailedCallback((res, err) =>
            //{
            //    string message;
            //    if (res != null)
            //    {
            //        message = new StreamReader(res.GetResponseStream()).ReadToEnd();
            //    }
            //    else
            //    {
            //        message = err.Message;
            //    }
            //    MessageBox.Show("保存失败：" + message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return false;
            //});
            //this.searchWidget1.SetConfigurationFromJson(strMetadata,this);
            //this.searchWidgetJsonRESTAdapter = new SearchViewJsonRESTAdapter();
            //this.searchWidgetJsonRESTAdapter.Bind(this.searchWidget1, this.synchronizer);
            

            //this.pagerWidget1.TotalPage = 10;
            //this.pagerWidget1.CurrentPage = 5;
        }

        private void SearchWidget1_OnSearch(OnSearchEventArgs args)
        {
            Console.WriteLine(args.Conditions[0].Key);
            Console.WriteLine(args.Conditions[0].Relation);
            Console.WriteLine(args.Conditions[0].Values[0]);
            if (args.Orders?.Length > 0)
            {
                Console.WriteLine(args.Orders[0].Key);
                Console.WriteLine(args.Orders[0].Order);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.jsonRESTSynchronizer1.PushToServer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //this.synchronizer.PullFromServer();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.model1.RemoveRows(this.model1.FirstSelectionRange.Row, this.model1.FirstSelectionRange.Rows);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.model1.InsertRow(0,null);
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

        private void basicView1_Load(object sender, EventArgs e)
        {

        }

        private void jsonRESTSynchronizer1_Load(object sender, EventArgs e)
        {

        }

        private void model1_Load(object sender, EventArgs e)
        {

        }

        private void reoGridView1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show(this.Size.Width + " x " + this.Size.Height);
        }
    }
}
