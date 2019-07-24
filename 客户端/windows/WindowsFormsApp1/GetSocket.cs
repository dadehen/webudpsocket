namespace WindowsFormsApp1
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    internal class GetSocket
    {
        public Socket socket;
        public byte[] data = new byte[0];
        public string user;
        public int id;
        public bool created = false;

        public GetSocket(Socket socket, string user, int id)
        {
            this.socket = socket;
            this.user = user;
            this.id = id;
            socket.ReceiveBufferSize = 0x100000;
            socket.SendBufferSize = 0x100000;
        }

        internal void AcceptContent()
        {
            while (!this.ContentOver())
            {
                this.GetSocketData();
            }
        }

        private bool ContentOver()
        {
            int contentLength = this.ContentLength;
            int heardLength = this.HeardLength;
            return (this.data.Length == (contentLength + heardLength));
        }

        internal void GetHeard()
        {
            while (!this.HeardOver())
            {
                this.GetSocketData();
            }
        }

        private void GetSocketData()
        {
            int available = this.socket.Available;
            if (available > 0)
            {
                byte[] array = new byte[available + this.data.Length];
                this.data.CopyTo(array, 0);
                this.socket.Receive(array, this.data.Length, available, SocketFlags.None);
                this.data = new byte[array.Length];
                array.CopyTo(this.data, 0);
            }
            else
            {
                Thread.Sleep(100);
            }
        }

        private bool HeardOver()
        {
            int num = 0;
            for (int i = 0; i < this.data.Length; i++)
            {
                if ((((num % 2) == 0) && (this.data[i] == 13)) || (((num % 2) == 1) && (this.data[i] == 10)))
                {
                    num++;
                }
                else
                {
                    num = 0;
                }
                if (num == 4)
                {
                    return true;
                }
            }
            return false;
        }

        public int HeardLength
        {
            get
            {
                int num = 0;
                for (int i = 0; i < this.data.Length; i++)
                {
                    if ((((num % 2) == 0) && (this.data[i] == 13)) || (((num % 2) == 1) && (this.data[i] == 10)))
                    {
                        num++;
                    }
                    else
                    {
                        num = 0;
                    }
                    if (num == 4)
                    {
                        return (i + 1);
                    }
                }
                return 0;
            }
        }

        public byte[] HeardData
        {
            get
            {
                int heardLength = this.HeardLength;
                byte[] destinationArray = new byte[heardLength];
                Array.Copy(this.data, destinationArray, heardLength);
                return destinationArray;
            }
        }

        public string Method
        {
            get
            {
                string str = Encoding.UTF8.GetString(this.data).Split("\r\n".ToCharArray())[0];
                char[] separator = new char[] { ' ' };
                return str.Split(separator)[0].ToUpper();
            }
        }

        public string Host
        {
            get
            {
                string str = Encoding.UTF8.GetString(this.data).Split("\r\n".ToCharArray())[0];
                char[] separator = new char[] { ' ' };
                return str.Split(separator)[1];
            }
        }

        public string Url
        {
            get
            {
                string host = this.Host;
                if (host.Contains("http://") || host.Contains("https://"))
                {
                    host = host.TrimStart("https://".ToCharArray()).TrimStart("http://".ToCharArray());
                }
                char[] separator = new char[] { ':' };
                if (host.Split(separator).Length > 1)
                {
                    char[] chArray2 = new char[] { ':' };
                    char[] chArray3 = new char[] { '/' };
                    return host.Split(chArray2)[0].Split(chArray3)[0];
                }
                char[] chArray4 = new char[] { '/' };
                return host.Split(chArray4)[0];
            }
        }

        public int Port
        {
            get
            {
                string host = this.Host;
                bool flag = false;
                bool flag2 = false;
                if (host.Contains("http://"))
                {
                    flag = true;
                    host = host.TrimStart("http://".ToCharArray());
                }
                else if (host.Contains("https://"))
                {
                    flag2 = true;
                    host = host.TrimStart("https://".ToCharArray());
                }
                char[] separator = new char[] { ':' };
                if (host.Split(separator).Length > 1)
                {
                    char[] chArray2 = new char[] { ':' };
                    return int.Parse(host.Split(chArray2)[1]);
                }
                if (!flag && flag2)
                {
                    return 0x1bb;
                }
                return 80;
            }
        }

        public int ContentLength
        {
            get
            {
                string[] strArray = Encoding.UTF8.GetString(this.HeardData).Split("\r\n".ToCharArray());
                for (int i = 1; i < strArray.Length; i++)
                {
                    char[] separator = new char[] { ':' };
                    if (strArray[i].Split(separator)[0] == "Content-Length")
                    {
                        char[] chArray2 = new char[] { ':' };
                        return int.Parse(strArray[i].Split(chArray2)[1]);
                    }
                }
                return 0;
            }
        }
    }
}

