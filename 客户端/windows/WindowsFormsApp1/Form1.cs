namespace WindowsFormsApp1
{
    using RestSharp;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Windows.Forms;
    using WebSocketSharp;

    public class Form1 : Form
    {
        private WindowsFormsApp1.PacServer PacServer = new WindowsFormsApp1.PacServer();
        private WindowsFormsApp1.ProxyServer ProxyServer = new WindowsFormsApp1.ProxyServer();
        private WebSocket webSocketClient = null;
        private System.Timers.Timer t = new System.Timers.Timer(2000.0);
        private IContainer components = null;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Button button1;
        private Button button2;

        public Form1()
        {
            this.InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(delegate {
                this.webSocketClient = new WebSocket("wss://api.hitbtc.com/api/2/ws", new string[0]);
                this.webSocketClient.SetProxy("http://127.0.0.1:5886", "", "");
                this.webSocketClient.OnError += new EventHandler<ErrorEventArgs>(this.webSocketClient_Error);
                this.webSocketClient.OnOpen += new EventHandler(this.webSocketClient_Opened);
                this.webSocketClient.OnClose += new EventHandler<CloseEventArgs>(this.webSocketClient_Closed);
                this.webSocketClient.OnMessage += new EventHandler<MessageEventArgs>(this.webSocketClient_MessageReceived);
                this.webSocketClient.ConnectAsync();
                while (!this.webSocketClient.IsAlive)
                {
                    Console.WriteLine("Waiting WebSocket connnet......");
                    Thread.Sleep(0x3e8);
                }
                this.t.Elapsed += new ElapsedEventHandler(this.heatBeat);
                this.t.Start();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task.Run(delegate {
                Method pOST = Method.POST;
                RestRequest request = new RestRequest(pOST);
                Uri uri = new Uri("https://www.google.com");
                RestClient client = new RestClient {
                    Proxy = new WebProxy("127.0.0.1:5886"),
                    BaseUrl = uri
                };
                MessageBox.Show(client.Execute(request).Content);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components > null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.PacServer.socket.Close();
            this.PacServer.socket.Dispose();
            this.ProxyServer.socket.Close();
            this.ProxyServer.socket.Dispose();
            Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.PacServer.StartServer(this.label1, this.label3);
            this.ProxyServer.StartServer(this.label4, this.label5, this.label6);
            SetProxy.Set("http://127.0.0.1:5885");
        }

        private void heatBeat(object sender, ElapsedEventArgs e)
        {
            this.webSocketClient.Send("ping");
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.label4 = new Label();
            this.label5 = new Label();
            this.label6 = new Label();
            this.button1 = new Button();
            this.button2 = new Button();
            base.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.ForeColor = Color.Chartreuse;
            this.label1.Location = new Point(13, 0x12);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x79, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "PAC服务器启动中";
            this.label2.AutoSize = true;
            this.label2.ForeColor = Color.Chartreuse;
            this.label2.Location = new Point(0x9c, 0x12);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x43, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "已使用：";
            this.label3.AutoSize = true;
            this.label3.ForeColor = Color.Green;
            this.label3.Location = new Point(0xd7, 0x12);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x2f, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "0/0/0";
            this.label4.AutoSize = true;
            this.label4.ForeColor = Color.DodgerBlue;
            this.label4.Location = new Point(13, 0x31);
            this.label4.Name = "label4";
            this.label4.Size = new Size(0x98, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "Proxy服务器：启动中";
            this.label5.AutoSize = true;
            this.label5.ForeColor = Color.DodgerBlue;
            this.label5.Location = new Point(13, 0x52);
            this.label5.Name = "label5";
            this.label5.Size = new Size(90, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "连接数量：0";
            this.label6.AutoSize = true;
            this.label6.ForeColor = Color.DodgerBlue;
            this.label6.Location = new Point(15, 0x6f);
            this.label6.Name = "label6";
            this.label6.Size = new Size(100, 15);
            this.label6.TabIndex = 5;
            this.label6.Text = "流量：0/0 kb";
            this.button1.Location = new Point(0xda, 0x31);
            this.button1.Name = "button1";
            this.button1.Size = new Size(0x4b, 0x17);
            this.button1.TabIndex = 6;
            this.button1.Text = "测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new EventHandler(this.button1_Click);
            this.button2.Location = new Point(0xda, 0x52);
            this.button2.Name = "button2";
            this.button2.Size = new Size(0x4b, 0x17);
            this.button2.TabIndex = 7;
            this.button2.Text = "测试";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new EventHandler(this.button2_Click);
            base.AutoScaleDimensions = new SizeF(8f, 15f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x141, 0xa3);
            base.Controls.Add(this.button2);
            base.Controls.Add(this.button1);
            base.Controls.Add(this.label6);
            base.Controls.Add(this.label5);
            base.Controls.Add(this.label4);
            base.Controls.Add(this.label3);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.label1);
            base.Name = "Form1";
            this.Text = "Proxy代理";
            base.FormClosed += new FormClosedEventHandler(this.Form1_FormClosed);
            base.Load += new EventHandler(this.Form1_Load);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void webSocketClient_Closed(object sender, CloseEventArgs e)
        {
        }

        private void webSocketClient_Error(object sender, ErrorEventArgs e)
        {
        }

        private void webSocketClient_MessageReceived(object sender, MessageEventArgs e)
        {
        }

        private void webSocketClient_Opened(object sender, EventArgs e)
        {
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Form1.<>c <>9 = new Form1.<>c();
            public static Action <>9__13_0;

            internal void <button2_Click>b__13_0()
            {
                Method pOST = Method.POST;
                RestRequest request = new RestRequest(pOST);
                Uri uri = new Uri("https://www.google.com");
                RestClient client = new RestClient {
                    Proxy = new WebProxy("127.0.0.1:5886"),
                    BaseUrl = uri
                };
                MessageBox.Show(client.Execute(request).Content);
            }
        }
    }
}

