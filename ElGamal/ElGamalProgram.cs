using InfoSecurity;
using InfoSecurity.ElGamal;

public static class ElGamalProgram
{
    public static void Execute(string[] args)
    {
        if (args.Length != 6)
        {
            SimpleExample();
            return;
        }
        
        string inputPath = args[0];
        string mode = args[1];
        long p = long.Parse(args[2]);
        long g = long.Parse(args[3]);
        long cB = long.Parse(args[4]);
        string outputPath = args[5];

        var decoder = new ElGamalDecoder(p, g, cB);
        try
        {
            if (mode == "-encrypt")
            {
                decoder.EncryptFile(inputPath, outputPath);
                Console.WriteLine($"File encrypted successfully. Output: {outputPath}");
            }
            else if (mode == "-decrypt")
            {
                decoder.DecryptFile(inputPath, outputPath);
                Console.WriteLine($"File decrypted successfully. Output: {outputPath}");
            }
            else
                Console.WriteLine("Invalid mode. Use -encrypt or -decrypt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    public static void SimpleExample()
    {
        long p = 257;
        long g = MyAlgorithms.FindPrimitiveRoot(p);
        
        var alice = new ElGamalUser(p, g);
        var bob = new ElGamalUser(p, g);
        
        string origMessage = "Hello!";
        Console.WriteLine($"\nOriginal message: {origMessage}");
        
        List<(long c1, long c2)> encryptedByAlice = [];
        foreach (char c in origMessage)
        {
            var encrypted = alice.EncryptBlock(c, bob.PublicKey);
            encryptedByAlice.Add(encrypted);
            //Console.WriteLine($"'{c}' ({(int)c}) -> ({encrypted.c1}, {encrypted.c2})");
        }
        
        var decryptedByBob = new List<long>();
        foreach (var (c1, c2) in encryptedByAlice)
        {
            long decrypted = bob.DecryptBlock(c1, c2);
            decryptedByBob.Add(decrypted);
            //Console.WriteLine($"({c1}, {c2}) -> {decrypted} ('{(char)decrypted}')");
        }
        
        char[] finalMessage = decryptedByBob.Select(c => (char)c).ToArray();
        Console.WriteLine($"\nFinal message: {new string(finalMessage)}");   
    }
}

public class ElGamalUser
{
    private readonly Random _random = new();
    private readonly long _p;
    
    private readonly long _privateKey;
    public long PublicKey { get; }
    public long G { get; } 

    public ElGamalUser(long p, long g)
    {
        _p = p;
        _privateKey = _random.NextInt64(2, _p - 1);
        
        G = g;
        PublicKey = MyAlgorithms.ModPow(G, _privateKey, _p);
    }

    public ElGamalUser(long p, long g, long privateKey)
    {
        _p = p;
        _privateKey = privateKey;
        
        G = g;
        PublicKey = MyAlgorithms.ModPow(G, _privateKey, _p);
    }

    public (long c1, long c2) EncryptBlock(long message, long recipientPublicKey)
    {
        long k;
        do
        {
            k = _random.NextInt64(2, _p - 1);
        } while (MyAlgorithms.Gcd(k, _p - 1) != 1);
        
        long c1 = MyAlgorithms.ModPow(G, k, _p);
        long c2 = (message * MyAlgorithms.ModPow(recipientPublicKey, k, _p)) % _p;
        return (c1, c2);
    }
    
    public long DecryptBlock(long c1, long c2)
    {
        long s = MyAlgorithms.ModPow(c1, _privateKey, _p);
        long sInverse = MyAlgorithms.ModInverse(s, _p);
        return (c2 * sInverse) % _p;
    }

}