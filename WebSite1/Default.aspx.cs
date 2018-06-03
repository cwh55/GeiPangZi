using System.Threading;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

public partial class _Default : System.Web.UI.Page
{
    public delegate void MyDelegate(string str);

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    public void WriteLog(string msg)
    {
        string filePath = AppDomain.CurrentDomain.BaseDirectory + "Log";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        string logPath = AppDomain.CurrentDomain.BaseDirectory + "Log\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

        using (StreamWriter sw = File.AppendText(logPath))
        {
            sw.WriteLine(msg);
            sw.WriteLine("时间：" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
            sw.WriteLine("**************************************************");
            sw.WriteLine();
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }

    }


    public void ExportToExcel(string FileName, string dt)
    {
        System.Web.HttpContext.Current.Response.Charset = "utf-8";
        System.Web.HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" +
            HttpUtility.UrlEncode(FileName, System.Text.Encoding.UTF8).ToString());
        System.Web.HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
        System.Web.HttpContext.Current.Response.ContentType = "application/ms-excel";
        //StringWriter tw = new StringWriter();
        System.Web.HttpContext.Current.Response.Output.Write(dt);
        System.Web.HttpContext.Current.Response.Flush();
        System.Web.HttpContext.Current.Response.End();
    }

    public string IgetNumber(string str)
    {
        return System.Text.RegularExpressions.Regex.Replace(str, @"[^\d{2}-]*", "");
    }

    //选择.txt文件并解析
    protected void Button1_Click(object sender, EventArgs e)
    {
        if (!fpFile.HasFile)
        {
            Response.Write("<script>alert('请上传文件')</script>");
            return;
        }

        string strfileExtension = System.IO.Path.GetExtension(fpFile.FileName).ToLower();//文件后缀名  

        if (strfileExtension != ".txt")
        {
            Response.Write("<script>alert('格式不对')</script>");
            return;
        }

        try
        {
            //Thread t1 = new Thread(test);
            //t1.IsBackground = true;//后台线程
            //t1.Start();

            test();

        }
        catch (Exception ex)
        {
            WriteLog(ex.Message);
            return;
        }
    }


    public void test()
    {
        string line;
        try
        {
            StreamReader sr = new StreamReader(fpFile.FileContent, Encoding.Default, false, 1024);

            HtmlWeb web = new HtmlWeb();
            WriteLog("开始时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                int num = 0;
                int i = 0;
                string html = string.Empty;

                FileMsgQueue<HtmlDocument> msgQueue = new FileMsgQueue<HtmlDocument>();
                //HtmlDocument returnInfo;

                while ((line = sr.ReadLine()) != null)
                {
                    bool isTrueNums = true;
                    i++;

                    if (string.IsNullOrEmpty(line))
                    {
                        isTrueNums = false;
                        break;
                    }

                    bool b = Regex.IsMatch(line, "[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+.?");
                    if (!b)
                    {
                        isTrueNums = false;
                        WriteLog(i + "行不是域名，不解析------" + line);
                        continue;
                    }

                    #region
                    
                    HtmlDocument doc = web.Load("https://www.baidu.com/s?wd=" + line);

                   msgQueue.Send(doc);


                    HtmlNode row = doc.DocumentNode.SelectSingleNode("//span[@class='nums_text']");
                    if (row == null)
                    {
                        isTrueNums = false;
                        WriteLog(i + "行,不是首页第一，不解析" + line);
                        continue;
                    }

                    num = Convert.ToInt32(IgetNumber(row.InnerHtml));
                    if (num == 0)
                    {
                        isTrueNums = false;
                        WriteLog(i + "行,不是首页第一，不解析:" + line);
                        continue;
                    }

                    HtmlNode row2 = doc.DocumentNode.SelectSingleNode("//font[@class='c-gray']");
                    if (row2 == null)
                    {
                        isTrueNums = false;
                        WriteLog(i + "行,不是首页第一，不解析:" + line);
                        continue;
                    }

                    if (row2.InnerHtml.IndexOf("没有找到该URL") > -1)
                    {
                        isTrueNums = false;
                        WriteLog(i + "行，没有找到该URL，不解析" + line);
                        continue;
                    }
                    #endregion

                   

                    if (isTrueNums)
                    {
                        html += "<tr><td>" + line + "</td><td>" + num + "</td></tr>";
                    }

                }

                string pHtml = string.Empty;
                pHtml += "<table  border='1' cellspacing='1' cellpadding='1'><thead>";
                pHtml += "<tr><th>URL</th><th>结果数</th></tr>";
                pHtml += "</thead><tbody>";

                if (!string.IsNullOrEmpty(html))
                {
                    pHtml += html;
                    pHtml += "</tbody></table>";
                    ExportToExcel("符合条件的URL_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls", pHtml);
                    WriteLog("结束时间" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    WriteLog("导出0条数据");
                    return;
                }
            //}
        }
        catch (Exception ex)
        {
            WriteLog("异常信息"+ex.Message);
            WriteLog("详细信息----" + ex.StackTrace);
            return;
        }
    }


}