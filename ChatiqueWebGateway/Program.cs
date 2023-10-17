using System;
using System.Threading.Tasks;

namespace ChatiqueWebGateway
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Run(new CustomAuth());
        }

        static async Task Run(IAuth listener)
        {
            Console.WriteLine("Gateway's listening on: {0}", Extensions.MyIP());
            await listener.GoAsync();
        }
    }
}