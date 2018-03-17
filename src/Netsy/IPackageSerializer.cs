namespace Netsy
{
    public interface IPackageSerializer
    {
        byte[] Serialize(object package);
        object Deserialize(byte[] data);
    }
}