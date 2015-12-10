using System;
using System.Threading;
using System.Net.Sockets;

namespace SpectateServer
{
	public class Relay
	{
		private static String HOST;
		private static int PORT;

		private Thread thread;
		private 

		public void Relay ()
		{
			thread = new Thread (listen ());
			loginToSever ();
			thread.Start ();
		}

		private void listen(){

		}

		private bool loginToSever(){
			
			return false;
		}
	}
}

