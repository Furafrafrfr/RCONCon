using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RCONCon
{
    public class RCONController : IDisposable
    {
        IPEndPoint Endpoint;
        TcpClient client;
        NetworkStream ns;
        int PacketId;
        static Random rnd = new Random();
        public bool IsAuthenticated = false;
        BinaryReader reader;
        BinaryWriter writer;
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
            byte[] RequestPayloadBytes = Encoding.ASCII.GetBytes(request.Payload + "\u0000");
            byte[] nul = Encoding.ASCII.GetBytes("\u0000");

            byte[] sizeBytes = IntToBytesLE(request.GetSizeParamValue());
            byte[] idBytes = IntToBytesLE(request.Id);
            byte[] typeBytes = IntToBytesLE((int)request.Type);

            byte[] packetBytes = new byte[RequestPayloadBytes.Length + 13];
            sizeBytes.CopyTo(packetBytes, 0);
            idBytes.CopyTo(packetBytes, 4);
            typeBytes.CopyTo(packetBytes, 8);
            RequestPayloadBytes.CopyTo(packetBytes, 12);
            packetBytes[packetBytes.Length - 1] = nul[0];

            writer.Write(packetBytes);
        }

        byte[] IntToBytesLE(int value)
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
            int size = reader.ReadInt32();
            int id = reader.ReadInt32();
            ResponsePacketType type = (ResponsePacketType)reader.ReadInt32();
            byte[] payloadBytes = reader.ReadBytes(size - 8);
            // nul文字も取り除く
            string payload = Encoding.ASCII.GetString(payloadBytes);
            payload = payload.Substring(0, payload.Length - 2);

            return new ResponsePacket(payload, id, type);
        }

        int BytesToInt32LE(byte[] value)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(value);
            }
            else
            {
                return BitConverter.ToInt32(value.Reverse().ToArray());
            }
        }

        public void Authenticate(string Password)
        {
            RequestPacket request = new RequestPacket(Password, PacketId, RequestPacketType.Auth);
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
                else
                {
                    throw new Exception("Unknown resopnse type error");
                }
            }
            else
            {
                throw new Exception("Invalid value of Id parameter of response. request:" + request + " response:" + authRes);
            }
        }

        public ResponsePacket SendCommand(string Command)
        {
            if (!IsAuthenticated)
            {
                throw new Exception("not authenticated");
            }

            RequestPacket request = new RequestPacket(Command, PacketId, RequestPacketType.ExecCommand);
            ResponsePacket response = SendPacket(request);

            return response;
        }

        public void Dispose()
        {
            ns.Dispose();
            client.Close();
            client.Dispose();
        }
    }
}
