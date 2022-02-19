using System.Text;

namespace RCONCon
{
    public enum PacketType
    {
        Response = 0,
        AuthResponseAndExecCommand = 2,
        Auth = 3,
        Error = -1
    }
    public class Packet
    {
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

        public int SizeParam
        {
            get
            {
                Encoding ascii = Encoding.ASCII;
                int payloadLength = ascii.GetBytes(this.Payload).Length;
                int size = 10 + payloadLength;
                return size;
            }
        }
    }
}