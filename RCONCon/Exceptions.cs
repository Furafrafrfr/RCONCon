namespace RCONCon
{
    public class NotAuthenticatedException : Exception
    {
        public NotAuthenticatedException() : base("Not authenticated. You have to call RCONCon.Authenticate() method first.")
        {

        }
    }

    public class ConnectionLostException : Exception
    {
        public ConnectionLostException() : base("Connection was lost.")
        {

        }
    }

    public class InvalidIdResponseException : Exception
    {
        public InvalidIdResponseException() : base("Request id and response id doesn't match.")
        {

        }
    }
}