public class RsaDecoder(long p, long q, long d)
{
    protected readonly RsaUser User = new(p, q, d);
    
    public void EncryptFile(string inputPath, string outputPath)
    {
        byte[] fileData = File.ReadAllBytes(inputPath);
        
        using var outputStream = File.Create(outputPath);
        using var writer = new BinaryWriter(outputStream);
        
        writer.Write(fileData.Length);
        
        int blockSize = CalculateSafeBlockSize(User.N);
        
        for (int i = 0; i < fileData.Length; i += blockSize)
        {
            int bytesToRead = Math.Min(blockSize, fileData.Length - i);
            byte[] block = new byte[bytesToRead];
            Array.Copy(fileData, i, block, 0, bytesToRead);
            
            long number = BytesToLong(block);
            
            if (number >= User.N)
            {
                throw new InvalidOperationException(
                    $"Block value {number} exceeds N={User.N}. " +
                    $"Use larger primes or smaller block size.");
            }
            
            long encrypted = RsaUser.Encrypt(number, User.GetPublicKey());
            writer.Write(encrypted);
        }
    }
    
    public void DecryptFile(string inputPath, string outputPath)
    {
        byte[] encryptedData = File.ReadAllBytes(inputPath);
        
        using var inputStream = new MemoryStream(encryptedData);
        using var reader = new BinaryReader(inputStream);
        using var outputStream = File.Create(outputPath);
        
        int originalLength = reader.ReadInt32();
        
        int blockSize = CalculateSafeBlockSize(User.N);
        int encryptedBlockSize = sizeof(long);
        
        int remainingData = encryptedData.Length - sizeof(int);
        int blockCount = remainingData / encryptedBlockSize;
        
        var decryptedData = new List<byte>();
        
        for (int i = 0; i < blockCount && decryptedData.Count < originalLength; i++)
        {
            long encrypted = reader.ReadInt64();
            long decrypted = User.Decrypt(encrypted);
            
            byte[] blockBytes = LongToBytes(decrypted);
            
            int bytesNeeded = Math.Min(blockSize, originalLength - decryptedData.Count);
            byte[] actualBytes = new byte[bytesNeeded];
            Array.Copy(blockBytes, 0, actualBytes, 0, bytesNeeded);
            
            decryptedData.AddRange(actualBytes);
        }
        
        outputStream.Write(decryptedData.ToArray(), 0, decryptedData.Count);
    }
    
    private int CalculateSafeBlockSize(long n)
    {
        long maxValue = 1;
        int blockSize = 0;
        
        while (maxValue < n && blockSize < 8)
        {
            blockSize++;
            maxValue <<= 8;
            if (maxValue > n || maxValue < 0)
                break;
        }
        return Math.Max(1, blockSize - 1);
    }
    
    private long BytesToLong(byte[] bytes)
    {
        long result = 0;
        for (int i = 0; i < bytes.Length && i < 8; i++)
        {
            result |= ((long)bytes[i]) << (8 * i);
        }
        return result;
    }
    
    private byte[] LongToBytes(long value)
    {
        byte[] result = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            result[i] = (byte)((value >> (8 * i)) & 0xFF);
        }
        return result;
    }
}