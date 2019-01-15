using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace WebCrawler
{
    class WebCrawler
    {

        //private Dictionary<String, bool> visitedSites;

        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Not enough arguments were given.");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }

            
            string hostname = args[0];
            int numHops = Int32.Parse(args[1]);
			string htmlPage = getHTML(hostname, numHops);
            
            //Hit any key to exit
			Console.WriteLine("Press any key to exit");
            Console.ReadKey();
			
        }

		static Tuple<Socket,IPHostEntry> getSock(string hostname){
            Socket sock = null;
			IPHostEntry host = null;
			int port = 80;
			try{
				host = Dns.GetHostEntry(hostname);
			}
			catch(SocketException e){
				Console.WriteLine("Invalid Host");
				Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
			}
            if(host == null)
            {
                Console.Write("Invalid Host");
				Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }

            foreach(IPAddress addr in host.AddressList)
            {
                IPEndPoint ipe = new IPEndPoint(addr, port);
                Socket tempSock = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                tempSock.Connect(ipe);
                if (tempSock.Connected)
                {
                    sock = tempSock;
                    break;
                }
            }
            if(sock == null)
            {
                Console.WriteLine("Unable to connect socket");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
			Tuple<Socket,IPHostEntry> retVal = new Tuple<Socket,IPHostEntry>(sock,host);
			return retVal;
		}

		static string prepareHost(string host){
			if(host.Contains("http://") || host.Contains("HTTP://")){
				host = host.Substring(7);
				Console.WriteLine(host);
			}
			else if(host.Contains("https://") || host.Contains("HTTPs://")){
				host = host.Substring(8);
			}

			if(!host.Contains("www.") || !host.Contains("WWW.")){
				host = "www." + host;
			}
			return host;
		}

		static string getPath(string host){
			string path = "";
			if(host.Contains('/')){
				path = host.Substring(host.IndexOf('/'));
			}
			else{
				path = "/";
			}
			return path;
		}

		static string getHTML(string hostname, int numHops){
			if(numHops == 0){
				return parseHTML(hostname);
			}

			string resp = makeHTTPRequest(hostname);

			MatchCollection matches = getLinks(resp);
			if(matches.Count == 0){
				return parseHTML(resp);
			}
			foreach(Match match in matches){
				GroupCollection groups = match.Groups;
				
				Console.WriteLine(groups[0]);
			}
			return "";
		}
		
		static string parseHTML(string hostname){
			return parseHTML(makeHTTPRequest(hostname));
		}

		static string makeHTTPRequest(string hostname){
			hostname = prepareHost(hostname);
			string path = getPath(hostname);
			Tuple<Socket,IPHostEntry> hostInfo = getSock(hostname);
			Socket sock = hostInfo.Item1;
			string host = hostInfo.Item2.HostName;

			string request = "GET " + path + " HTTP/1.1\r\nHost: " + host + 
				"\r\nConnection: keep-alive\r\nAccept: text/html\r\n\r\n";
            byte[] dataToSend = Encoding.ASCII.GetBytes(request);
            sock.Send(dataToSend);

            byte[] buff = new byte[1024];

            int bytes = 0;
            string resp = "";

            do
            {
               	bytes = sock.Receive(buff);
               	resp = resp + Encoding.ASCII.GetString(buff, 0, bytes);
				Thread.Sleep(5);
            } while (sock.Available > 0);

			return resp;
		}

		static MatchCollection getLinks(string resp){
			//string regExp = "href=[\'"]?([^\'" >]+)";
			Regex rx = new Regex("href=\"([^\"]*)",RegexOptions.IgnoreCase);
			return rx.Matches(resp);
		}	
    }
}
