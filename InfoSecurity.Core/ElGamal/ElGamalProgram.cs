using InfoSecurity;
using InfoSecurity.ElGamal;
using Open.Numeric.Primes;

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
        long p;
        long g;

        do
        {
            p = Prime.Numbers.Next(new Random().NextInt64(1000, 10000));
            g = MyAlgorithms.FindPrimitiveRoot(p);
        } while (g == -1);
        
        Console.WriteLine($"p = {p}, g = {g}");
        
        var alice = new ElGamalUser(p, g);
        var bob = new ElGamalUser(p, g);

        long m = 123;
        var ab = alice.EncryptBlock(m, bob.PublicKey);
        long m2 = bob.DecryptBlock(ab.a, ab.b);
        
        Console.WriteLine($"m = {m}, m2 = {m2}");
        
        // string origMessage = "12345helloworld54321";
        // Console.WriteLine($"Original message: {origMessage}");
        //
        // List<(long c1, long c2)> encryptedByAlice = [];
        // foreach (char c in origMessage)
        // {
        //     var encrypted = alice.EncryptBlock(c, bob.PublicKey);
        //     encryptedByAlice.Add(encrypted);
        // }
        //
        // var decryptedByBob = new List<long>();
        // foreach (var (c1, c2) in encryptedByAlice)
        // {
        //     long decrypted = bob.DecryptBlock(c1, c2);
        //     decryptedByBob.Add(decrypted);
        // }
        
        // char[] finalMessage = decryptedByBob.Select(c => (char)c).ToArray();
        // Console.WriteLine($"\nFinal message: {new string(finalMessage)}");   
    }
}

public class ElGamalUser
{
    private readonly Random _random = new();
    private readonly long _p;
    
    private readonly long _x;
    public long PublicKey { get; }
    public long G { get; } 

    public ElGamalUser(long p, long g)
    {
        _p = p;
        _x = _random.NextInt64(2, _p - 1);
        
        G = g;
        PublicKey = MyAlgorithms.ModPow(G, _x, _p);
    }

    public ElGamalUser(long p, long g, long x)
    {
        _p = p;
        _x = x;
        
        G = g;
        PublicKey = MyAlgorithms.ModPow(G, _x, _p);
    }

    public (long a, long b) EncryptBlock(long m, long y)
    {
        long k = _random.NextInt64(2, _p - 1);
        
        long a = MyAlgorithms.ModPow(G, k, _p);
        long b = (m * MyAlgorithms.ModPow(y, k, _p)) % _p;
        return (a, b);
    }
    
    public long DecryptBlock(long a, long b)
    {
        // long s = MyAlgorithms.ModPow(a, _privateKey, _p);
        // long sInverse = MyAlgorithms.ModInverse(s, _p);
        // return (b * sInverse) % _p;
        return (b * MyAlgorithms.ModPow(a, _p - 1 - _x, _p)) % _p;
    }
}