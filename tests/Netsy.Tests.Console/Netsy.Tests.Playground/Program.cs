using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Netsy.Atom;
using Netsy.Atom.Extensibility;

namespace Netsy.Tests.Playground
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var certificateSubject = "desktop-haefele";
        
            var certificateStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certificateStore.Open(OpenFlags.ReadOnly);

            var certificate = certificateStore.Certificates
                .Find(X509FindType.FindBySubjectName, certificateSubject, false)
                .Cast<X509Certificate2>()
                .ToList();

            var server = new AtomServer(new IPEndPoint(IPAddress.Any, 1337), new SslAtomServerPlugin(certificate.FirstOrDefault()));
            server.ChannelConnected += ServerOnChannelConnected;
            server.ChannelDisconnected += ServerOnChannelDisconnected;

            await server.StartAsync();

            var client = new AtomClient(new IPEndPoint(IPAddress.Loopback, 1337), new SslAtomClientPlugin(certificateSubject));
            //var netsyClient = new NetsyClient(client, null);
            //netsyClient.OnRequestReceived((object o) =>
            //{
            //    return "swag";
            //});

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
