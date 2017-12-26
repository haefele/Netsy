using System;
using System.IO;
using System.Threading.Tasks;

namespace Netsy.Atom.Extensibility
{
    public interface IAtomServerPlugin
    {
        void OnAppliedToServer(AtomServer server);

        Task OnServerStartingAsync();
        Task OnServerStartedAsync();

        Task OnServerStoppingAsync();
        Task OnServerStoppedAsync();

        Task OnChannelConnectedAsync(AtomChannel channel);
        Task OnChannelDisconnectedAsync(AtomChannel channel);

        Task OnChannelSendingMessageAsync(AtomChannel channel, AtomMessage message);
        Task OnChannelMessageReceivedAsync(AtomChannel channel, AtomMessage message);

        Stream OnChannelCreatingStream(AtomChannel channel, Stream stream);
    }
}