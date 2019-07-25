using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketUPTServer
{
    class UdpSocket
    {
        internal static void Receive(Socket server)
        {

            ReceiveClass rexeive = new ReceiveClass();

            byte[] receiveData = new byte[13];

            EndPoint Remote = new IPEndPoint(IPAddress.Any, 8001);
            server.ReceiveFrom(receiveData, 0, 13, SocketFlags.None, ref Remote);

            rexeive.SetHeard(receiveData);



        }
    }
}
