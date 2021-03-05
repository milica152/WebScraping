using System;
using System.IO;
using System.Net;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace WebScraping
{
    class Program
    {
        static void Main(string[] args)
        {

            // fill form parameters
            string html = Get();
            List<string> currencies = GetCurrencies(html);

            DateTime today = DateTime.Now;
            string end = today.ToString("yyyy-MM-dd");

            DateTime startDate = today.AddDays(-2);
            string start = startDate.ToString("yyyy-MM-dd");

            foreach (var c in currencies)
            {
                Console.WriteLine();                
                List<string> data = new List<string>();
                int page = 1;

                while(true)
                {
                    string responseHTML = Post(start, end, c, page);
                    Console.WriteLine("Scraping data for currency " + c + ", start date: " + start + " end date: " + end + " and page " + page.ToString() + ". . .");
                    List<string> temp = ScrapeData(responseHTML);
                    
                    if(page != 1 && temp.Count > 0)   // write header values only once and do not write error messages to CSV file
                    {
                        temp = temp.Skip(1).ToList();
                    }

                    data.AddRange(temp);
                    if (LastPage(responseHTML, page))   // there's no more data to scrape
                    {
                        break;
                    }
                    Console.WriteLine("Data for currency " + c + ", start date: " + start + " and end date: " + end + " is successfully scraped.");
                    page++;
                }
                
                
                if (data.Count > 1)   // table is not empty
                {
                    WriteCSV(data, c, start, end);
                }
                else if(data.Count == 1)    // table is empty, just write message to console 
                {
                    Console.WriteLine(data[0]);
                }
                else
                {
                    Console.WriteLine("Error happened while scraping data for currency " + c + ", start date: " + start + " and end date: " + end);
                }
            }
        }

        private static bool LastPage(string html, int page)
        {
            HtmlDocument htmlDoc = LoadHTML(html);
            var pageValue = htmlDoc.DocumentNode.SelectSingleNode("//form[@name='pageform']/input[@name='page']");
            string pageStr = pageValue.Attributes.Last().Value;
            
            try
            {
                int pageQueried = Int32.Parse(pageStr);
                if (pageQueried != page)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw new Exception("Error while parsing page number. . .");
            }
            return false;
        }

        private static void WriteCSV(List<string> data, string currency, string start, string end)
        {            
            var sb = new StringBuilder();

            foreach(string line in data)
            {
                sb.AppendLine(line);
            }
            string filename = currency + start + end + ".csv";
            string filepath = GetPath() + filename;
            Console.WriteLine("Writing to CSV file " + filepath + ". . .");

            File.WriteAllText(filepath, sb.ToString());

            Console.WriteLine("Data successfully written to CSV file.");
        }

        private static List<string> ScrapeData(string html)
        {
            List<string> returnString = new List<string>();
            HtmlDocument htmlDoc = LoadHTML(html);
            var table = htmlDoc.DocumentNode.SelectSingleNode("(//table)[3]");
            var rows = table.ChildNodes;
            int rowCounter = 0;

            foreach(var row in rows)
            {
                if (row.Name == "tr")    //iterating through rows
                {
                    int columnCounter = 0;

                    rowCounter += 1;
                    string stringRow = "";
                    var columns = row.ChildNodes;

                    foreach (var c in columns)    // iterating through columns\
                    {
                        if(c.Name == "td")
                        {
                            columnCounter += 1;
                            stringRow += "," + c.InnerHtml;
                        }
                    }
                    
                    stringRow = stringRow.Substring(1);
                    returnString.Add(stringRow);
                    if (columnCounter != 7)
                    {
                        return returnString;
                    }
                }
            }
            return returnString;
        }

        private static List<string> GetCurrencies(string html)
        {
            List<string> currencies = new List<string>();
            HtmlDocument htmlDoc = LoadHTML(html);
            var selectNode = htmlDoc.GetElementbyId("pjname");
            int counter = 0;

            foreach( var node in selectNode.ChildNodes.Descendants("#text"))
            {
                counter += 1;

                if (counter == 1)
                {
                    continue;
                }
                currencies.Add(node.InnerHtml);
            }
            return currencies;
        }

        public static string Post(string start, string end, string currency, int page)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://srh.bankofchina.com/search/whpj/searchen.jsp");

            string postData = "erectDate=" + start + "&nothing=" + end + "&pjname=" + currency + "&page=" + page.ToString();
            byte[] send = Encoding.Default.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = send.Length;

            Stream sout = request.GetRequestStream();
            sout.Write(send, 0, send.Length);
            sout.Flush();
            sout.Close();

            WebResponse res = request.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream());
            string html = sr.ReadToEnd();
            sr.Close();

            return html;
        }

        public static string Get()
        {
            string url = "https://srh.bankofchina.com/search/whpj/searchen.jsp";
            string html = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            return html;
        }

        private static HtmlDocument LoadHTML(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc;
        }

        private static string GetPath()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName)
            .AddJsonFile("appSettings.json", optional: true);

            IConfiguration config = builder.Build();
            return config.GetSection("filePath").Value;
        }
    }
}
