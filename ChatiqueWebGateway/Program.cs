using System;

namespace ChatiqueWebGateway
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(new CustomAuth());
        }

        static void Run(IAuth listener)
        {
            Console.WriteLine("Gateway's listening on: {0}", Extensions.MyIP());
            listener.Go();
        }
    }
}