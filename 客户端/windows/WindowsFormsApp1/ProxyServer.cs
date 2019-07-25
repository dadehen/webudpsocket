using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1
{
    class ProxyServer
    {
        public Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<GetSocket> listsocket = new List<GetSocket>();

        //string ipString = "127.0.0.1";   // 服务器端ip
        string ipString = "0.0.0.0";   // 服务器端ip
        int port = 5886;                // 服务器端口

        Label ConnectLabel;
        Label FlowLabel;

        string user = "admin"+ Guid.NewGuid().ToString("N");  
        int id = 0;
        static string baseurl = "http://btcjqr.cn";
        //static string baseurl = "http://47.99.65.252";
        //static string baseurl = "http://localhost:51376";

        string CONNECTSendurl = baseurl + "/admin/api/mysystem/CONNECTSend"; 
        string CONNECTCanReadurl = baseurl + "/admin/api/mysystem/CONNECTCanRead";
        string ProxyHttpurl = baseurl + "/admin/api/mysystem/HttpSend";
        

        int ConnectNum = 0;
        int SendFlowNum = 0;
        int AcceptFlowNum = 0;

        public void StartServer(Label label4, Label label5, Label label6)
        {
            try
            {
                IPAddress address = IPAddress.Parse(ipString);
                socket.Bind(new IPEndPoint(address, port));
                socket.Listen(10000);
                new Thread(ProxyConnect).Start(socket);  // 在新的线程中监听客户端连接
                label4.Text = "Proxy服务器：启动成功";
                ConnectLabel = label5;
                FlowLabel = label6;

                Task.Run(() =>
                {
                    while(true)
                    {
                        try
                        {
                            string strreaddata = GetCONNECTCanRead();

                            if (strreaddata.Length > 0)
                            {
                                JArray readdata = (JArray)JsonConvert.DeserializeObject(strreaddata);

                                foreach (var item in readdata)
                                {
                                    if ((int)item["length"] > 0)
                                    {
                                        byte[] data = Convert.FromBase64String((string)item["data"]);

                                        foreach (var mysocket in listsocket)
                                        {
                                            if ((int)item["id"] == mysocket.id)
                                            {
                                                mysocket.socket.Send(data);
                                                AcceptFlowNum += data.Length;
                                                ShowFlowLabel(); 
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {

                        }
                        
                    } 
                });

                // Send数据
                Task.Run(() =>
                {
                    while(true)
                    {
                        try
                        { 
                            for (int i = 0; i < listsocket.Count; i++)
                            {
                                if(listsocket[i].socket.Connected==false)
                                {
                                    listsocket.RemoveAt(i);
                                    i--;
                                }
                            }
                           
                            JArray jsondata = new JArray(); 
                            foreach (var mysocket in listsocket)
                            { 
                                if (mysocket.socket.Available > 0)
                                { 
                                    try
                                    {
                                        int length = mysocket.socket.Available;
                                        if(length>0)
                                        {
                                            byte[] senddata = new byte[length];
                                            mysocket.socket.Receive(senddata, length, SocketFlags.None); 
                                            JObject data = new JObject(); 
                                            data["id"] = mysocket.id;
                                            data["created"] = mysocket.created;
                                            data["url"] = mysocket.Url;
                                            data["port"] = mysocket.Port;
                                            data["data"] = Convert.ToBase64String(senddata);
                                            jsondata.Add(data);
                                            mysocket.created = true;
                                        } 
                                    }
                                    catch(Exception ex)
                                    {
                                        mysocket.socket.Close();  
                                    } 
                                }
                            }

                            if(jsondata.Count>0)
                            {
                                HttpSend(jsondata);
                            }
                            else
                            {
                                ConnectNum = listsocket.Count;
                                ShowConnectLabel();
                                Thread.Sleep(10);
                            }
                        }
                        catch(Exception ex)
                        {

                        }
                       
                    } 
                });
            }
            catch (Exception ex)
            {
                label4.Text = "Proxy服务器：启动失败";
                label4.ForeColor = Color.DarkRed;
            }
        }

        RestClient clientConnectSend = new RestClient();
        private bool HttpSend(JArray jsondata)
        {
            string senddata = JsonConvert.SerializeObject(jsondata);
            Method methodTask = Method.POST;
            RestRequest requestTask = new RestRequest(methodTask);
            requestTask.AddParameter("user", user);
            requestTask.AddParameter("senddata", senddata);

            Uri uriTask = new Uri(CONNECTSendurl);

            if(clientConnectSend.BaseUrl != uriTask)
            {
                clientConnectSend.BaseUrl = uriTask;
            }
            IRestResponse responseTask = clientConnectSend.Execute(requestTask);
            string dataTask = responseTask.Content;

            SendFlowNum += senddata.Length;
            ShowFlowLabel();

            if (dataTask != "true")
            {
                return false;
            }
            return true;
        }

        RestClient clientConnectRead = new RestClient();
        private string GetCONNECTCanRead()
        {  
            Method method = Method.POST;
            RestRequest request = new RestRequest(method);
            request.AddParameter("user", user); 

            Uri uri = new Uri(CONNECTCanReadurl);

            if(clientConnectRead.BaseUrl!= uri)
            {
                clientConnectRead.BaseUrl = uri;
            }
            IRestResponse response = clientConnectRead.Execute(request);
            string strreaddata = "";
            if(response.StatusCode==HttpStatusCode.OK)
            {
                strreaddata = response.Content;
            }
            return strreaddata;
        }

        private void ProxyConnect(object obj)
        {
            Socket socket = (Socket)obj;
            while (true)
            {
                try
                {
                    Socket clientScoket = socket.Accept();
                    new Thread(receiveData).Start(clientScoket);   // 在新的线程中接收客户端信息 
                }
                catch(Exception ex)
                {

                }
            }
        }

        private void receiveData(object obj)
        {
            try
            { 
                id++;
                Socket socket = (Socket)obj;
                GetSocket model = new GetSocket(socket, user, id);
                model.GetHeard();

                if(model.Method == "CONNECT")
                {
                    //Socket socketproxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    //socketproxy.Connect("127.0.0.1", 1080);
                    //socketproxy.Send(model.data);

                    //int datalen = socketproxy.Available;
                    //byte[] dataproxy = new byte[datalen];

                    //while (datalen == 0)
                    //{
                    //    datalen = socketproxy.Available;
                    //    Thread.Sleep(10);
                    //}

                    //while (datalen > 0)
                    //{
                    //    dataproxy = new byte[datalen];
                    //    socketproxy.Receive(dataproxy, datalen, SocketFlags.None);
                    //    datalen = socketproxy.Available;
                    //}
                    //model.socket.Send(dataproxy);

                    string creatok = "HTTP/1.1 200 Connection established\r\n\r\n";
                    model.socket.Send(Encoding.ASCII.GetBytes(creatok));

                    listsocket.Add(model);

                    //while (true)
                    //{
                    //    Thread.Sleep(10);
                    //}

                    //datalen = socketproxy.Available;
                    //while (datalen == 0)
                    //{
                    //    datalen = socketproxy.Available;
                    //    Thread.Sleep(10);
                    //}

                    //while (datalen > 0)
                    //{
                    //    dataproxy = new byte[datalen];
                    //    socketproxy.Receive(dataproxy, datalen, SocketFlags.None);
                    //    datalen = socketproxy.Available;
                    //}

                }
                else if (model.Method == "GET"||model.Method =="POST"||model.Method== "HEAD")
                {
                    model.AcceptContent();
                    ProxyHttp(model, model.data);
                } 
                else
                {

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void ProxyHttp(GetSocket model, byte[] senddata)
        {
            Method method = Method.POST;
            RestRequest request = new RestRequest(method); 
            request.AddParameter("url", model.Url);
            request.AddParameter("port", model.Port);
            request.AddParameter("senddata", Convert.ToBase64String(senddata));

            Uri uri = new Uri(ProxyHttpurl);
            RestClient client = new RestClient();

            client.BaseUrl = uri;
            IRestResponse response = client.Execute(request);

            byte[] data = Convert.FromBase64String(response.Content);

            if (data.Length > 0)
            {
                model.socket.Send(data);
                AcceptFlowNum += data.Length;
                ShowFlowLabel();
            }
            model.socket.Close();
        }
           
        delegate void SetConnectCallBack();
        delegate void SetFlowCallBack();
         
        private void ShowConnectLabel()
        {
            string text = "连接数量：" + ConnectNum;
            if (this.ConnectLabel.InvokeRequired)
            {
                SetConnectCallBack stcb = new SetConnectCallBack(ShowConnectLabel);
                this.ConnectLabel.Invoke(stcb);
            }
            else
            {
                this.ConnectLabel.Text = text;
            }
        }

        private void ShowFlowLabel()
        {
            string text = "流量：" + SendFlowNum +"/"+ AcceptFlowNum+" b";
            if (this.FlowLabel.InvokeRequired)
            {
                SetFlowCallBack stcb = new SetFlowCallBack(ShowFlowLabel);
                this.FlowLabel.Invoke(stcb);
            }
            else
            {
                this.FlowLabel.Text = text;
            }
        }
         
    }
}
