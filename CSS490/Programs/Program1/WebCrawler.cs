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
//===================================================================================================
	//
//===================================================================================================
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
			hostname = prepareHost(hostname);
			IPHostEntry host = getIP(getHostName(hostname));
			string htmlPage = getHTML(hostname,host, numHops);
            
			Console.WriteLine(htmlPage);
            //Hit any key to exit
			Console.WriteLine("Press any key to exit");
            Console.ReadKey();
			
        }

//===================================================================================================
	//
//===================================================================================================
		static Socket getSock(IPHostEntry host){
            Socket sock = null;
			int port = 80;
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
			return sock;
		}
//===================================================================================================
	//
//===================================================================================================
		static string prepareHost(string host){
			//Console.WriteLine("Test: " + host);
			if(host.Contains("://")){
				host = host.Substring(host.IndexOf("://")+ 3);
				//Console.WriteLine(host);
			}

			if(!host.Contains("www.") && !host.Contains("WWW.")){
				host = "www." + host;
			}
			//Console.WriteLine("Test After: " + host);
			return host;
		}

//===================================================================================================
	//
//===================================================================================================
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

//===================================================================================================
	//
//===================================================================================================
		static string getHTML(string hostname, IPHostEntry host, int numHops){
			
			string resp = makeHTTPRequest(hostname,host);
			if(numHops == 0){
				return parseHTML(resp);
			}
			MatchCollection matches = getLinks(resp);
			if(matches.Count == 0){
				return parseHTML(resp);
			}
			foreach(Match match in matches){
				GroupCollection groups = match.Groups;
				//Console.WriteLine(groups[1].ToString());
				string hostN = prepareHost(groups[1].ToString());
				//Console.WriteLine(hostN);
				host = getIP(getHostName(hostN));
				if(host != null){
					return getHTML(hostN, host, numHops-1);
				}
			}
			return parseHTML(resp);
		}

//===================================================================================================
	//
//===================================================================================================		
		static string parseHTML(string resp){
			if(resp.Contains("\r\n\r\n")){
				return resp.Substring(resp.IndexOf("\r\n\r\n"));
			}
			else{
				return "The HTML could not be found.";
			}
		}

//===================================================================================================
	//
//===================================================================================================
		static string makeHTTPRequest(string hostname, IPHostEntry hostEntry){
			string path = getPath(hostname);
			Socket sock = getSock(hostEntry);

			//Console.WriteLine(hostEntry.HostName + path);

			string request = "GET " + path + " HTTP/1.1\r\nHost: " + hostEntry.HostName + 
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

//===================================================================================================
	//
//===================================================================================================
		static MatchCollection getLinks(string resp){
			//string regExp = "href=[\'"]?([^\'" >]+)";
			Regex rx = new Regex("href=\"([^\"]*)",RegexOptions.IgnoreCase);
			return rx.Matches(resp);
		}	

//===================================================================================================
	//
//===================================================================================================
		static IPHostEntry getIP(string hostname){
			IPHostEntry host = null;
			try{
				host = Dns.GetHostEntry(hostname);
			}
			catch(SocketException e){
				Console.WriteLine("Invalid Hostname");
				Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
			}

			return host;
		}

//===================================================================================================
	//
//===================================================================================================
		static string getHostName(string host){
			if(host.Contains('/')){
				host = host.Substring(0,host.IndexOf('/'));
			}
			return host;
		}
    }
}
