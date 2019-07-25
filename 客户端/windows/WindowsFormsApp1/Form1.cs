using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using WebSocketSharp;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        PacServer PacServer = new PacServer();
        ProxyServer ProxyServer = new ProxyServer();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PacServer.StartServer(label1, label3);
            ProxyServer.StartServer(label4, label5, label6);

            SetProxy.Set("http://127.0.0.1:5885");
        }
        private WebSocket webSocketClient = null;
        private System.Timers.Timer t = new System.Timers.Timer(2000); //
        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                webSocketClient = new WebSocket("wss://api.hitbtc.com/api/2/ws");
                //webSocketClient = new WebSocket("ws://121.40.165.18:8800");
                webSocketClient.SetProxy("http://127.0.0.1:5886", "", "");
                webSocketClient.OnError += new EventHandler<ErrorEventArgs>(webSocketClient_Error);
                webSocketClient.OnOpen += new EventHandler(webSocketClient_Opened);
                webSocketClient.OnClose += new EventHandler<CloseEventArgs>(webSocketClient_Closed);
                webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(webSocketClient_MessageReceived);
                webSocketClient.ConnectAsync();
                while (!webSocketClient.IsAlive)
                {
                    Console.WriteLine("Waiting WebSocket connnet......");
                    Thread.Sleep(1000);
                }
                t.Elapsed += new ElapsedEventHandler(heatBeat);
                t.Start();
            }); 
        }

        private void heatBeat(object sender, ElapsedEventArgs e)
        {
            webSocketClient.Send("ping");
        }

        private void webSocketClient_MessageReceived(object sender, MessageEventArgs e)
        {
             
        }

        private void webSocketClient_Closed(object sender, CloseEventArgs e)
        {
             
        }

        private void webSocketClient_Opened(object sender, EventArgs e)
        {
             
        }

        private void webSocketClient_Error(object sender, ErrorEventArgs e)
        {
             
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            PacServer.socket.Close();
            PacServer.socket.Dispose();
            ProxyServer.socket.Close();
            ProxyServer.socket.Dispose();
            System.Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Method methodTask = Method.POST;
                RestRequest requestTask = new RestRequest(methodTask);

                //Uri uriTask = new Uri("https://www.163.com/");
                //Uri uriTask = new Uri("http://btcjqr.cn");
                //Uri uriTask = new Uri("https://www.btcjqr.cn");
                Uri uriTask = new Uri("https://www.google.com");

                //wss://api.hitbtc.com/api/2/ws

                RestClient clientTask = new RestClient();

                clientTask.Proxy = new WebProxy("127.0.0.1:5886");

                clientTask.BaseUrl = uriTask;
                IRestResponse responseTask = clientTask.Execute(requestTask);
                string dataTask = responseTask.Content;

                MessageBox.Show(dataTask);
            });
        }
    }
}
