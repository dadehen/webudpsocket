namespace WindowsFormsApp1
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    internal class ProxyServer
    {
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private List<GetSocket> listsocket = new List<GetSocket>();
        private string ipString = "0.0.0.0";
        private int port = 0x16fe;
        private Label ConnectLabel;
        private Label FlowLabel;
        private string user = ("admin" + Guid.NewGuid().ToString("N"));
        private int id = 0;
        private static string baseurl = "http://btcjqr.cn";
        private string CONNECTSendurl = (baseurl + "/admin/api/mysystem/CONNECTSend");
        private string CONNECTCanReadurl = (baseurl + "/admin/api/mysystem/CONNECTCanRead");
        private string ProxyHttpurl = (baseurl + "/admin/api/mysystem/HttpSend");
        private int ConnectNum = 0;
        private int SendFlowNum = 0;
        private int AcceptFlowNum = 0;
        private RestClient clientConnectSend = new RestClient();
        private RestClient clientConnectRead = new RestClient();

        private string GetCONNECTCanRead()
        {
            Method pOST = Method.POST;
            RestRequest request = new RestRequest(pOST);
            request.AddParameter("user", this.user);
            Uri uri = new Uri(this.CONNECTCanReadurl);
            if (this.clientConnectRead.BaseUrl != uri)
            {
                this.clientConnectRead.BaseUrl = uri;
            }
            IRestResponse response = this.clientConnectRead.Execute(request);
            string content = "";
            if (response.StatusCode == HttpStatusCode.OK)
            {
                content = response.Content;
            }
            return content;
        }

        private bool HttpSend(JArray jsondata)
        {
            string str = JsonConvert.SerializeObject(jsondata);
            Method pOST = Method.POST;
            RestRequest request = new RestRequest(pOST);
            request.AddParameter("user", this.user);
            request.AddParameter("senddata", str);
            Uri uri = new Uri(this.CONNECTSendurl);
            if (this.clientConnectSend.BaseUrl != uri)
            {
                this.clientConnectSend.BaseUrl = uri;
            }
            string content = this.clientConnectSend.Execute(request).Content;
            this.SendFlowNum += str.Length;
            this.ShowFlowLabel();
            if (content != "true")
            {
                return false;
            }
            return true;
        }

        private void ProxyConnect(object obj)
        {
            Socket socket = (Socket) obj;
            while (true)
            {
                try
                {
                    Socket parameter = socket.Accept();
                    new Thread(new ParameterizedThreadStart(this.receiveData)).Start(parameter);
                }
                catch (Exception)
                {
                }
            }
        }

        private void ProxyHttp(GetSocket model, byte[] senddata)
        {
            Method pOST = Method.POST;
            RestRequest request = new RestRequest(pOST);
            request.AddParameter("url", model.Url);
            request.AddParameter("port", model.Port);
            request.AddParameter("senddata", Convert.ToBase64String(senddata));
            Uri uri = new Uri(this.ProxyHttpurl);
            RestClient client = new RestClient {
                BaseUrl = uri
            };
            byte[] buffer = Convert.FromBase64String(client.Execute(request).Content);
            if (buffer.Length != 0)
            {
                model.socket.Send(buffer);
                this.AcceptFlowNum += buffer.Length;
                this.ShowFlowLabel();
            }
            model.socket.Close();
        }

        private void receiveData(object obj)
        {
            try
            {
                this.id++;
                Socket socket = (Socket) obj;
                GetSocket item = new GetSocket(socket, this.user, this.id);
                item.GetHeard();
                if (item.Method == "CONNECT")
                {
                    string s = "HTTP/1.1 200 Connection established\r\n\r\n";
                    item.socket.Send(Encoding.ASCII.GetBytes(s));
                    this.listsocket.Add(item);
                }
                else if (((item.Method == "GET") || (item.Method == "POST")) || (item.Method == "HEAD"))
                {
                    item.AcceptContent();
                    this.ProxyHttp(item, item.data);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private void ShowConnectLabel()
        {
            string str = "连接数量：" + this.ConnectNum;
            if (this.ConnectLabel.InvokeRequired)
            {
                SetConnectCallBack method = new SetConnectCallBack(this.ShowConnectLabel);
                this.ConnectLabel.Invoke(method);
            }
            else
            {
                this.ConnectLabel.Text = str;
            }
        }

        private void ShowFlowLabel()
        {
            object[] objArray1 = new object[] { "流量：", this.SendFlowNum, "/", this.AcceptFlowNum, " b" };
            string str = string.Concat(objArray1);
            if (this.FlowLabel.InvokeRequired)
            {
                SetFlowCallBack method = new SetFlowCallBack(this.ShowFlowLabel);
                this.FlowLabel.Invoke(method);
            }
            else
            {
                this.FlowLabel.Text = str;
            }
        }

        public void StartServer(Label label4, Label label5, Label label6)
        {
            try
            {
                IPAddress address = IPAddress.Parse(this.ipString);
                this.socket.Bind(new IPEndPoint(address, this.port));
                this.socket.Listen(0x2710);
                new Thread(new ParameterizedThreadStart(this.ProxyConnect)).Start(this.socket);
                label4.Text = "Proxy服务器：启动成功";
                this.ConnectLabel = label5;
                this.FlowLabel = label6;
                Task.Run(delegate {
                    while (true)
                    {
                        try
                        {
                            string cONNECTCanRead = this.GetCONNECTCanRead();
                            if (cONNECTCanRead.Length > 0)
                            {
                                JArray array = (JArray) JsonConvert.DeserializeObject(cONNECTCanRead);
                                foreach (JToken token in array)
                                {
                                    if (((int) token["length"]) > 0)
                                    {
                                        byte[] buffer = Convert.FromBase64String((string) token["data"]);
                                        foreach (GetSocket socket in this.listsocket)
                                        {
                                            if (((int) token["id"]) == socket.id)
                                            {
                                                socket.socket.Send(buffer);
                                                this.AcceptFlowNum += buffer.Length;
                                                this.ShowFlowLabel();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                });
                Task.Run(delegate {
                    while (true)
                    {
                        try
                        {
                            for (int i = 0; i < this.listsocket.Count; i++)
                            {
                                if (!this.listsocket[i].socket.Connected)
                                {
                                    this.listsocket.RemoveAt(i);
                                    i--;
                                }
                            }
                            JArray jsondata = new JArray();
                            foreach (GetSocket socket in this.listsocket)
                            {
                                if (socket.socket.Available > 0)
                                {
                                    try
                                    {
                                        int available = socket.socket.Available;
                                        if (available > 0)
                                        {
                                            byte[] buffer = new byte[available];
                                            socket.socket.Receive(buffer, available, SocketFlags.None);
                                            JObject item = new JObject();
                                            item["id"] = socket.id;
                                            item["created"] = socket.created;
                                            item["url"] = socket.Url;
                                            item["port"] = socket.Port;
                                            item["data"] = Convert.ToBase64String(buffer);
                                            jsondata.Add(item);
                                            socket.created = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        socket.socket.Close();
                                    }
                                }
                            }
                            if (jsondata.Count > 0)
                            {
                                this.HttpSend(jsondata);
                            }
                            else
                            {
                                this.ConnectNum = this.listsocket.Count;
                                this.ShowConnectLabel();
                                Thread.Sleep(10);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                });
            }
            catch (Exception)
            {
                label4.Text = "Proxy服务器：启动失败";
                label4.ForeColor = Color.DarkRed;
            }
        }

        private delegate void SetConnectCallBack();

        private delegate void SetFlowCallBack();
    }
}

