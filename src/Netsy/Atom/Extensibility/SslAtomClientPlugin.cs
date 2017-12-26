using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Netsy.Atom.Extensibility
{
    public class SslAtomClientPlugin : NullAtomClientPlugin
    {
        private readonly string _targetHost;
        private readonly RemoteCertificateValidationCallback _validationCallback;

        public SslAtomClientPlugin(string targetHost, RemoteCertificateValidationCallback validationCallback = null)
        {
            Guard.NotNullOrWhiteSpace(targetHost, nameof(targetHost));

            this._targetHost = targetHost;
            this._validationCallback = validationCallback;
        }

        public override Stream OnCreatingStream(Stream stream)
        {
            var sslStream = new SslStream(stream, false, this.RemoteCertificateValidationCallback);
            sslStream.AuthenticateAsClient(this._targetHost);

            return sslStream;
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (this._validationCallback != null)
                return this._validationCallback(sender, certificate, chain, sslPolicyErrors);

            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}