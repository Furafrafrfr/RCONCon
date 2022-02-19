namespace RCONCon
{
    class RequestPacket : Packet
    {
        public RequestPacket(string Payload, int Id, RequestPacketType Type) : base(Payload, Id)
        {
            this.Type = Type;
        }

        public RequestPacketType Type{
            get;
            private set;
        }

        public override string ToString()
        {
            return $"Id = {Id}, Type = {Type}, Payload = {Payload}";
        }
    }
    enum RequestPacketType{
        None = 0,
        ExecCommand = 2,
        Auth = 3
    }
}