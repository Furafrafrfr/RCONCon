using System;

using RCONCon;

namespace ConsoleClient
{
    class ConsoleApp
    {
        public static void Main(string[] args)
        {
            if(args.Length != 2){
                Console.WriteLine("IP address and port must be passed as arguments.");
            }
            string ipStr = args[0];
            string portStr = args[1];

            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelEventHandler);

            RCONController controller = new(ipStr, int.Parse(portStr));

            controller.Authenticate("testing");

            if (controller.IsAuthenticated)
            {
                Console.WriteLine("Successfully authenticated.");
                Console.WriteLine("Press ctrl+C when exit.");
                while (true)
                {
                    Console.Write("> ");
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

        static void CancelEventHandler(object? sender, ConsoleCancelEventArgs args){
            Console.WriteLine("\nBye");
        }
    }
}
