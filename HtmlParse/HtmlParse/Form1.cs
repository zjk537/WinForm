using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace HtmlParse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            string content = txtContent.Text.Trim();
            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("内容为空!");
                return;
            }
            txtResult.Text = "";
            List<SectionModel> results = HtmlToList(content);
            StringBuilder sb = new StringBuilder();
            results.ForEach(item =>
            {
                sb.Append(item.ToString());
                sb.Append("\r\n,");
            });
            txtResult.Text = string.Format("[{0}]", sb.ToString().TrimEnd('\r', '\n', ','));
            txtResult.AppendText("拆分后总数组个数：" + results.Count);
        }

        private List<SectionModel> HtmlToList(string content)
        {
            content = content.Replace("&nbsp;", "").Replace("\r\n", "").Replace("\t", "");//去空格 回车 换行 制表符
            content = Regex.Replace(content, "style=.+?['|\"]", "", RegexOptions.IgnoreCase);// 去内联样式
            content = Regex.Replace(content, "class=.+?['|\"]", "", RegexOptions.IgnoreCase);// 去外联样式
            content = Regex.Replace(content, "<[p|br|div|span]+[ ]+.*?[/]?>", "<p>", RegexOptions.IgnoreCase);// 替换p span br div 为p
            content = Regex.Replace(content, "(<p>){2,}", "<p>", RegexOptions.IgnoreCase);// 连续两个以上的p 替换成一个p
            content = Regex.Replace(content, "<cbnblock[ ]*>", "<cbnblock><innercbnblock>", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, "<[^(>|descript|input|img|a|strong|iframe)]+>", "", RegexOptions.IgnoreCase);//去掉除指定标签外的所有html标签
            content = Regex.Replace(content, @"</((?!iframe|strong|descript|a>)\w)+>", "", RegexOptions.IgnoreCase);//去掉除指定标签外的所有html标签
            content = Regex.Replace(content, @"</((?!iframe|strong|descript|a>)\w)+>", "", RegexOptions.IgnoreCase);//去掉除指定标签外的所有html标签
            content = Regex.Replace(content, @"<!--.*-->", "", RegexOptions.IgnoreCase);//去掉除指定标签外的所有html标签
            string[] test = Regex.Split(content, "<p>");
            List<SectionModel> sections = new List<SectionModel>();
            foreach (string item in test)
            {
                string tempItem = item.Trim();
                if (string.IsNullOrEmpty(tempItem) || string.IsNullOrEmpty(Regex.Replace(tempItem, "</?strong>", ""))) // 跳过空内容 空标签
                {
                    continue;
                }

                Match mImg = Regex.Match(tempItem, "<input.*value=['|\"](.*?)['|\"] />", RegexOptions.IgnoreCase);
                Match mVideo = Regex.Match(tempItem, "<iframe.*src=['|\"](.*?)['|\"] />", RegexOptions.IgnoreCase);
                if (mImg.Length > 0) // 获取input 设置的 value 值 小物件为图片
                {
                    sections.Add(new SectionModel() { Type = "img", Content = mImg.Groups[1].Value });
                }
                else if (tempItem.StartsWith("<img")) // 图片
                {
                    sections.Add(new SectionModel() { Type = "img", Content = tempItem });
                }
                else if (Regex.IsMatch(tempItem, "^<strong>.*</strong>$")) // subtitle
                {
                    sections.Add(new SectionModel() { Type = "subtitle", Content = Regex.Replace(tempItem, "<[^>]+>", "") });
                }
                else if (mVideo.Length > 0) // video
                {
                    sections.Add(new SectionModel() { Type = "video", Content = mVideo.Groups[1].Value });
                }
                else
                {
                    sections.Add(new SectionModel() { Type = "text", Content = Regex.Replace(tempItem, "<[^>]+>", "") });
                }

            }
            return sections;

        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                ((TextBox)sender).SelectAll();
            }
        }

    }

    public class SectionModel
    {
        public string Type { get; set; }

        public string Content { get; set; }

        public override string ToString()
        {
            return string.Format("{{\"Type\":\"{0}\",\"Content\":\"{1}\"}}", this.Type, this.Content.Replace("\"", "\\\""));
        }
    }
}
