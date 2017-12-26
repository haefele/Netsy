using System.IO;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public class NullAtomServerPlugin : IAtomServerPlugin
    {
        public void OnAppliedToServer(AtomServer server)
        {
            
        }

        public Task OnServerStartingAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnServerStartedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnServerStoppingAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnServerStoppedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnChannelDisconnectedAsync(AtomChannel channel)
        {
            return Task.CompletedTask;
        }

        public Task OnChannelConnectedAsync(AtomChannel channel)
        {
            return Task.CompletedTask;
        }

        public Task OnChannelSendingMessageAsync(AtomChannel channel, AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public Task OnChannelMessageReceivedAsync(AtomChannel channel, AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public Stream OnChannelCreatingStream(AtomChannel channel, Stream stream)
        {
            return stream;
        }
    }
}