using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Netsy.Atom.Extensibility
{
    public class SslAtomServerPlugin : NullAtomServerPlugin
    {
        private readonly X509Certificate _certificate;

        public SslAtomServerPlugin(X509Certificate certificate)
        {
            Guard.NotNull(certificate, nameof(certificate));

            this._certificate = certificate;
        }

        public override Stream OnChannelCreatingStream(AtomChannel channel, Stream stream)
        {
            var sslStream = new SslStream(stream);
            sslStream.AuthenticateAsServer(this._certificate);

            return sslStream;
        }
    }
}