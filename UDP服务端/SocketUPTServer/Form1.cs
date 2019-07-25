using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SocketUPTServer
{
    public partial class Form1 : Form
    {
        List<RowClass> TableData = new List<RowClass>();
        List<RowClass> OldTableData = new List<RowClass>();
        Socket server = null;

        class RowClass
        {
            public string No;
            public string Ip;
            public int Port;

            /// <summary>
            /// 心跳包 是udp下端向此端发送得时间。获取 macserver webserver时仅获取最新的
            /// </summary>
            public DateTime HeartTime = new DateTime();
            /// <summary>
            /// 心跳次数，连续心跳次数
            /// </summary>
            public int HeartNum = 0;
            /// <summary>
            ///  type 分为 macserver macclient webserver webclient，有可能会在后面加 webserver_1 webclient_1
            /// </summary>
            public string Type; 
        }

        class ClientData
        {
            public string Type; 
            public string No;
            public string Data;
        }

        public Form1()
        {
            InitializeComponent();
        }

       
        private void Form1_Load(object sender, EventArgs e)
        {
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timer1.Interval = 100;
            timer1.Elapsed += Timer1_Tick;
            timer1.Start();

            System.Timers.Timer timer2 = new System.Timers.Timer();
            timer2.Interval = 5000;
            timer2.Elapsed += Timer2_Tick;
            timer2.Start();

            IPEndPoint loaclEndPoint = new IPEndPoint(IPAddress.Any, 8001);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(loaclEndPoint);

            Task.Run(() => {
               
                while(true)
                {
                    try
                    {

                        UdpSocket.Receive(server);

                        byte[] receiveData = new byte[1024];

                        EndPoint Remote = loaclEndPoint;
                        server.ReceiveFrom(receiveData, 0, 1024, SocketFlags.None, ref Remote);
                        string stringData = Encoding.UTF8.GetString(receiveData);
                        ClientData clientData = JsonConvert.DeserializeObject<ClientData>(stringData);

                        string IpPort = Remote.ToString();//获取发送端的IP及端口转为String备用
                        string ip = IpPort.Split(':')[0];
                        int port = int.Parse(IpPort.Split(':')[1]);

                        if (clientData.Type == "Ping")
                        {
                            bool isHaveConnect = false;
                            for (int i = 0; i < TableData.Count; i++)
                            { 
                                if(TableData[i].No == clientData.No)
                                {
                                    isHaveConnect = true;

                                    if (TableData[i].Ip == ip &&
                                        TableData[i].Port == port)
                                    {
                                        TableData[i].HeartNum++;
                                        TableData[i].HeartTime = DateTime.Now;
                                    }
                                    else
                                    {
                                        TableData[i].Ip = ip;
                                        TableData[i].Port = port;
                                        TableData[i].HeartNum = 0;
                                        TableData[i].HeartTime = DateTime.Now;
                                    }
                                }
                               
                            }
                            if(isHaveConnect == false)
                            {
                                RowClass udpData = new RowClass();
                                udpData.No = clientData.No;
                                udpData.Ip = ip;
                                udpData.Port = port;
                                udpData.HeartNum = 0;
                                udpData.HeartTime = DateTime.Now;
                                udpData.Type = clientData.Data;

                                TableData.Add(udpData);
                            }

                        }

                        else if (clientData.Type == "Get") // Get 
                        {
                            List<RowClass> runData = new List<RowClass>();

                            if(clientData.Data.Contains("server"))
                            {
                                runData = TableData.Where(it => it.Type == clientData.Data).OrderByDescending(it=>it.HeartTime).Take(1).ToList();
                            }
                            if (clientData.Data.Contains("client"))
                            {
                                runData = TableData.Where(it => it.Type == clientData.Data).OrderByDescending(it => it.HeartTime).ToList();
                            }
                            else if(clientData.Data=="All")
                            {
                                runData = TableData.ToList();
                            }

                            SocketUdpSend(ip, port, "GetData", JsonConvert.SerializeObject(runData));
                        }

                        else if (clientData.Type == "Send") // Get 
                        {
                            RowClass SendClient = new RowClass();

                            SendClient = TableData.Where(it => it.No == clientData.No).FirstOrDefault();

                            if(SendClient!=null)
                            {
                                SocketUdpSend(SendClient.Ip, SendClient.Port, "Send", IpPort);
                            }
                        }

                    }
                    catch
                    {

                    }
                }
            });
        }

        private void SocketUdpSend(string ip, int port, string type, string data)
        {
            //Socket socket = new Socket(ipe.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            //socket.Connect(ipe);
            if (server!=null && server.Connected)
            {
                ClientData sendData = new ClientData();
                sendData.Type = type;
                sendData.Data = data;

                string sendStrData = JsonConvert.SerializeObject(sendData);
                byte[] sendByteData = Encoding.UTF8.GetBytes(sendStrData);
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
                server.SendTo(sendByteData, sendByteData.Length, 0, ipe);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            bool isEqual = true;

            if(TableData.Count!=OldTableData.Count)
            {
                isEqual = false;
            }


            for (int i = 0; isEqual && i < TableData.Count; i++)
            {
                if(JsonConvert.SerializeObject(TableData[i])!= JsonConvert.SerializeObject(OldTableData[i]))
                {
                    isEqual = false;
                }
            }

            if(isEqual == false)
            {
                OldTableData = JsonConvert.DeserializeObject<List<RowClass>>(JsonConvert.SerializeObject(TableData));

                dataGridView1.Rows.Clear();
                foreach (var item in OldTableData)
                {
                    dataGridView1.Rows.Add(item.No,item.Ip,item.Port,item.HeartTime.ToString("HH:dd:ss"), item.HeartNum.ToString(),item.Type);
                }
            }
           
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if(OldTableData.Count(it=>it.HeartTime.AddMilliseconds(10000)>DateTime.Now && it.Type == "webserver")==0)
                {
                    Method method = Method.GET;
                    RestRequest request = new RestRequest(method);
                    request.AddParameter("ip", "");
                    request.AddParameter("port", 8001);
                    RestClient client = new RestClient(); 
                    client.BaseUrl = "http://btcjqr.cn/admin/api/mysystem/udpwebserver";
                    IRestResponse response = client.Execute(request);
                }

                foreach (var item in OldTableData)
                {
                    SocketUdpSend(item.Ip, item.Port, "Ping", "Udp中转服务");
                }

                OldTableData = OldTableData.Where(it => it.HeartTime.AddHours(1) > DateTime.Now).ToList();
            }
            catch
            {

            }
        } 
    }
}
