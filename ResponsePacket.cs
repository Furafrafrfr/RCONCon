namespace RCONCon
{
    class ResponsePacket : Packet
    {
        public ResponsePacket(string Payload, int Id, ResponsePacketType Type) : base(Payload, Id)
        {
            this.Type = Type;
        }

        public ResponsePacketType Type
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return $"Id = {Id}, Type = {Type.ToString()}, Payload = {Payload}";
        }
    }
    public enum ResponsePacketType
    {
        CommandResponse = 0,
        AuthResponse = 2
    }
}