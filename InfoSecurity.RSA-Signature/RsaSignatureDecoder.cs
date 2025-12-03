using System.Security.Cryptography;
using System.Text.Json;

namespace InfoSecurity.RsaSignature;

public class SignatureData
{
    public long E { get; init; }
    public long N { get; init; }
    public long[] Signature { get; init; } = [];
}

public class RsaSignatureDecoder : RsaUser
{
    public RsaSignatureDecoder(long p, long q, long d) : base(p, q, d) { }

    public RsaSignatureDecoder(long d) : base(d) { }
    
    public void SignFile(string inputPath, string signaturePath)
    {
        byte[] fileHash = ComputeFileHash(inputPath);
        
        long[] signature = new long[fileHash.Length];
        for (int i = 0; i < fileHash.Length; i++)
            signature[i] = Decrypt(fileHash[i]);
        
        var publicKey = GetPublicKey();
        var signatureData = new SignatureData
        {
            E = publicKey.E,
            N = publicKey.N,
            Signature = signature
        };
        
        string json = JsonSerializer.Serialize(signatureData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        File.WriteAllText(signaturePath, json);
    }
    
    public static bool VerifySignature(string inputPath, string signaturePath)
    {
        byte[] fileHash = ComputeFileHash(inputPath);
        string json = File.ReadAllText(signaturePath);
        var signatureData = JsonSerializer.Deserialize<SignatureData>(json);
        
        if (signatureData is null) throw new ArgumentNullException($"Wrong signature");
        
        var signature = signatureData.Signature;
        for (int i = 0; i < signature.Length; i++)
        {
            long verified = Encrypt(signature[i], (signatureData.E, signatureData.N));
            if (verified != fileHash[i])
                return false;
        }

        return true;
    }
    
    private static byte[] ComputeFileHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var fileStream = File.OpenRead(filePath);
        return sha256.ComputeHash(fileStream);
    }
}