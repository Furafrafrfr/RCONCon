using System.Text;

namespace RCONCon
{
    public enum PacketType
    {
        Response = 0,
        AuthResponseAndExecCommand = 2,
        Auth = 3,
        Error = -1,
        IdMismatchError
    }
    public class Packet
    {
        static Random rnd = new Random();
        public Packet(string Payload, int Id)
        {
            this.Payload = Payload;
            this.Id = Id;
        }
        public int Id
        {
            get;
            private set;
        }
        public string Payload
        {
            get;
            private set;
        }
        public int GetSizeParamValue()
        {
            /**
            RequestId       4bytes          
            PacketType      4bytes          
            Payload+nul     varies+1bytes   
            Empty String    1bytes
            -----------------------------
            sum             10 + Payload bytes
            パケットのsizeのパラメータ分のバイト数は含まない
            **/
            Encoding ascii = Encoding.ASCII;
            int payloadLength = ascii.GetBytes(this.Payload).Length;
            int size = 10 + payloadLength;
            return size;
        }
    }   
}