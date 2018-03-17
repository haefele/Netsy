using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Newtonsoft.Json;

namespace Netsy.Tests.Playground
{
    public class NewtonsoftJsonPackageSerializer : IPackageSerializer
    {
        public byte[] Serialize(object package)
        {
            var serialized = JsonConvert.SerializeObject(package, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return Encoding.UTF8.GetBytes(serialized);
        }

        public object Deserialize(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);

            return JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }

    public class TextPackage
    {
        public string Text { get; set; }
    }

    public class AddNumbersRequest
    {
        public double Number1 { get; set; }
        public double Number2 { get; set; }
    }

    public class AddNumbersResponse
    {
        public double Result { get; set; }
    }

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

            var server = new NetsyServer(new IPEndPoint(IPAddress.Any, 1337), new NewtonsoftJsonPackageSerializer(), new SslAtomServerPlugin(certificate.FirstOrDefault()));
            server.OnPackageReceived((TextPackage package) =>
            {
                Console.WriteLine($"Received TextMessage! {package.Text}");
            });
            server.OnRequestReceived((AddNumbersRequest request) =>
            {
                return new AddNumbersResponse
                {
                    Result = request.Number1 + request.Number2
                };
            });
            //server.ChannelConnected += ServerOnChannelConnected;
            //server.ChannelDisconnected += ServerOnChannelDisconnected;

            await server.StartAsync();

            var client = new NetsyClient(new IPEndPoint(IPAddress.Loopback, 1337), new NewtonsoftJsonPackageSerializer(), new SslAtomClientPlugin(certificateSubject));
            client.OnRequestReceived((AddNumbersRequest request) =>
            {
                return new AddNumbersResponse
                {
                    Result = request.Number1 + request.Number2
                };
            });

            //client.MessageReceived += ClientOnMessageReceived;
            await client.ConnectAsync();
            
            while (true)
            {
                var watch = Stopwatch.StartNew();
                var response = await server.Channels.First().SendRequestAsync<AddNumbersResponse>(new AddNumbersRequest
                {
                    Number1 = 2,
                    Number2 = 4
                });
                watch.Stop();
                Console.WriteLine(watch.Elapsed);

                Console.ReadLine();
            }
            
            await client.DisconnectAsync();

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
