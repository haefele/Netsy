using System.IO;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public class NullAtomClientPlugin : IAtomClientPlugin
    {
        public virtual void OnAppliedToClient(AtomClient client)
        {
            
        }

        public virtual Task OnConnectingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDisconnectingAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDisconnectedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnSendingMessageAsync(AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnMessageReceivedAsync(AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public virtual Stream OnCreatingStream(Stream stream)
        {
            return stream;
        }
    }
}