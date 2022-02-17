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
        }

        ResponsePacket SendPacket(RequestPacket request)
        {
            WriteRequest(request);
            ResponsePacket response = ReadResponse();

            return response;
        }

        void WriteRequest(RequestPacket request)
        {
            Encoding ascii = Encoding.ASCII;
            byte[] RequestPayloadBytes = ascii.GetBytes(request.Payload + "\u0000");
            byte[] nul = ascii.GetBytes("\u0000");

            byte[] sizeBytes = IntToBytesLE(request.GetSizeParamValue());
            byte[] idBytes = IntToBytesLE(request.Id);
            byte[] typeBytes = IntToBytesLE((int)request.Type);

            byte[] packetBytes = new byte[RequestPayloadBytes.Length + 13];
            sizeBytes.CopyTo(packetBytes, 0);
            idBytes.CopyTo(packetBytes, 4);
            typeBytes.CopyTo(packetBytes, 8);
            RequestPayloadBytes.CopyTo(packetBytes, 12);
            packetBytes[packetBytes.Length - 1] = nul[0];

            ns.Write(packetBytes, 0, packetBytes.Length);
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
            // byte[] sizeBytes = new byte[4];
            // int sizeSuccess = ns.Read(sizeBytes, 0, 4);
            // byte[] idBytes = new byte[4];
            // int idSuccess = ns.Read(sizeBytes, 0, 4);
            // byte[] typeBytes = new byte[4];
            // int typeSuccess = ns.Read(sizeBytes, 0, 4);

            // Console.WriteLine($"Number of bytes successfully readed: size=${sizeSuccess}, id={idSuccess}, type={typeSuccess}");
            // int size = BytesToInt32LE(sizeBytes);
            // int id = BytesToInt32LE(idBytes);
            // ResponsePacketType type = (ResponsePacketType)BytesToInt32LE(typeBytes);

            // int payloadSize = size - 8;
            // byte[] payloadBytes = new byte[payloadSize];
            // ns.Read(payloadBytes, 0, payloadSize);
            // string payload = Encoding.ASCII.GetString(payloadBytes);

            // ResponsePacket response = new ResponsePacket(payload, id, type);
            // return response;

            int size = reader.ReadInt32();
            int id = reader.ReadInt32();
            ResponsePacketType type = (ResponsePacketType)reader.ReadInt32();
            byte [] packetBytes = reader.ReadBytes(size - 10);
            string packet = Encoding.ASCII.GetString(packetBytes);

            return new ResponsePacket(packet, id, type);
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

        int ReadInt32(byte[] source, int start)
        {
            byte[] resultBytes = new byte[4];
            Array.Copy(source, start, resultBytes, 0, 4);
            return BytesToInt32LE(resultBytes);
        }

        public void Authenticate(string Password)
        {
            RequestPacket request = new RequestPacket(Password, PacketId, RequestPacketType.Auth);
            WriteRequest(request);
            Task.Delay(2000);
            ResponsePacket response = ReadResponse();
            if (request.Id == response.Id)
            {
                ResponsePacket authResult = ReadResponse();
                if (authResult.Id == -1)
                {
                    this.IsAuthenticated = false;

                }
                else if (authResult.Type == ResponsePacketType.AuthResponse)
                {
                    this.IsAuthenticated = true;
                }
            }
            else
            {
                throw new Exception("invalid value of Id parameter of response. request:" + request + " response:" + response);
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
