using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    class PacServer
    {
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

         //string ipString = "127.0.0.1";   // 服务器端ip
         string ipString = "0.0.0.0";   // 服务器端ip
         int port = 5885;                // 服务器端口
         
        Label UserNumLabel;

        int UserNumCount = 0;
        int UserNumRight = 0;
        int UserNumError = 0;

        string PacData =
@"
proxy =  ""PROXY 127.0.0.1:5886""
function FindProxyForURL(url, host) {
if(url.indexOf(""fanyi"")>0)
{
    return proxy
}
    return ""DIRECT""
}
";

        string PacHttpData
        {
            get
            {
                return @"HTTP/1.0 200 OK
Content-Disposition:attachment;filename=pac.js
Content-Type:application/octet-stream;charset=UTF-8
Content-Length:" + PacData.Length + @"

" + PacData; 
            }
        }


        public void StartServer(Label label1, Label label3)
        {
            try
            {
                IPAddress address = IPAddress.Parse(ipString);
                socket.Bind(new IPEndPoint(address, port));
                socket.Listen(10000);
                new Thread(PacConnect).Start(socket);  // 在新的线程中监听客户端连接
                label1.Text = "PAC服务器启动完成";
                label1.ForeColor = Color.Chartreuse;
                UserNumLabel = label3;
            }
            catch(Exception ex)
            {
                label1.Text = "PAC服务器启动失败";
                label1.ForeColor = Color.DarkRed;
            }
        }

        private void PacConnect(object obj)
        {
            Socket socket = (Socket)obj;
            while (true)
            {
                try
                {
                    Socket clientScoket = socket.Accept();
                    UserNumCount++;
                    ShowPacLabel();
                    new Thread(receiveData).Start(clientScoket);   // 在新的线程中接收客户端信息  
                }
                catch(Exception ex)
                {

                }
                 
            }
               
        }

        delegate void SetTextCallBack();
   
        private void ShowPacLabel()
        {
           string text =UserNumRight + "/" + UserNumError + "/" + UserNumCount;
            if (this.UserNumLabel.InvokeRequired)
            {
                SetTextCallBack stcb = new SetTextCallBack(ShowPacLabel);
                this.UserNumLabel.Invoke(stcb);
            }
            else
            {
                this.UserNumLabel.Text = text;
            }
        }

        private void receiveData(object obj)
        {

            Socket socket = (Socket)obj;
            socket.SendBufferSize = 1024 * 1024;
            PacData = ReadPacFile();
            socket.Send(Encoding.Default.GetBytes(PacHttpData));
            Thread.Sleep(1000);
            socket.Close();
            UserNumRight++;
            ShowPacLabel();
        }

        private string ReadPacFile()
        {
            StreamReader sr = new StreamReader("pac.txt", Encoding.Default);
            String data = sr.ReadToEnd();
            return data;  
        }
    }
}
