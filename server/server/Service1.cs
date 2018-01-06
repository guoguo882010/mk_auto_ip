using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Web.Administration;
using System.Threading;
using System.IO;

namespace server
{
    public partial class Service1 : ServiceBase
    {
        public static string data = null;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread _thread = new Thread(StartListening);
            _thread.Name = "My Worker Thread";
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected override void OnStop()
        {
        }

        public static void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPAddress ipAddress = IPAddress.Parse("111.111.111.111");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11111);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    // Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            string ip = data.Replace("<EOF>", "");

                            if (!findBlockIp(ip.Trim(), "default site"))
                            {
                                addBlockIp(ip.Trim(), "default site");
                            }

                            

                            break;
                        }
                    }

                    // Show the data on the console.  
                    // Console.WriteLine("Text received : {0}", data);

                    // Echo the data back to the client.  
                    // byte[] msg = Encoding.ASCII.GetBytes(data);

                    // handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                // Console.WriteLine(e.ToString());
            }

        }

        private static bool findBlockIp(String ip,String site)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection ipSecuritySection = config.GetSection("system.webServer/security/ipSecurity", site);
                ConfigurationElementCollection ipSecurityCollection = ipSecuritySection.GetCollection();

                foreach (var item1 in ipSecurityCollection)
                {
                    if ((String)item1["ipAddress"] == ip)
                    {
                        return true;
                    }
                }

                return false;

            }
        }

        private static void addBlockIp(String ip,String site)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection ipSecuritySection = config.GetSection("system.webServer/security/ipSecurity", site);
                ConfigurationElementCollection ipSecurityCollection = ipSecuritySection.GetCollection();

                ConfigurationElement addElement = ipSecurityCollection.CreateElement("add");
                addElement["ipAddress"] = ip;
                addElement["allowed"] = true;
                ipSecurityCollection.Add(addElement);

                serverManager.CommitChanges();
            }
        }

        private static void WriteToFile(string text)
        {
            string path = "D:\\ServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }
    }
}
