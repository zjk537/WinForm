using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using System.Data.OleDb;
using log4net;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Threading;
using System.Security.Cryptography;

namespace EMailSend
{
    public partial class Form1 : Form
    {
        private static readonly ILog log = LogManager.GetLogger("EMailSend");
        public Form1()
        {
            InitializeComponent();
        }

        string smtpHost = string.Empty,
            hostMail = string.Empty,
            hostPwd = string.Empty;
        int hostPort = 0;
        int successCount = 0, failCount = 0, totalCount = 0, emailErrorCount = 0, emailEmptyCount = 0;

        List<FileInfo> attachFiles = new List<FileInfo>();
        private void Form1_Load(object sender, EventArgs e)
        {
            //btnSend_Click(null, null);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string filePath = this.txtFilePath.Text;
            string tmplPath = this.txtTmpl.Text;
            //string filePath = string.Format(@"D:/Application/YiCai/CBNWeek/EMailSend/Code_{0}.xls", DateTime.Now.ToString("yyyyMMdd"));
            smtpHost = this.txtSMTPHost.Text.Trim();
            hostMail = this.txtEmail.Text.Trim();
            hostPwd = this.txtPwd.Text.Trim();
            string strPort = this.txtPort.Text.Trim();

            if (string.IsNullOrEmpty(smtpHost))
            {
                MessageBox.Show("SMTP服务器不能为空！");
                this.txtSMTPHost.Focus();
                return;
            }

            if (string.IsNullOrEmpty(strPort))
            {
                MessageBox.Show("端口不能为空！");
                this.txtPort.Focus();
                return;
            }

            if (string.IsNullOrEmpty(hostMail))
            {
                MessageBox.Show("邮箱地址不能为空！");
                this.txtEmail.Focus();
                return;
            }

            if (string.IsNullOrEmpty(hostPwd))
            {
                MessageBox.Show("邮箱密码不能为空！");
                this.txtPwd.Focus();
                return;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("请选择数据源文件");
                return;
            }
            if (string.IsNullOrEmpty(tmplPath))
            {
                MessageBox.Show("请选择邮件模板文件");
                return;
            }
            hostPort = Convert.ToInt32(strPort);
            if (hostPort == 0)
            {
                MessageBox.Show("端口号只能为数字");
                this.txtPort.Clear();
                this.txtPort.Focus();
                return;
            }
            Dictionary<string, DataTable> dtDic = ToDataTable(filePath);
            if (dtDic == null)
            {
                MessageBox.Show("结果集为空，请检查Excel 数据源");
                return;
            }

            successCount = 0; failCount = 0; totalCount = 0; emailErrorCount = 0; emailEmptyCount = 0;

            //StreamReader sr = new StreamReader(System.Environment.CurrentDirectory + "/lastNum.txt");
            //int lastNum = Convert.ToInt32(sr.ReadToEnd());

            this.btnSend.Enabled = false;
            this.btnSend.Text = "开始发送";
            string mailBody = ReadHtmlToStr(tmplPath);
            foreach (string strName in dtDic.Keys)
            {
                DataTable dt = dtDic[strName];
                totalCount += dt.Rows.Count;
                log.Info(string.Format("开始发送{0}邮件，合计：{1} 封邮件", strName, dt.Rows.Count));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var email = dt.Rows[i][0].ToString();
                    var code = dt.Rows[i][1].ToString();
                    var smsType = strName.Trim('\'').TrimEnd('$');
                    //SendMailUseQQ(email, code, smsType);
                    SendMail(email, code, smsType, mailBody, attachFiles);
                }
            }
            //MessageBox.Show("邮件发送完毕！");
            log.Info("邮件发送完毕！等待发送结果...");
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            //fd.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            fd.Filter = "Excel|*.xls|Excel|*.xlsx";
            fd.RestoreDirectory = true;
            fd.FilterIndex = 1;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                this.txtFilePath.Text = fd.FileName;
            }
            else
            {
                this.txtFilePath.Text = "";
            }

        }

        private void btnTmpl_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            //fd.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            fd.Filter = "Html|*.html|Text|*.txt";
            fd.RestoreDirectory = true;
            fd.FilterIndex = 1;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                this.txtTmpl.Text = fd.FileName;
            }
            else
            {
                this.txtTmpl.Text = "";
            }
        }

        private void btnAttach_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            //fd.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            fd.Filter = "image|*.bmp;*.jpg;*.gif;*.jpeg;*.png|rar;zip|*.rar;*.zip|Office|*.doc;*.docx;*.xls;*.xlsx;*.ppt|pdf|*.pdf";
            fd.Multiselect = true;
            fd.RestoreDirectory = true;
            fd.FilterIndex = 1;
            if (fd.ShowDialog() == DialogResult.OK)
            {

                foreach (string s in fd.FileNames)
                {
                    FileInfo f = new FileInfo(s);
                    attachFiles.Add(f);
                    this.txtAttach.Text += f.Name + ";";
                }
            }
            else
            {
                attachFiles.Clear();
                this.txtAttach.Text = "";
            }
        }

        private void SendMail(string email, string code, string smsType, string mailBody, List<FileInfo> attachFiles)
        {
            if (string.IsNullOrEmpty(email))
            {
                emailEmptyCount++;
                totalCount--;
                log.Info("邮箱为空！跳过。");
                return;
            }
            if (!isEmail(email))
            {
                emailErrorCount++;
                totalCount--;
                log.Info(string.Format("邮箱地址错误：{0}|{1}", smsType, email));
                return;
            }
            try
            {
                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.To.Add(email);
                msg.From = new MailAddress(hostMail, "第一财经周刊", System.Text.Encoding.UTF8);
                /* 上面3个参数分别是发件人地址（可以随便写），发件人姓名，编码*/
                msg.Subject = GetMailTitle(mailBody);//邮件标题 
                msg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码 
                msg.Body = mailBody.Replace("[code]", code);//邮件内容

                for (int i = 0; i < attachFiles.Count; i++)
                {
                    FileInfo file = attachFiles[i];
                    if (file != null)
                    {
                        msg.Attachments.Add(new Attachment(file.FullName));//邮件附件
                        msg.Attachments[i].ContentType.Name = MimeMapping.GetMimeMapping(file.Name);
                        msg.Attachments[i].ContentId = string.Format("attach_{0}_{1}", smsType, i);
                        msg.Attachments[i].ContentDisposition.Inline = true;
                        msg.Attachments[i].TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                    }
                }

                msg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码 
                msg.IsBodyHtml = true;//是否是HTML邮件 
                msg.Priority = MailPriority.Normal;//邮件优先级 
                object userState = email;

                SmtpClient client = new SmtpClient();
                client.Credentials = new System.Net.NetworkCredential(hostMail, hostPwd);
                client.Port = hostPort;//使用的端口 
                client.Host = smtpHost;
                client.EnableSsl = true;//经过ssl加密 
                client.SendCompleted += new SendCompletedEventHandler(smtp_SendCompleted);

                client.SendAsync(msg, userState);
                //Thread.Sleep(1000 * 3);
                //简单一点儿可以client.Send(msg); 
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                failCount++;
                //MessageBox.Show(ex.Message, "发送邮件出错");
                log.Error(string.Format("发送邮件出错：{0}|{1}|{2}", email, code, smsType), ex);
                // 要保证最后一个邮箱地址正确且不能为空
                if (totalCount == 0)
                {
                    string resultMsg = string.Format("邮件发送成功{0},失败{1},邮箱地址错误{2}", successCount, failCount, emailErrorCount);
                    log.Info(resultMsg);
                    this.btnSend.Enabled = true;
                    MessageBox.Show(resultMsg);
                }
            }
            catch (Exception e)
            {
                log.Error(string.Format("发送邮件出错：{0}|{1}|{2}", email, code, smsType), e);
            }
        }
        //通过QQ的SMTP
        public void SendMailUseQQ(string email, string code, string smsType)
        {
            if (string.IsNullOrEmpty(email))
            {
                log.Info("邮箱为空！跳过。");
                return;
            }
            if (!isEmail(email))
            {
                log.Info(string.Format("邮箱地址错误：{0}|{1}", smsType, email));
                return;
            }
            string mailBody = ReadHtmlToStr(smsType);
            System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
            msg.To.Add(email);
            msg.From = new MailAddress("zkkf@cbnweek.com", "第一财经周刊", System.Text.Encoding.UTF8);
            /* 上面3个参数分别是发件人地址（可以随便写），发件人姓名，编码*/
            msg.Subject = GetMailTitle(mailBody);//邮件标题 
            msg.SubjectEncoding = System.Text.Encoding.UTF8;//邮件标题编码 
            msg.Body = mailBody.Replace("[code]", code);//邮件内容

            string attachFile = string.Format("{0}/Attachments_{1}.png", System.Environment.CurrentDirectory, smsType);
            DirectoryInfo dir = new DirectoryInfo(System.Environment.CurrentDirectory);
            FileSystemInfo[] files = dir.GetFileSystemInfos(string.Format("Attach_{0}*", smsType));
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                if (file != null)
                {
                    msg.Attachments.Add(new Attachment(file.FullName));//邮件附件
                    msg.Attachments[i].ContentType.Name = MimeMapping.GetMimeMapping(file.Name);
                    msg.Attachments[i].ContentId = string.Format("attach_{0}_{1}", smsType, i);
                    msg.Attachments[i].ContentDisposition.Inline = true;
                    msg.Attachments[i].TransferEncoding = System.Net.Mime.TransferEncoding.Base64;
                }
            }

            msg.BodyEncoding = System.Text.Encoding.UTF8;//邮件内容编码 
            msg.IsBodyHtml = true;//是否是HTML邮件 
            msg.Priority = MailPriority.Normal;//邮件优先级 
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential(hostMail, hostPwd);
            client.Port = hostPort;//使用的端口 
            client.Host = smtpHost;
            client.EnableSsl = true;//经过ssl加密 
            client.SendCompleted += new SendCompletedEventHandler(smtp_SendCompleted);
            object userState = email;
            try
            {
                client.SendAsync(msg, userState);
                //简单一点儿可以client.Send(msg); 
                //MessageBox.Show("发送成功");
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                //MessageBox.Show(ex.Message, "发送邮件出错");
                log.Error(string.Format("发送邮件出错：{0}|{1}|{2}", email, code, smsType), ex);
            }
        }

        private void smtp_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {

            string email = e.UserState.ToString();
            if (e.Error == null)//问题出现后，这里的Error并没有错误
            {
                successCount++;
                totalCount--;
                this.btnSend.Text = string.Format("已成功发送{0}条", successCount);
            }
            else
            {
                failCount++;
                totalCount--;
                log.Info(string.Format("邮箱{0}发送失败,原因:{1}", email, e.Error.ToString()));
            }
            // 要保证最后一个邮箱地址正确且不能为空
            if (totalCount == 0)
            {
                string resultMsg = string.Format("邮件发送成功{0},失败{1},邮箱地址错误{2}", successCount, failCount, emailErrorCount);
                log.Info(resultMsg);
                this.btnSend.Enabled = true;
                MessageBox.Show(resultMsg);
            }

            SmtpClient client = (SmtpClient)sender;
            client.Dispose();
        }

        public static Dictionary<string, DataTable> ToDataTable(string filePath)
        {
            Dictionary<string, DataTable> resultDict = new Dictionary<string, DataTable>();
            string connStr = "";
            string fileType = System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(fileType)) return null;

            if (fileType == ".xls")
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            else
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + filePath + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            string sql_F = "Select * FROM [{0}]";

            OleDbConnection conn = null;
            OleDbDataAdapter da = null;
            DataTable dtSheetName = null;
            try
            {
                // 初始化连接，并打开
                conn = new OleDbConnection(connStr);
                conn.Open();

                // 获取数据源的表定义元数据                        
                string SheetName = "";
                dtSheetName = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                // 初始化适配器
                da = new OleDbDataAdapter();
                for (int i = 0; i < dtSheetName.Rows.Count; i++)
                {
                    SheetName = (string)dtSheetName.Rows[i]["TABLE_NAME"];

                    if (SheetName.Contains("$") && !SheetName.Replace("'", "").EndsWith("$"))
                    {
                        continue;
                    }

                    da.SelectCommand = new OleDbCommand(String.Format(sql_F, SheetName), conn);
                    DataSet dsItem = new DataSet();
                    da.Fill(dsItem, "Table");
                    resultDict.Add(SheetName.TrimEnd('$'), dsItem.Tables[0].Copy());
                }
            }
            catch (Exception ex)
            {
                log.Error("读取Excel 文件出错：", ex);
            }
            finally
            {
                // 关闭连接
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    da.Dispose();
                    conn.Dispose();
                }
            }
            return resultDict;
        }

        private string ReadHtmlToStr(string tmplPath)
        {
            System.IO.StreamReader rd = new System.IO.StreamReader(tmplPath);
            string str = rd.ReadToEnd();
            return str;
        }


        private string GetMailTitle(string mailBody)
        {
            string mailTitle = "";
            Regex reg = new Regex(@"(?<=title\>).*(?=</title)");
            mailTitle = reg.Match(mailBody).ToString(); ;
            return mailTitle;
        }

        private bool isEmail(string strEmail)
        {
            Regex emailReg = new Regex(@"(?:[a-zA-Z0-9]+[_\-\+\.]?)*[a-zA-Z0-9]+@(?:([a-zA-Z0-9]+[_\-]?)*[a-zA-Z0-9]+\.)+([a-zA-Z]{2,})+");
            return emailReg.IsMatch(strEmail);
        }


       
    }


}
