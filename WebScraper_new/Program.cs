using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Net;


namespace WebScraper_new
{
    class Program
    {
        static void Main(string[] args)
        {

            CreateFolder();
            AddRecord("Name","Description","Image Path");
            GetHtmlAsync();
            Console.ReadLine();
          
        }

        private static async void GetHtmlAsync()
        {
            string baseUrl = "https://townshiptale.gamepedia.com/";
            string indexAdd = "Category:Items";
            string firstUrl = baseUrl + indexAdd;


            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(firstUrl);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);



            var ProductsList = htmlDocument.DocumentNode.Descendants("div")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("mw-category")).ToList();

            var names = ProductsList[0].SelectNodes("//li/a");

            Console.WriteLine("Adding items into the file:");

            for (int i = 0; i < 46; i++)
            {
                string title=names[i].InnerText;
                string currentAddUrl = title;

                if (currentAddUrl.Contains(" "))
                {
                    currentAddUrl = currentAddUrl.Replace(" ", "_");
                }
                string url = baseUrl + currentAddUrl;

                HtmlWeb web = new HtmlWeb();
                var detailedDoc = web.Load(url);

                string desc = detailedDoc.DocumentNode
                    .SelectSingleNode("//p")
                    .InnerText;

                try
                {
                    var imgUrlList = detailedDoc.DocumentNode
                        .SelectNodes("//img")
                        .Where(node=>node.GetAttributeValue("alt","")
                        .Equals(title)).ToList();
                    var imgUrl = imgUrlList[0].Attributes["src"].Value;
                    SaveImage(imgUrl, i);
                    string imgFullPath = Path.GetFullPath(@"result/img" + i + ".png");

                    AddRecordSecond(title, desc, imgFullPath);
                }
                catch(ArgumentOutOfRangeException e)
                {
                    AddRecordSecond(title, desc, "N/A");
                    Console.WriteLine("This item does not have any images.", e); 
                }
    
                Console.WriteLine(url);
                Console.WriteLine(title);
                Console.WriteLine(desc);
            }

            Console.WriteLine("All done. Please see the result folder.");
        }

        private static void AddRecord(string title, string desc, string imgPath)
        {
            try
            {
                using(StreamWriter file = new StreamWriter(@"result/result.csv", true))
                {
                    file.WriteLine(title + "," + desc + "," + imgPath);
                    file.Flush();
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("This program did an oopsie:", e);
            }
        }

        private static void AddRecordSecond(string title,string desc,string imgPath)
        {
            using(StreamWriter sw = File.AppendText(@"result/result.csv"))
            {
                sw.WriteLine(title+","+desc+","+imgPath);
                sw.Flush();
            }     
        }

        private static void SaveImage(string imgUrl,int i)
        {
            using(WebClient client=new WebClient())
            {
                client.DownloadFile(new Uri(imgUrl), "result\\img"+i+".png");
            }
        }

        private static void CreateFolder()
        {
            string path = @"result";
            try
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine("That path exists already.");
                    return;
                }
                DirectoryInfo di = Directory.CreateDirectory(path);
                Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed:{0}", e.ToString());
            }
            finally { }
        }
    }
}
