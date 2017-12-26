using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Netsy.Atom;

namespace Netsy.Tests.Playground
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var server = new AtomServer(new IPEndPoint(IPAddress.Any, 1337));
            server.ChannelConnected += ServerOnChannelConnected;
            server.ChannelDisconnected += ServerOnChannelDisconnected;

            await server.StartAsync();

            var client = new AtomClient(new IPEndPoint(IPAddress.Loopback, 1337));
            await client.ConnectAsync();

            Console.ReadLine();

            //client.Test();
            //client.Test();
            //client.Test();
            //client.Test();
            //client.Test();
            //client.Test();
            //client.Test();

            await client.DisconnectAsync();

            Console.ReadLine();
        }

        private static void ServerOnChannelConnected(object sender, AtomChannelConnectedEventArgs e)
        {
            Console.WriteLine("Client Connected!");
            //e.Channel.Disconnect();

            //e.Channel.MessageReceived += ChannelOnMessageReceived;
        }
        private static void ServerOnChannelDisconnected(object sender, AtomChannelDisconnectedEventArgs e)
        {
            e.Channel.MessageReceived -= ChannelOnMessageReceived;
            Console.WriteLine("Client Disconnected!");
        }

        private static void ChannelOnMessageReceived(object sender, AtomChannelMessageReceivedEventArgs e)
        {
            Console.WriteLine("Message Received!");
            var message = Encoding.UTF8.GetString(e.Message.Data);
            Console.WriteLine(message);
        }

    }
}
