using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace WebScraper
{
    class Program
    {

        static private Dictionary<string, bool> visitedSites;
        static void Main(string[] args)
        {
            visitedSites = new Dictionary<string, bool>();

            if (args.Length < 2)
            {
                Console.WriteLine("Too few arguments given");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }

            Console.WriteLine(getHTML(args[0], Int32.Parse(args[1])));

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static string getHTML(string hostname, int hops)
        {
            string result = "";
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                try
                {
                    client.BaseAddress = new Uri(hostname);
                    HttpResponseMessage resp = client.GetAsync("/").Result;
                    //resp.EnsureSuccessStatusCode();
                    result = resp.Content.ReadAsStringAsync().Result;
                    //Console.WriteLine("Result: " + result);
                    //Console.WriteLine("Status code: " + resp.StatusCode);
                }
                catch (UriFormatException e)
                {
                    Console.WriteLine("The URL was in an unknown format");
                    return result;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Could not locate that URL");
                    return result;
                }
                Console.WriteLine("URL: " + hostname);
                if(hops == 0)
                {
                    return result;
                }
                else
                {
                    MatchCollection matches = new Regex("href=\"([^\"]*)", RegexOptions.IgnoreCase).Matches(result);
                    if(matches.Count == 0)
                    {
                        return result;
                    }
                    foreach(Match match in matches)
                    {
                        GroupCollection groups = match.Groups;
                        if (!visitedSites.ContainsKey(groups[1].ToString()))
                        {
                            visitedSites[groups[1].ToString()] = true;
                            return getHTML(groups[1].ToString(), hops - 1);
                        }
                    }
                }
            }
            return result;
        }
    }
}
