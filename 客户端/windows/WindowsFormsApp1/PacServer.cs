namespace WindowsFormsApp1
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    internal class PacServer
    {
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private string ipString = "0.0.0.0";
        private int port = 0x16fd;
        private Label UserNumLabel;
        private int UserNumCount = 0;
        private int UserNumRight = 0;
        private int UserNumError = 0;
        private string PacData = "\r\nproxy =  \"PROXY 127.0.0.1:5886\"\r\nfunction FindProxyForURL(url, host) {\r\nif(url.indexOf(\"fanyi\")>0)\r\n{\r\n    return proxy\r\n}\r\n    return \"DIRECT\"\r\n}\r\n";

        private void PacConnect(object obj)
        {
            Socket socket = (Socket) obj;
            while (true)
            {
                try
                {
                    Socket parameter = socket.Accept();
                    this.UserNumCount++;
                    this.ShowPacLabel();
                    new Thread(new ParameterizedThreadStart(this.receiveData)).Start(parameter);
                }
                catch (Exception)
                {
                }
            }
        }

        private string ReadPacFile()
        {
            StreamReader reader = new StreamReader("pac.txt", Encoding.Default);
            return reader.ReadToEnd();
        }

        private void receiveData(object obj)
        {
            Socket socket = (Socket) obj;
            socket.SendBufferSize = 0x100000;
            this.PacData = this.ReadPacFile();
            socket.Send(Encoding.Default.GetBytes(this.PacHttpData));
            Thread.Sleep(0x3e8);
            socket.Close();
            this.UserNumRight++;
            this.ShowPacLabel();
        }

        private void ShowPacLabel()
        {
            object[] objArray1 = new object[] { this.UserNumRight, "/", this.UserNumError, "/", this.UserNumCount };
            string str = string.Concat(objArray1);
            if (this.UserNumLabel.InvokeRequired)
            {
                SetTextCallBack method = new SetTextCallBack(this.ShowPacLabel);
                this.UserNumLabel.Invoke(method);
            }
            else
            {
                this.UserNumLabel.Text = str;
            }
        }

        public void StartServer(Label label1, Label label3)
        {
            try
            {
                IPAddress address = IPAddress.Parse(this.ipString);
                this.socket.Bind(new IPEndPoint(address, this.port));
                this.socket.Listen(0x2710);
                new Thread(new ParameterizedThreadStart(this.PacConnect)).Start(this.socket);
                label1.Text = "PAC服务器启动完成";
                label1.ForeColor = Color.Chartreuse;
                this.UserNumLabel = label3;
            }
            catch (Exception)
            {
                label1.Text = "PAC服务器启动失败";
                label1.ForeColor = Color.DarkRed;
            }
        }

        private string PacHttpData
        {
            get
            {
                object[] objArray1 = new object[] { "HTTP/1.0 200 OK\r\nContent-Disposition:attachment;filename=pac.js\r\nContent-Type:application/octet-stream;charset=UTF-8\r\nContent-Length:", this.PacData.Length, "\r\n\r\n", this.PacData };
                return string.Concat(objArray1);
            }
        }

        private delegate void SetTextCallBack();
    }
}

