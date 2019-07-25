using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketUPTServer
{
    public class ReceiveClass
    {
        public class HeaderClass
        {
            public HeaderClass()
            {

            }

            public HeaderClass(byte[] data)
            {

            }
        }

        public ReceiveClass()
        {

        }

        

        public byte[] HrardData;
        public HeaderClass Hrard;
        public byte[] Data;
        public string Md5;

        internal void SetHeard(byte[] receiveData)
        {
            throw new NotImplementedException();
        }

        public bool IsNormal;



    }
}
