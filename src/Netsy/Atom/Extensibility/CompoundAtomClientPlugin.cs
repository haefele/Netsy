using System.IO;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public class CompoundAtomClientPlugin : IAtomClientPlugin
    {
        private readonly IAtomClientPlugin[] _plugins;

        public CompoundAtomClientPlugin(params IAtomClientPlugin[] plugins)
        {
            Guard.NotNullOrEmpty(plugins, nameof(plugins));

            this._plugins = plugins;
        }

        public void OnAppliedToClient(AtomClient client)
        {
            foreach (var plugin in this._plugins)
            {

            }
        }

        public async Task OnConnectingAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnConnectingAsync();
            }
        }

        public async Task OnConnectedAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnConnectedAsync();
            }
        }

        public async Task OnDisconnectingAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnDisconnectingAsync();
            }
        }

        public async Task OnDisconnectedAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnDisconnectedAsync();
            }
        }

        public async Task OnSendingMessageAsync(AtomMessage message)
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnSendingMessageAsync(message);
            }
        }

        public async Task OnMessageReceivedAsync(AtomMessage message)
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnMessageReceivedAsync(message);
            }
        }

        public Stream OnCreatingStream(Stream stream)
        {
            foreach (var plugin in this._plugins)
            {
                stream = plugin.OnCreatingStream(stream);
            }

            return stream;
        }
    }
}