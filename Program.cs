using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.IO;

namespace GetXmlString
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please input your origin file path. e.g, E:\\Script-Answer.txt");
            var inputFilePath = Console.ReadLine();
            Console.WriteLine("Your origin file path is " + inputFilePath);
            var queryFilePath = inputFilePath;// @"E:\Script-Answer.txt";

            FileInfo info = new FileInfo(queryFilePath);
            if (!File.Exists(queryFilePath))
            {
                Console.WriteLine("File doesn't exist in current path, pls check...");
                return;
            }
            var fileLength = info.Length;
            if (fileLength > 1024 * 1024 * 4)
            {
                //file size more than 4M
                Console.WriteLine("File size limit 4M, pls check...");
                return;
            }

            var queryWords = File.ReadAllLines(queryFilePath, Encoding.UTF8);
            var inputCount = queryWords.Count();
            Console.WriteLine("Input file total count:{0}", inputCount);
            Console.WriteLine();
            Console.WriteLine("Please input your result save path. e.g, E:\\SpeakResult.txt");
            var outputFilePath = Console.ReadLine();// @"E:\SpeakString.txt";                        
            int index = 1, succeedCount = 0;
            foreach (var word in queryWords)
            {
                index++;
                if (!string.IsNullOrWhiteSpace(word))
                {
                    Console.WriteLine(index + ". " + word + " is being processed...");
                    succeedCount += GetSpeakStringByQueryWord(word, outputFilePath);
                }
            }
            Console.WriteLine("Output file total count:{0}, failed count:{1}---{2}", succeedCount, inputCount - succeedCount, "Press any key to exist...");
            Console.ReadKey();
        }

        static int GetSpeakStringByQueryWord(string queryWord, string outputFilePath)
        {
            try
            {
                //target url
                var tgtUrl = "http://mycortana/?q=" + queryWord + "&format=xml";

                var wc = new WebClient();
                //Set default credential, avoid remote unauthorized error
                wc.Credentials = CredentialCache.DefaultCredentials;
                wc.UseDefaultCredentials = true;

                //Download target Xml doc
                var tgtXmlString = wc.DownloadString(tgtUrl);
                XmlDocument requestDocXml = new XmlDocument();
                requestDocXml.LoadXml(tgtXmlString);
                XmlElement rootElement = requestDocXml.DocumentElement;
                //Get all tags in doc named as 'ssml'(speak string in this tag)
                var tgtNodes = rootElement.GetElementsByTagName("ssml");

                var tgtString = tgtNodes[0].InnerText;
                if (!File.Exists(outputFilePath))
                {
                    using (FileStream fs = File.Create(outputFilePath))
                    {
                        Byte[] info = new UTF8Encoding(true).GetBytes(tgtString);
                        fs.Write(info, 0, info.Length);
                    }
                }
                else
                {
                    File.AppendAllText(outputFilePath, queryWord + "\t" + tgtString + "\r\n");
                }

                return 1;
            }
            catch (Exception e)
            {
                //TODO: LOG
                Console.WriteLine(queryWord + " occurs an error...");
                //throw new Exception(e.ToString());
                return 0;
            }
        }
    }
}
