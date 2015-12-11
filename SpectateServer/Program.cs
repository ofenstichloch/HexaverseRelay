using System;

namespace SpectateServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            Relay.createNewRelay("Client1", "localhost", 10244);
            Console.ReadLine();
		}
	}
}
