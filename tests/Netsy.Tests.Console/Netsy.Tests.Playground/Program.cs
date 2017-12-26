using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Netsy.Atom;

namespace Netsy.Tests.Playground
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stream = new SslStream(new MemoryStream(), leaveInnerStreamOpen: false, userCertificateValidationCallback: UserCertificateValidationCallback);
            stream.AuthenticateAsClient("test.test");
            stream.AuthenticateAsServer(new X509Certificate());     


            var server = new AtomServer(new IPEndPoint(IPAddress.Any, 1337));
            server.ChannelConnected += ServerOnChannelConnected;
            server.ChannelDisconnected += ServerOnChannelDisconnected;

            await server.StartAsync();

            var client = new AtomClient(new IPEndPoint(IPAddress.Loopback, 1337));
            client.MessageReceived += ClientOnMessageReceived;
            await client.ConnectAsync();

            while (true)
            {
                var data = Encoding.UTF8.GetBytes(Console.ReadLine());
                var message = AtomMessage.FromData(data);

                await client.SendMessageAsync(message);
            }
            
            //await client.DisconnectAsync();

            Console.ReadLine();
        }

        private static bool UserCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static void ClientOnMessageReceived(object sender, AtomClientMessageReceivedEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message.Data);
            Console.WriteLine("CLIENT: " + message);
        }

        private static void ServerOnChannelConnected(object sender, AtomChannelConnectedEventArgs e)
        {
            Console.WriteLine("Client Connected!");

            e.Channel.MessageReceived += ChannelOnMessageReceived;
        }
        private static void ServerOnChannelDisconnected(object sender, AtomChannelDisconnectedEventArgs e)
        {
            Console.WriteLine("Client Disconnected!");

            e.Channel.MessageReceived -= ChannelOnMessageReceived;
        }

        private static async void ChannelOnMessageReceived(object sender, AtomChannelMessageReceivedEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Message.Data);
            Console.WriteLine("SERVER: " + message);

            var channel = (AtomChannel) sender;
            await channel.SendMessageAsync(e.Message);
        }
    }
}
