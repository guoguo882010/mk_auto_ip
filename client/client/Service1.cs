using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Web;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace client
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread _thread = new Thread(go);
            _thread.Name = "My Worker Thread";
            _thread.IsBackground = true;
            _thread.Start();
        }

        protected override void OnStop()
        {
        }

        private static void go()
        {
            while (true)
            {
                StartClient();
                Thread.Sleep(5000);
            }
        }

        public static void StartClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = IPAddress.Parse("111.111.111.111"); // ip adress
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11111);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    string _ip = GetIPAddress();

                    if (_ip != "" && CheckIPValid(_ip))
                    {
                        sender.Connect(remoteEP);

                        // Encode the data string into a byte array.  
                        byte[] msg = Encoding.ASCII.GetBytes(_ip+"<EOF>");
                        // Send the data through the socket.  
                        int bytesSent = sender.Send(msg);
                        // Release the socket.  
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                    }

                }
                catch (ArgumentNullException ane)
                {
                    //Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                   // Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
            }
        }

        private static string GetIPAddress()
        {
            // 获取本机ip地址
            String address = "";
            WebRequest request = WebRequest.Create("http://ip.cn"); // get ip adress example http://ip.cn
            try
            {
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                address = address.Trim();

                return address;
            }
            catch
            {
                return "";
            }

        }

        private static Boolean CheckIPValid(String strIP)
        {
            // Split string by ".", check that array length is 3
            char chrFullStop = '.';
            string[] arrOctets = strIP.Split(chrFullStop);
            if (arrOctets.Length != 4)
            {
                return false;
            }
            // Check each substring checking that the int value is less than 255 
            // and that is char[] length is !>     2
            Int16 MAXVALUE = 255;
            Int32 temp; // Parse returns Int32
            foreach (String strOctet in arrOctets)
            {
                if (strOctet.Length > 3)
                {
                    return false;
                }

                temp = int.Parse(strOctet);
                if (temp > MAXVALUE)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
