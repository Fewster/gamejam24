public interface IPersistenceCodec
{
    byte[] Write(PersistentModel model);
    void Read(byte[] data, PersistentModel model);
}
