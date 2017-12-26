using System.IO;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public class NullAtomServerPlugin : IAtomServerPlugin
    {
        public virtual void OnAppliedToServer(AtomServer server)
        {
            
        }

        public virtual Task OnServerStartingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnServerStartedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnServerStoppingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnServerStoppedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnChannelDisconnectedAsync(AtomChannel channel)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnChannelConnectedAsync(AtomChannel channel)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnChannelSendingMessageAsync(AtomChannel channel, AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnChannelMessageReceivedAsync(AtomChannel channel, AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public virtual Stream OnChannelCreatingStream(AtomChannel channel, Stream stream)
        {
            return stream;
        }
    }
}