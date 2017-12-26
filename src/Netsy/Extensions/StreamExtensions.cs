using System;
using System.IO;
using System.Threading.Tasks;

namespace Netsy.Extensions
{
    public static class StreamExtensions
    {
        public static async Task WriteRawMessageAsync(this Stream self, byte[] data)
        {
            var length = BitConverter.GetBytes(data.Length);
            await self.WriteAsync(length, 0, length.Length);
            await self.WriteAsync(data, 0, data.Length);
        }

        public static async Task<byte[]> ReadRawMessageAsync(this Stream self)
        {
            var lengthAsBytes = await self.ReadDataAsync(4);

            if (lengthAsBytes == null)
                return null;

            var length = BitConverter.ToInt32(lengthAsBytes, 0);
            return await self.ReadDataAsync(length);
        }

        public static async Task<byte[]> ReadDataAsync(this Stream self, int length)
        {
            var result = new byte[length];

            int read = 0;

            while (true)
            {
                read += await self.ReadAsync(result, read, length - read);

                if (read == 0)
                    break;

                if (read == length)
                    break;
            }

            if (read == 0)
                return null;

            return result;
        }
    }
}