using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EMailSend
{
    public partial class SMS : Form
    {
        private static readonly ILog log = LogManager.GetLogger("SMSSend");
        public SMS()
        {
            InitializeComponent();
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

            regCount = newCount = failCount = successCount = 0;
        }

        int regCount = 0,
            newCount = 0,
            failCount = 0,
            successCount = 0;
        private void btnSend_Click(object sender, EventArgs e)
        {
            string filePath = this.txtFilePath.Text;
            string regSmsContent = this.txtRegSMS.Text.Trim();
            string noRegSmsContent = this.txtNoRegSMS.Text.Trim();
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("请选择数据源文件");
                return;
            }

            if (string.IsNullOrEmpty(regSmsContent))
            {
                MessageBox.Show("老用户短信消息内容为空");
                return;
            }

            if (string.IsNullOrEmpty(noRegSmsContent))
            {
                MessageBox.Show("新用户短信消息内容为空");
                return;
            }

            Dictionary<string, DataTable> dtDic = ToDataTable(filePath);
            if (dtDic == null)
            {
                MessageBox.Show("结果集为空，请检查Excel 数据源");
                return;
            }

            this.btnSend.Enabled = false;
            this.btnSend.Text = "开始发送";
            foreach (string strName in dtDic.Keys)
            {
                DataTable dt = dtDic[strName];
                log.Info(string.Format("开始发送{0}短信，合计：{1} 封短信", strName, dt.Rows.Count));
                log.Info("等待发送结果...");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var phone = dt.Rows[i][2].ToString();
                    SendSMS(phone, noRegSmsContent);
                    //var pwd = dt.Rows[i][1].ToString().Trim();
                    //if (string.IsNullOrEmpty(pwd))
                    //{
                    //    newCount++;
                    //    continue; // 因为之前发错了，新用户发了，老用户没发，所以第二次跳过新用户
                    //    //CreateDefaultUser(phone, noRegSmsContent);
                    //}
                    //else
                    //{
                    //    regCount++;
                    //    AddPermission(phone, pwd, regSmsContent);
                    //}
                    System.Threading.Thread.Sleep(100);
                }
            }
            this.btnSend.Enabled = true;

            MessageBox.Show("短信发送完毕！");
        }

        #region MyRegion
        /// <summary>
        /// 发送短信
        /// </summary>
        private void SendSMS(string toPhone, string smsContnet)
        {
            try
            {
                string smsuid = "cbnyicaiwang";
                string smspwd = "998877";
                string url = "http://service.winic.org:8009/sys_port/gateway/index.asp";
                //string postDataStr = string.Format("id=" + smsuid + "&pwd=" + smspwd + "&to=" + tos + //"&content=" + msg + "&time=")
                string postDataStr = string.Format("id={0}&pwd={1}&to={2}&content={3}&time=", smsuid, smspwd, toPhone, smsContnet);
                string resultStr = HttpHelper.PostData(url, postDataStr);
                var resultArr = resultStr.Split('/');
                if (resultArr[0] == "000")
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                    log.Error(string.Format("手机号：{0} 短信发送失败，原因：{1}", toPhone, getErrorMsg(resultArr[0])));
                }
            }
            catch (Exception e)
            {
                failCount++;
                log.Error(string.Format("手机号：{0} 短信发送失败，原因：{1}", toPhone, e.Message));
            }

            this.lblResult.Text = string.Format("老用户{0},新册用户{1},成功发送{2},失败{3}", regCount, newCount, successCount, failCount);
        }

        private string getErrorMsg(string resultCode)
        {
            string resultMsg = "";
            switch (resultCode)
            {
                case "000":
                    resultMsg = "成功";
                    break;
                case "-01":
                    resultMsg = "当前账号余额不足";
                    break;
                case "-02":
                    resultMsg = "当前用户ID错误";
                    break;
                case "-03":
                    resultMsg = "当前密码错误";
                    break;
                case "-04":
                    resultMsg = "参数不够或参数内容的类型错误";
                    break;
                case "-05":
                    resultMsg = "手机号码格式不对";
                    break;
                case "-06":
                    resultMsg = "短信内容编码不对";
                    break;
                case "-07":
                    resultMsg = "短信内容含有敏感字符";
                    break;
                case "-08":
                    resultMsg = "无接收数据";
                    break;
                case "-09":
                    resultMsg = "系统维护中...";
                    break;
                case "-10":
                    resultMsg = "手机号码数量超长";
                    break;
                case "-11":
                    resultMsg = "短信内容超长！（70个字符）";
                    break;
                case "-12":
                    resultMsg = "其它错误";
                    break;
                case "-13":
                    resultMsg = "文件传输错误";
                    break;
                default:
                    resultMsg = "读取接口失败";
                    break;
            }
            return resultMsg;
        }
        /// <summary>
        /// 创建默认用户
        /// </summary>
        /// <param name="phone"></param>
        private void CreateDefaultUser(string phone, string smsContnet)
        {
            try
            {
                string url = "http://www.cbnweek.com/v/phone_reg_api";
                string pwd = "cbnweekly";
                string paramStr = string.Format("phone={0}&pwd={1}&nickname={0}", phone, pwd);
                string result = HttpHelper.HttpGet(url, paramStr);
                ResultModel model = (ResultModel)JsonConvert.DeserializeObject(result, typeof(ResultModel));
                if (model.result)
                {
                    AddPermission(phone, Md5(pwd), smsContnet);
                }
                else
                {
                    failCount++;
                    log.Error(string.Format("手机号：{0} 创建新用户失败，原因：{1}", phone, model.msg));
                }
            }
            catch (Exception e)
            {
                failCount++;
                log.Error(string.Format("手机号：{0} 创建新用户失败，原因：{1}", phone, e.Message));
            }
        }

        /// <summary>
        /// 为用户添加权限
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="pwd"></param>
        private void AddPermission(string phone, string pwd, string smsContnet)
        {
            try
            {
                //password需要MD5
                string tranId = phone + DateTime.Now.ToString("yyMMddHHmmss");
                string auth = Md5(string.Format("{0}*{1}*_Pay_weekly", phone, tranId));
                //string url = "http://www.cbnweek.com/v/user_permissions_api";
                string url = "http://www.cbnweek.com/v/user_permissionsTest_api";
                string paramStr = string.Format("username={0}&password={1}&transaction_id={2}&auth={3}&time=3", phone, pwd, tranId, auth);
                string result = HttpHelper.HttpGet(url, paramStr);
                PermissionModel model = (PermissionModel)JsonConvert.DeserializeObject(result, typeof(PermissionModel));
                if (model.Code == 200)
                {
                    SendSMS(phone, smsContnet);
                }
                else
                {
                    failCount++;
                    log.Error(string.Format("手机号：{0} 创建新用户失败，原因：{1}", phone, model.Data));
                }
            }
            catch (Exception e)
            {
                failCount++;
                log.Error(string.Format("手机号：{0} 添加用户权限失败，原因：{1}", phone, e.Message));
            }

        }
        #endregion


        static string Md5(string str)
        {
            byte[] result = Encoding.Default.GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
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
                    if (SheetName.StartsWith("hide")) continue;
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


        private class ResultModel
        {
            public bool result { get; set; }
            public string msg { get; set; }
        }

        private class PermissionModel
        {
            public int Code { get; set; }
            public object Data { get; set; }
        }
    }



}
