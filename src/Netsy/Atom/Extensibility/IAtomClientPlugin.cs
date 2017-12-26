using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public interface IAtomClientPlugin
    {
        void OnAppliedToClient(AtomClient client);
        
        Task OnConnectingAsync();
        Task OnConnectedAsync();

        Task OnDisconnectingAsync();
        Task OnDisconnectedAsync();

        Task OnSendingMessageAsync(AtomMessage message);
        Task OnMessageReceivedAsync(AtomMessage message);

        Stream OnCreatingStream(Stream stream);
    }
}