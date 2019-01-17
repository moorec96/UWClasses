/**
 * Class: WebScraper
 * Purpose: Take in a url and a number, and return the html at the nth hop
 * Author: Caleb Moore
 * Date: 1/17/19
 * */

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
    class WebScraper
    {

        static private Dictionary<string, bool> visitedSites;   //Keeps track of sites that have been visited
        static private int numHops;                             //Keeps track of number of successful hops
        static void Main(string[] args)
        {
            visitedSites = new Dictionary<string, bool>();
            numHops = 0;
            if (args.Length < 2)    //If not enough arguments, end the application
            {
                Console.WriteLine("Too few arguments given");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }

            string result = getHTML(args[0], Int32.Parse(args[1]), "").Item1 + "\n";       
            Console.WriteLine(result);
            Console.WriteLine("Number of successful hops: " + numHops );

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        /**
         * Method: getHTML
         * Purpose: Recursively send http requests for html, find first link in html, and then call function again with that url
         * Parameters: 
         *      hostname - url to make http request with
         *      hops - # of urls to jump to
         *      lastSiteReached - last html page that was reached
         * Returns: html string
         * */
        static Tuple<string,bool> getHTML(string hostname, int hops, string lastSiteReached)
        {
            string result = "";
            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                try
                {
                    HttpResponseMessage resp;
                    if (!isAbsoluteURL(hostname))       //If url is not an absolute url, get the BaseAdress 
                    {
                        client.BaseAddress = new Uri(hostname);
                        resp = client.GetAsync("/").Result;
                    }
                    else                               //Else, make request with absolute url
                    {
                        resp = client.GetAsync(hostname).Result;
                    }
                    result = resp.Content.ReadAsStringAsync().Result;      //set result to html returned from http response
                }
                catch (UriFormatException e)                                //Catch any bad url exceptions
                {
                    Console.WriteLine("The URL was in an unknown format");
                    return new Tuple<string, bool>(lastSiteReached,false);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("Could not locate that URL");
                    return new Tuple<string, bool>(lastSiteReached, false);
                }
                catch(ArgumentException e)
                {
                    Console.WriteLine("URL did not contain http or https");
                    return new Tuple<string, bool>(lastSiteReached, false);
                }
                catch(AggregateException e)
                {
                    Console.WriteLine("The URL was in an unknown format");
                    return new Tuple<string, bool>(lastSiteReached, false);
                }

                Console.WriteLine("URL: " + hostname);
                if(hops == 0)           //Base case of recursive function -> If hops == 0, then return the current html page
                {
                    return new Tuple<string, bool>(result, true);
                }
                else
                {
                    MatchCollection matches = new Regex("(?<=a href=\")http.+?(?=\")", RegexOptions.IgnoreCase).Matches(result);    //Find all href links in html
                    if (matches.Count == 0)     //if there were no matches, return the current html page
                    {
                        return new Tuple<string, bool>(result, true); ;
                    }
                    foreach(Match match in matches)                 //Find first unique href link in matches, and call this function again with it 
                    {
                        GroupCollection groups = match.Groups;
                        string nextURL = groups[0].ToString();
                        if(nextURL[nextURL.Length - 1] != '/')      //Ensures that urls are in the same format in the Dictionary
                        {
                            nextURL += '/';
                        }
                        if (!visitedSites.ContainsKey(nextURL))     //If a link has not been visited yet, then call the function again with that link
                        {
                            visitedSites[nextURL] = true;
                            
                            Tuple<string, bool> res = getHTML(groups[0].ToString(), hops - 1, result);
                            if (res.Item2)      //If the result from previous function call is true, then return html
                            {
                                numHops++;
                                return res;
                            }
                            
                        }
                    }
                }
            }
            return new Tuple<string, bool>(result, true);
        }

        //Returns true if the given url is an absolute address 
        static bool isAbsoluteURL(string url) {
            Uri res;
            return Uri.TryCreate(url, UriKind.Absolute, out res);
        }
    }
}
