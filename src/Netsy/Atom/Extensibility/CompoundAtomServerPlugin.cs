using System.IO;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public class CompoundAtomServerPlugin : IAtomServerPlugin
    {
        private readonly IAtomServerPlugin[] _plugins;

        public CompoundAtomServerPlugin(params IAtomServerPlugin[] plugins)
        {
            Guard.NotNullOrEmpty(plugins, nameof(plugins));

            this._plugins = plugins;
        }
        
        public void OnAppliedToServer(AtomServer server)
        {
            foreach (var plugin in this._plugins)
            {
                plugin.OnAppliedToServer(server);
            }
        }

        public async Task OnServerStartingAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnServerStartingAsync();
            }
        }

        public async Task OnServerStartedAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnServerStartedAsync();
            }
        }

        public async Task OnServerStoppingAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnServerStoppingAsync();
            }
        }

        public async Task OnServerStoppedAsync()
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnServerStoppedAsync();
            }
        }

        public async Task OnChannelConnectedAsync(AtomChannel channel)
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnChannelConnectedAsync(channel);
            }
        }

        public async Task OnChannelDisconnectedAsync(AtomChannel channel)
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnChannelDisconnectedAsync(channel);
            }
        }

        public async Task OnChannelSendingMessageAsync(AtomChannel channel, AtomMessage message)
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnChannelSendingMessageAsync(channel, message);
            }
        }

        public async Task OnChannelMessageReceivedAsync(AtomChannel channel, AtomMessage message)
        {
            foreach (var plugin in this._plugins)
            {
                await plugin.OnChannelMessageReceivedAsync(channel, message);
            }
        }

        public Stream OnChannelCreatingStream(AtomChannel channel, Stream stream)
        {
            foreach (var plugin in this._plugins)
            {
                stream = plugin.OnChannelCreatingStream(channel, stream);
            }

            return stream;
        }
    }
}