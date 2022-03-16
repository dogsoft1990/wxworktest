using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wxworktest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string url = "";

        string Icon_url = "";

        string Nick_name = "";

        string vid = "";

        string guid = "";



        bool ishasimages = false;

        public static string GetRegexstring(string inputstr, string pattentstr)
        {

            Regex r = new Regex(pattentstr, RegexOptions.IgnoreCase | RegexOptions.Singleline);


            return r.Match(inputstr).Groups[1].Value;


        }

        public static List<string> GetRegexArrayList(string inputstr, string pattentstr)
        {
            List<string> tem = new List<string>();

            Regex r = new Regex(pattentstr, RegexOptions.IgnoreCase | RegexOptions.Singleline);


            foreach (Match m in r.Matches(inputstr))
            {

                tem.Add(m.Groups[1].Value);

            }

            return tem;
        }

        public async Task<string> GetJson(string method, string data)
        {


            string result = await data.SendWxWorkPostData(url + "" + method);


            Msg(richTextBox1, result);

            return result;

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (guid.Length < 1)
            {
                MessageBox.Show("guid长度不正确，请先获取");
            }
            else
            {

                string data = await GetJson("api/Login/WXGetQRCode", "{\"Guid\":\""+guid+"\"}");

                RestfulData<object> h5 = JsonConvert.DeserializeObject<RestfulData<object>>(data);

                string bytee =GetRegexstring(data, @"Qrcode"":""(?<Name>[^""]*?)""");

                Bitmap bitmap = new Bitmap(new MemoryStream(Convert.FromBase64String(bytee)));

                pbQrCode.Image = bitmap;


                button1.Enabled = false;

                button1.Text = "等待扫描中";

                timer1.Enabled = true;

            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            url = textBox1.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1_TextChanged(null,null);
        }

        private async void button2_Click(object sender, EventArgs e)
        {

          string data=  await GetJson("api/Client/WXCreate", "");

          RestfulData<OpenGuid> h5 = JsonConvert.DeserializeObject<RestfulData<OpenGuid>>(data);

           guid = h5.data.Guid;

          textBox2.Text = guid;
        }



        public void Msg(RichTextBox richTextBox, string Msg)
        {



            base.BeginInvoke((Action)delegate ()
            {


                if (richTextBox.Lines.Length > 1000)
                {
                    richTextBox.Clear();
                }

                richTextBox.AppendText(Msg + "\r\n");

                richTextBox.Select(richTextBox.TextLength, 0);

                richTextBox.ScrollToCaret();


            });


        }

        private async void button3_Click(object sender, EventArgs e)
        {

        }


        private async Task<bool> login()
        {

            string data = await GetJson("api/Login/WXLogin", "{\"Guid\":\"" + guid + "\"}");

            string str = GetRegexstring(data, @"message"":""(?<Name>[^""]*?)""");

            if (str == "success")
            {

                return true;
            }
            else
            {
                return false;

            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {

            string data = await GetJson("api/Login/WXCheckQRCode", "{\"Guid\":\"" + guid + "\"}");

            int Status = -1;
            try
            {


                Status = Convert.ToInt32(GetRegexstring(data, @"Status"":(?<Name>[^,]*?),"));
            }
            catch
            {
                Status = -2;

            }

            switch (Status)
            {
                case 0:
                    break;
                case 1:

                    if (!ishasimages)
                    {


                        Icon_url = GetRegexstring(data, @"Icon_url"":""(?<Name>[^""]*?)""");


                        Nick_name = GetRegexstring(data, @"Nick_name"":""(?<Name>[^""]*?)""");

                        vid = GetRegexstring(data, @"Vid"":""(?<Name>[^""]*?)""");


                       byte[] img= await Icon_url.GetBytesAsync();

                        Bitmap bitmap = new Bitmap(new MemoryStream(img));

                        pbQrCode.Image = bitmap;

                        button1.Text = "等待确认中";

                        label4.Text = Nick_name;

                        label6.Text = vid;

                        ishasimages = true;

                    }


                    break;
                case 2:


                   bool isok= await login();

                    if (isok)
                    {

                        await GetJson("api/Msg/WXGetSessionList", "{\"Guid\":\"" + guid + "\",\"beginseq\": 0}");


                        button1.Text = "登录成功";

                        timer1.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("登录失败！");
                        timer1.Enabled = false;
                        pbQrCode.Image = null;
                        button1.Text = "获取二维码";
                        ishasimages = false;
                        button1.Enabled = true;

                        label4.Text = "";

                        label6.Text = "";
                    }
                    break;
                case 4:
                    MessageBox.Show("对方取消！");
                    timer1.Enabled = false;
                    pbQrCode.Image = null;
                    button1.Text = "获取二维码";
                    ishasimages = false;
                    button1.Enabled = true;

                    label4.Text = "";

                    label6.Text = "";


                    break;
                default:
                    MessageBox.Show("其他错误！");
                    timer1.Enabled = true;
                    pbQrCode.Image = null;
                    button1.Text = "获取二维码";
                    ishasimages = false;
                    button1.Enabled = true;

                    label4.Text = "";

                    label6.Text = "";
                    break;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //39a52326-ead3-4b94-a7e1-7965f8d47a75
            guid = textBox2.Text;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(url+ "swagger/index.html");
        }

        private async void button6_Click(object sender, EventArgs e)
        {
      
           string data = await GetJson("api/Msg/WXGetSessionList", "{\"Guid\":\"" + guid + "\",\"beginseq\": 0}");


           MessageBox.Show("请根据消息类型，自行通过PB格式解析");

        }

        private async void button7_Click(object sender, EventArgs e)
        {
            if (textBox3.Text.Length < 8 || textBox4.Text.Length < 1)
            {

                MessageBox.Show("长度不正确");
            }
            else
            {

                //{
                //                  "Guid": "string",
                //"vid": 0,
                //"roomid": 0,
                //"content": "string",
                //"sendname": "string",
                //\"atlist\": [{\"uin\": 0,\"name\": \"string\"}],\"atAll\": 0
                //}


                string c = "0";

                string c1 = "0";

                if (radioButton1.Checked == true)
                {

                    c = textBox3.Text;
                }
                else
                {

                    c1 = textBox3.Text;
                }

                string tt = "{\"Guid\":\"" + guid + "\",\"vid\":"+ c+ ",\"roomid\":"+c1+",\"content\":\""+textBox4.Text+ "\",\"sendname\": \"string\",\"atlist\": [{\"uin\": 0,\"name\": \"string\"}],\"atAll\": 0}";

                string data = await GetJson("api/Msg/WXSendText", tt);

                MessageBox.Show("请在日志区查看");

            }

        }

        private async void button3_Click_1(object sender, EventArgs e)
        {
            string data = await GetJson("api/User/WXGetIntUserList", "{\"Guid\":\"" + guid + "\"}");

            List<string> ss = GetRegexArrayList(data, @"""uin"":""(?<Name>[^}]*?)"",""emailaddr");


            for (int i = 0; i < ss.Count; i++)
            {


                ss[i] = ss[i].Replace("\",\"name\":\"","-");


                string[] s = ss[i].Split('-');


                listBox1.Items.Add(s[0]+"-"+Encoding.UTF8.GetString(Convert.FromBase64String(s[1])));

                Msg(richTextBox1,ss[i]);
            }






        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string data = await GetJson("api/User/WXGetOutUserList", "{\"Guid\":\"" + guid + "\",\"seq\": 0}");


            List<string> ss = GetRegexArrayList(data, @"""uin"":""(?<Name>[^}]*?)"",""emailaddr");


            for (int i = 0; i < ss.Count; i++)
            {

                ss[i] = ss[i].Replace("\",\"name\":\"", "-");


                string[] s = ss[i].Split('-');

               listBox2.Items.Add(s[0] + "-" + Encoding.UTF8.GetString(Convert.FromBase64String(s[1])));

                Msg(richTextBox1, ss[i]);
            }
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            string data = await GetJson("api/Room/WXGetIntRoomIDList", "{\"Guid\":\"" + guid + "\"}");


            List<string> ss = GetRegexArrayList(data, @"""id"":""(?<Name>[^}]*?)"",""fw_id");


            for (int i = 0; i < ss.Count; i++)
            {


                listBox3.Items.Add(ss[i]);

                Msg(richTextBox1, ss[i]);
            }
    
        }
    }
}
