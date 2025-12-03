using InfoSecurity;

public class RsaUser
{
    private static readonly Random Random = new();
    
    public long N { get; }
    public long E { get; }
    protected long D { get; }

    public RsaUser(long d)
    {
        long p, q, phi;
        do
        {
            p = MillerRabinPrimeGenerator.GeneratePrime(10_000, 100_000);
            q = MillerRabinPrimeGenerator.GeneratePrime(10_000, 100_000);
            phi = (p - 1) * (q - 1);
        } while (MyAlgorithms.Gcd(d, phi) != 1);
        
        N = p * q;
        D = d;
        E = MyAlgorithms.ModInverse(D, phi);
        
        if (MyAlgorithms.MultiplyMod(E, D, phi) != 1)
            throw new InvalidOperationException("Invalid key pair generated");
    }
    
    public RsaUser(long p, long q, long? d = null)
    {
        if (!p.IsPrime() || !q.IsPrime())
            throw new ArgumentException("P and Q must be prime numbers");
        
        N = p * q;
        long phi = (p - 1) * (q - 1);

        if (d.HasValue)
        {
            D = d.Value;
            E = MyAlgorithms.ModInverse(D, phi);
        }
        else
        {
            do
            {
                E = Random.NextInt64(2, phi - 1);
            } while (MyAlgorithms.Gcd(E, phi) != 1);
            
            D = MyAlgorithms.ModInverse(E, phi);
        }
        
        if (MyAlgorithms.MultiplyMod(E, D, phi) != 1)
            throw new InvalidOperationException("Invalid key pair generated");
    }
    
    public (long E, long N) GetPublicKey() => (E, N);

    public static long Encrypt(long message, (long E, long N) pubKey)
        => (message < pubKey.N) 
            ? MyAlgorithms.ModPow(message, pubKey.E, pubKey.N) 
            : throw new ArgumentException($"Message ({message}) must be less than N ({pubKey.N})");

    public long Decrypt(long encrypted)
    {
        long result = MyAlgorithms.ModPow(encrypted, D, N);
        
        if (result < 0)
            result += N;
    
        return result;
    }
}