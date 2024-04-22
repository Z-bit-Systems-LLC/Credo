using System;
using System.Threading.Tasks;
using Adapter;

namespace Credo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var adapterConnection = new Connection("http://localhost:5844/");

            bool validProfile = await adapterConnection.CheckProfile();
            if (!validProfile)
            {
                Console.WriteLine("Doesn't support PLAI.");
                return;
            }

            await adapterConnection.TakeOwnership("user", "user1234");
        }
    }
}