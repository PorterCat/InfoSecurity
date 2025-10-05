namespace InfoSecurity.ElGamal;

public class ElGamalDecoder
{
    private readonly long _p;
    private readonly long _g;
    private readonly long _privateKey;
    private readonly long _publicKey;
    public ElGamalDecoder(long p, long g, long cB)
    {
        _p = p;
        _g = g;
        _privateKey = cB;
        _publicKey = MyAlgorithms.ModPow(g, cB, p);
    }
    public void EncryptFile(string inputPath, string outputPath)
    {
        using var inputStream = File.OpenRead(inputPath);
        using var outputStream = File.Create(outputPath);
        using var reader = new BinaryReader(inputStream);
        using var writer = new BinaryWriter(outputStream);

        var encryptor = new ElGamalUser(_p, _g, _privateKey);

        while (inputStream.Position < inputStream.Length)
        {
            byte originalByte = reader.ReadByte();
            var encrypted = encryptor.EncryptBlock(originalByte, _publicKey);
            writer.Write(encrypted.c1);
            writer.Write(encrypted.c2);
        }
    }

    public void DecryptFile(string inputPath, string outputPath)
    {
        using var inputStream = File.OpenRead(inputPath);
        using var outputStream = File.Create(outputPath);
        using var reader = new BinaryReader(inputStream);
        using var writer = new BinaryWriter(outputStream);

        var decryptor = new ElGamalUser(_p, _g, _privateKey);

        while (inputStream.Position < inputStream.Length)
        {
            long c1 = reader.ReadInt64();
            long c2 = reader.ReadInt64();
            long decrypted = decryptor.DecryptBlock(c1, c2);
            writer.Write((byte)decrypted);
        }
    }
}