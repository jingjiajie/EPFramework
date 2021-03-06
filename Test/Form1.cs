﻿using System;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
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
            this.jsonRESTSynchronizer1.PullFromServer();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.model1.RemoveRows(this.model1.SelectionRange.Row, this.model1.SelectionRange.Rows);
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

        private void pagerView1_Load(object sender, EventArgs e)
        {

        }

        public string[] RoleValues()
        {
            return new string[] { "管理员","普通用户"};
        }
    }
}
