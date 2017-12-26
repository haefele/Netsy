using System.IO;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public class NullAtomClientPlugin : IAtomClientPlugin
    {
        public void OnAppliedToClient(AtomClient client)
        {
            
        }

        public Task OnConnectingAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnDisconnectingAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnDisconnectedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnSendingMessageAsync(AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public Task OnMessageReceivedAsync(AtomMessage message)
        {
            return Task.CompletedTask;
        }

        public Stream OnCreatingStream(Stream stream)
        {
            return stream;
        }
    }
}