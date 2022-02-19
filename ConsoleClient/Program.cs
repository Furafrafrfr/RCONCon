using System;

using RCONCon;

namespace ConsoleClient
{
    class ConsoleApp
    {
        public static void Main(string[] args)
        {
            if(args.Length == 2){
                Console.WriteLine("IP address and port must be specified.");
            }
            string ipStr = args[0];
            string portStr = args[1];

            RCONController controller = new(ipStr, int.Parse(portStr));

            controller.Authenticate("testing");

            if (controller.IsAuthenticated)
            {
                Console.WriteLine("Successfully authenticated.");
                while (true)
                {
                    Console.Write(">");
                    string? command = Console.ReadLine();
                    if (command != null)
                    {
                        string response = controller.SendCommand(command);
                        if (response != string.Empty)
                        {
                            Console.WriteLine(response);
                        }
                    }
                }
            }

            controller.Dispose();
        }
    }
}
