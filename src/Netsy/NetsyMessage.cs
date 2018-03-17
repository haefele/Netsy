using System;
using Netsy.Atom;

namespace Netsy
{
    public struct NetsyMessage
    {
        public byte[] Data { get;  }
        public NetsyMessageKind Kind { get; }
        public Guid RequestId { get; }

        public static NetsyMessage Package(byte[] data)
        {
            return new NetsyMessage(data, NetsyMessageKind.SendPackage, Guid.Empty);
        }

        public static NetsyMessage Request(byte[] data)
        {
            return new NetsyMessage(data, NetsyMessageKind.Request, Guid.NewGuid());
        }

        public static NetsyMessage ResponseFor(byte[] data, Guid requestId)
        {
            return new NetsyMessage(data, NetsyMessageKind.Response, requestId);
        }

        public static NetsyMessage FromAtomMessage(byte[] data)
        {
            var kind = (NetsyMessageKind) data[0];

            var requestId = Guid.Empty;
            int bytesToSkip = 0;

            switch (kind)
            {
                case NetsyMessageKind.Request:
                case NetsyMessageKind.Response:
                    var requestIdBytes = new byte[16];
                    Array.Copy(data, 1, requestIdBytes, 0, 16);
                    requestId = new Guid(requestIdBytes);
                    bytesToSkip = 16;

                    break;
            }

            var actualData = new byte[data.Length - 1 - bytesToSkip];
            Array.Copy(data, 1 + bytesToSkip, actualData, 0, actualData.Length);

            return new NetsyMessage(actualData, kind, requestId);
        }

        private NetsyMessage(byte[] data, NetsyMessageKind kind, Guid requestId)
        {
            this.Data = data;
            this.Kind = kind;
            this.RequestId = requestId;
        }

        public AtomMessage ToAtomMessage()
        {
            return AtomMessage.FromData(this.GetData());
        }

        private byte[] GetData()
        {
            var result = new byte[this.GetDataLength()];
            result[0] = (byte) this.Kind;

            switch (this.Kind)
            {
                case NetsyMessageKind.Request:
                case NetsyMessageKind.Response:
                    Array.Copy(this.RequestId.ToByteArray(), 0, result, 1, 16);
                    break;
            }

            Array.Copy(this.Data, 0, result, result.Length - this.Data.Length, this.Data.Length);
            return result;
        }

        private int GetDataLength()
        {
            int length = this.Data.Length;
            length += 1; //NetsyMessageKind
            switch (this.Kind)
            {
                case NetsyMessageKind.SendPackage:
                    break;
                case NetsyMessageKind.Request:
                    length += 16; //RequestId
                    break;
                case NetsyMessageKind.Response:
                    length += 16; //RequestId
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return length;
        }
    }
}