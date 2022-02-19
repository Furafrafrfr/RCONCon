using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RCONCon
{
    public class RCONController : IDisposable
    {
        readonly IPEndPoint Endpoint;
        readonly TcpClient client;
        NetworkStream ns;
        readonly int PacketId;
        readonly static Random rnd = new();
        BinaryReader reader;
        BinaryWriter writer;

        public bool IsAuthenticated
        {
            get;
            private set;
        }

        public bool IsConnected
        {
            get
            {
                return client.Connected;
            }
        }
        public RCONController(string IP, int Port = 27015)
        {
            IPAddress addr = IPAddress.Parse(IP);
            this.Endpoint = new IPEndPoint(addr, Port);
            this.client = new TcpClient();
            client.ReceiveTimeout = 5000;
            this.PacketId = rnd.Next();
            client.Connect(Endpoint);
            this.ns = client.GetStream();
            reader = new BinaryReader(ns);
            writer = new BinaryWriter(ns);
        }

        ResponsePacket SendPacket(RequestPacket request)
        {

            WriteRequest(request);
            ResponsePacket response = ReadResponse();

            return response;
        }

        void WriteRequest(RequestPacket request)
        {
            if (!IsConnected)
            {
                throw new ConnectionLostException();
            }



            byte[] sizeBytes = IntToBytesLE(request.SizeParam);
            byte[] idBytes = IntToBytesLE(request.Id);
            byte[] typeBytes = IntToBytesLE((int)request.Type);

            byte[] RequestPayloadBytes = Encoding.ASCII.GetBytes(request.Payload + "\u0000");
            byte[] nul = Encoding.ASCII.GetBytes("\u0000");

            byte[] packetBytes = new byte[RequestPayloadBytes.Length + 13];
            sizeBytes.CopyTo(packetBytes, 0);
            idBytes.CopyTo(packetBytes, 4);
            typeBytes.CopyTo(packetBytes, 8);
            RequestPayloadBytes.CopyTo(packetBytes, 12);
            packetBytes[^1] = nul[0];

            writer.Write(packetBytes);
        }

        static byte[] IntToBytesLE(int value)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                return result;
            }
            else
            {
                return result.Reverse().ToArray();
            }
        }

        ResponsePacket ReadResponse()
        {
            if (!IsConnected)
            {
                throw new ConnectionLostException();
            }

            int size = reader.ReadInt32();
            int id = reader.ReadInt32();
            ResponsePacketType type = (ResponsePacketType)reader.ReadInt32();
            byte[] payloadBytes = reader.ReadBytes(size - 8);
            // nul文字も取り除く
            string payload = Encoding.ASCII.GetString(payloadBytes);
            payload = payload[0..^2];

            return new ResponsePacket(payload, id, type);
        }

        public void Authenticate(string Password)
        {
            RequestPacket request = new(Password, PacketId, RequestPacketType.Auth);
            WriteRequest(request);
            ResponsePacket authRes = ReadResponse();
            if (request.Id == authRes.Id)
            {
                if (authRes.Id == -1)
                {
                    this.IsAuthenticated = false;

                }
                else if (authRes.Type == ResponsePacketType.AuthResponse)
                {
                    this.IsAuthenticated = true;
                }
            }
        }

        public string SendCommand(string Command)
        {
            if (!IsAuthenticated)
            {
                throw new NotAuthenticatedException();
            }
            RequestPacket request = new(Command, PacketId, RequestPacketType.ExecCommand);
            ResponsePacket response = SendPacket(request);

            return response.Payload;
        }

        public bool Reconnect()
        {
            client.Connect(Endpoint);
            if (IsConnected)
            {
                ns = client.GetStream();
                reader = new BinaryReader(ns);
                writer = new BinaryWriter(ns);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            ns.Dispose();
            client.Close();
            client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
