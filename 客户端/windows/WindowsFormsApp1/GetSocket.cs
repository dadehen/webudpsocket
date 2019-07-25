using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{ 
    class GetSocket
    {
        public Socket socket;
        public byte[] data = new byte[] { };
        public string user;
        public int id;

        public bool created = false;

        public GetSocket(Socket socket, string user, int id)
        {
            this.socket = socket;
            this.user = user;
            this.id = id;
            socket.ReceiveBufferSize = 1024 * 1024;
            socket.SendBufferSize = 1024 * 1024;
        }

        public int HeardLength
        {
            get
            {
                int num = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    if ((num % 2 == 0 && data[i] == '\r') || (num % 2 == 1 && data[i] == '\n'))
                    {
                        num++;
                    }
                    else
                    {
                        num = 0;
                    }

                    if (num == 4)
                    {
                        return i+1;
                    }
                }
                return 0;
            }
        }
        public byte[] HeardData {
            get
            {
                int length = HeardLength;
                byte[] hearddata = new byte[length];
                Array.Copy(data, hearddata, length);
                return hearddata;
            }
        }
         
        public string Method {
            get
            {
                string str = Encoding.UTF8.GetString(data);
                str = str.Split("\r\n".ToCharArray())[0];
                str = str.Split(' ')[0].ToUpper();
                return str;
            }
        }

        public string Host
        {
            get
            {
                string str = Encoding.UTF8.GetString(data);
                str = str.Split("\r\n".ToCharArray())[0];
                str = str.Split(' ')[1];
                return str;
            }
        }

        public string Url {
            get
            {
                //string host = Host.TrimStart("https://".ToCharArray()).TrimStart("http://".ToCharArray());
                string host = Host;
                if (host.Contains("http://") || host.Contains("https://"))
                {
                    host = host.TrimStart("https://".ToCharArray()).TrimStart("http://".ToCharArray());
                }
                if (host.Split(':').Length > 1)
                {
                    return host.Split(':')[0].Split('/')[0];
                }
                return host.Split('/')[0];
            }
        }
        public int Port {
            get
            {
                string host = Host;
                bool ishttp = false;
                bool ishttps = false;
                if (host.Contains("http://"))
                {
                    ishttp = true;
                    host = host.TrimStart("http://".ToCharArray());
                }
                else if (host.Contains("https://"))
                {
                    ishttps = true;
                    host = host.TrimStart("https://".ToCharArray());
                }

                if (host.Split(':').Length > 1)
                {
                    return int.Parse(host.Split(':')[1]);
                }
                else
                {
                    if (ishttp)
                    {
                        return 80;
                    }
                    else if (ishttps)
                    {
                        return 443;
                    }
                }
                return 80;
            }
        }
        public int ContentLength {
            get
            {
                string strHeard = Encoding.UTF8.GetString(HeardData);
                string[] arrHeard = strHeard.Split("\r\n".ToCharArray());

                for (int i = 1; i < arrHeard.Length; i++)
                {
                    if (arrHeard[i].Split(':')[0] == "Content-Length")
                    {
                        return int.Parse(arrHeard[i].Split(':')[1]);
                    }
                }

                return 0;
            }
        }
        internal void GetHeard()
        {
            while (!HeardOver())
            {
                GetSocketData();
            }
        }

        private void GetSocketData()
        {
            int length = socket.Available;
            if (length > 0)
            {
                byte[] acceptdata = new byte[length + data.Length];
                data.CopyTo(acceptdata, 0);
                socket.Receive(acceptdata, data.Length, length, SocketFlags.None);
                data = new byte[acceptdata.Length];
                acceptdata.CopyTo(data, 0);
            }
            else
            {
                Thread.Sleep(100);
            }
        }
         
        private bool HeardOver()
        {
            int num = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if((num%2==0&&data[i]=='\r')|| (num % 2 == 1 && data[i] == '\n'))
                {
                    num++;
                }
                else
                {
                    num = 0;
                }

                if(num==4)
                {
                    return true;
                }
            }
            return false;
        }

        internal void AcceptContent()
        {
            while (!ContentOver())
            {
                GetSocketData();
            }
        }

        private bool ContentOver()
        {
            int contetnt = ContentLength;
            int heard = HeardLength; 

            if(data.Length==contetnt+heard)
            {
                return true;
            }
            return false;
        }
         
    }
}
