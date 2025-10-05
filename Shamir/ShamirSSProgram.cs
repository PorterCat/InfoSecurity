using InfoSecurity;

public static class ShamirSSProgram
{
    private static readonly Random Random = new();
    
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
        long cA = long.Parse(args[3]);
        long cB = long.Parse(args[4]);
        string outputPath = args[5];

        var decoder = new ShamirSSDecoder(p, cA, cB);

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
            {
                Console.WriteLine("Invalid mode. Use -encrypt or -decrypt.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void SimpleExample()
    {
        long p = 23; // Чтобы шифровать весь диапазон Char, P >= 257!!! 
        var alice = new ShamirUser(p, 15);
        var bob = new ShamirUser(p, 7);

        string origMessage = "Hello!";
        Console.WriteLine($"Original message: {origMessage}");
        
        var encryptedByAlice = origMessage.Select(c => alice.EncryptBlock(c)).ToArray();
        var encryptedByBob = encryptedByAlice.Select(c => bob.EncryptBlock(c)).ToArray();
        
        var decryptedByAlice = encryptedByBob.Select(c => alice.DecryptBlock(c)).ToArray();
        var decryptedByBob = decryptedByAlice.Select(c => bob.DecryptBlock(c)).ToArray();
        
        char[] finalMessage = decryptedByBob.Select(c => (char)c).ToArray();
        Console.WriteLine($"Final message: {new string(finalMessage)}");
    }
}

public class ShamirUser
{
    private readonly Random _random = new();
    private readonly long _p;
    private long _c { get; }
    private long _d { get; }    
    
    public ShamirUser(long p)
    {
        _p = p;
        do
        {
            _c = _random.NextInt64(2, p - 1);
        } while (MyAlgorithms.Gcd(_c, p - 1) != 1);
        
        _d = MyAlgorithms.ModInverse(_c, p - 1);
        
        var cond = (_c * _d) % (p - 1) == 1;
        Console.WriteLine($"cA[{_c}] * dA[{_d}] (mod {p - 1}) == 1? ({cond})");
        
        if (!cond)
            throw new InvalidOperationException($"Invalid key");
    }

    public ShamirUser(long p, long c)
    {
        _p = p;
        _c = c;
        if(MyAlgorithms.Gcd(_c, p - 1) != 1)
            throw new InvalidOperationException($"Invalid C key");
        
        _d = MyAlgorithms.ModInverse(_c, p - 1);
        
        var cond = (_c * _d) % (p - 1) == 1;
        Console.WriteLine($"cA[{_c}] * dA[{_d}] (mod {p - 1}) == 1? ({cond})");
        
        if (!cond)
            throw new InvalidOperationException($"Invalid key");
    }
    
    public long EncryptBlock(long block) =>
        MyAlgorithms.ModPow(block, _c, _p);
    
    public long DecryptBlock(long block) =>
        MyAlgorithms.ModPow(block, _d, _p);
}