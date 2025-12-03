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
        writer.Write(inputStream.Length);

        byte[] buffer = new byte[8];
        while (inputStream.Position < inputStream.Length)
        {
            int bytesRead = reader.Read(buffer, 0, 8);
            
            if (bytesRead < 8)
                Array.Clear(buffer, bytesRead, 8 - bytesRead);

            long originalLongBlock = BitConverter.ToInt64(buffer, 0);
            var encrypted = encryptor.EncryptBlock(originalLongBlock, _publicKey);
            writer.Write(encrypted.a);
            writer.Write(encrypted.b);
        }
    }
    
    public void DecryptFile(string inputPath, string outputPath)
    {
        using var inputStream = File.OpenRead(inputPath);
        using var outputStream = File.Create(outputPath);
        using var reader = new BinaryReader(inputStream);
        using var writer = new BinaryWriter(outputStream);

        var decryptor = new ElGamalUser(_p, _g, _privateKey);
        
        long originalFileLength = reader.ReadInt64();

        while (inputStream.Position < inputStream.Length)
        {
            long c1 = reader.ReadInt64();
            long c2 = reader.ReadInt64();
            long decrypted = decryptor.DecryptBlock(c1, c2);
            byte[] decryptedBytes = BitConverter.GetBytes(decrypted);
            
            long remainingBytes = originalFileLength - outputStream.Position;
            if (remainingBytes < 8)
                writer.Write(decryptedBytes, 0, (int)remainingBytes);
            else
                writer.Write(decryptedBytes);
        }
    }
}