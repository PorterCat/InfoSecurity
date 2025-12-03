namespace InfoSecurity;

public class ShamirSSDecoder(long p, long ca, long cb)
{
    public void EncryptFile(string inputPath, string outputPath)
    {
        using var inputStream = File.OpenRead(inputPath);
        using var outputStream = File.Create(outputPath);
        using var reader = new BinaryReader(inputStream);
        using var writer = new BinaryWriter(outputStream);

        var alice = new ShamirUser(p, ca);
        var bob = new ShamirUser(p, cb);

        while (inputStream.Position < inputStream.Length)
        {
            long originalByte = reader.ReadByte();
            long encryptedByAlice = alice.EncryptBlock(originalByte);
            long encryptedByBob = bob.EncryptBlock(encryptedByAlice);
            
            writer.Write(encryptedByBob);
        }
    }

    public void DecryptFile(string inputPath, string outputPath)
    {
        using var inputStream = File.OpenRead(inputPath);
        using var outputStream = File.Create(outputPath);
        using var reader = new BinaryReader(inputStream);
        using var writer = new BinaryWriter(outputStream);

        var alice = new ShamirUser(p, ca);
        var bob = new ShamirUser(p, cb);

        while (inputStream.Position < inputStream.Length)
        {
            long encryptedValue = reader.ReadInt64();
            long decryptedByAlice = alice.DecryptBlock(encryptedValue);
            long decryptedByBob = bob.DecryptBlock(decryptedByAlice);
            
            writer.Write((byte)decryptedByBob);
        }
    }
}