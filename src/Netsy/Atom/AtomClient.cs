using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Netsy.Extensions;

namespace Netsy.Atom
{
    public class AtomClient
    {
        private TcpClient _client;
        public IPEndPoint Address { get; }

        public AtomClient(IPEndPoint address)
        {
            this.Address = address;
        }

        public async Task ConnectAsync()
        {
            var client = new TcpClient();
            client.Connect(this.Address);

            this._client = client;
        }


        public async Task DisconnectAsync()
        {
            this._client.Dispose();
            this._client = null;
        }

        public void Test()
        {
            var stream =  this._client.GetStream();

            var data = Encoding.UTF8.GetBytes("Hallo Welt!");
            stream.WriteRawMessageAsync(data).Wait();
        }
    }
}