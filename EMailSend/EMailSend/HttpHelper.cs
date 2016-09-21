using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EMailSend
{
    public class HttpHelper
    {
        private static string HttpPost(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
            //request.CookieContainer = cookie;
            Stream myRequestStream = request.GetRequestStream();
            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
            myStreamWriter.Write(postDataStr);
            myStreamWriter.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //response.Cookies = cookie.GetCookies(response.ResponseUri);
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public static string PostData(string purl, string str)
        {

            byte[] data = System.Text.Encoding.GetEncoding("GB2312").GetBytes(str);
            // 准备请求 
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(purl);

            //设置超时
            req.Timeout = 30000;
            req.Method = "Post";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            Stream stream = req.GetRequestStream();
            // 发送数据 
            stream.Write(data, 0, data.Length);
            stream.Close();

            HttpWebResponse rep = (HttpWebResponse)req.GetResponse();
            Stream receiveStream = rep.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("GB2312");
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, encode);

            Char[] read = new Char[256];
            int count = readStream.Read(read, 0, 256);
            StringBuilder sb = new StringBuilder("");
            while (count > 0)
            {
                String readstr = new String(read, 0, count);
                sb.Append(readstr);
                count = readStream.Read(read, 0, 256);
            }

            rep.Close();
            readStream.Close();

            return sb.ToString();
        }

        public static string HttpGet(string Url, string paramStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (paramStr == "" ? "" : "?") + paramStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
    }
}
