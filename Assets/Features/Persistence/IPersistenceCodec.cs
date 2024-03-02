public interface IPersistenceCodec
{
    byte[] Write(PersistenceModel model);
    void Read(byte[] data, PersistenceModel model);
}
