using System.Text.Json;

namespace InfoSecurity.VernamSipher;

public class DiffieHellmanKeyGenerator
{
    public long P { get; }
    public long G { get; }
    public long PublicKey { get; }

    public DiffieHellmanKeyGenerator(long p, long g, long privateKey)
    {
        P = p;
        G = g;
        PublicKey = MyAlgorithms.ModPow(G, privateKey, P);
    }

    public DiffieHellmanKeyGenerator(long privateKey)
    {
        do
        {
            P = MillerRabinPrimeGenerator.GeneratePrime(100_000, 100_000_000);
            G = MyAlgorithms.FindPrimitiveRoot(P);
        } while (G == -1);

        PublicKey = MyAlgorithms.ModPow(G, privateKey, P);
    }

    public long CalculateSharedSecret(long partnerPublicKey, long myPrivateKey) 
        => MyAlgorithms.ModPow(partnerPublicKey, myPrivateKey, P);

    public void SaveToFile(string filePath)
    {
        var keyInfo = new { P, G, PublicKey };
        string json = JsonSerializer.Serialize(keyInfo, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }

    public static (long p, long g, long publicKey) LoadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
            
        long p = root.GetProperty(nameof(P)).GetInt64();
        long g = root.GetProperty(nameof(G)).GetInt64();
        long publicKey = root.GetProperty(nameof(PublicKey)).GetInt64();
            
        return (p, g, publicKey);
    }
}