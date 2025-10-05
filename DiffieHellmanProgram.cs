using System.Diagnostics;
using System.Numerics;
using InfoSecurity;

// Схема Диффи-Хеллмана

public static class DiffieHellmanProgram
{
    private static long _p, _g, _xa, _xb;
    
    public static void Execute(string[] args)
    {
        if (args.Length >= 4)
        {
            if (!long.TryParse(args[0], out _p) || !_p.IsPrime())
            {
                Console.WriteLine("Error: p must be prime");
                return;
            }
    
            if (!long.TryParse(args[1], out _g))
            {
                return;
            }
    
            if (!long.TryParse(args[2], out _xa) || _xa <= 1 || _xa >= _p-1)
            {
                Console.WriteLine($"Error: Xa must be in [1, {_p-1}]");
                return;
            }
    
            if (!long.TryParse(args[3], out _xb) || _xb <= 1 || _xb >= _p-1)
            {
                Console.WriteLine($"Error: Xb must be in [1, {_p-1}]");
                return;
            }
        }
        else
        {
            _p = 23;
            //_p = FermatPrimeGenerator.GeneratePrime(1_000_000_000, 2_000_000_000);
            //_p = Prime.Numbers.StartingAt(3_123_123_123_123).First();
            //_p = Prime.Numbers.StartingAt(40_000_000_000_000).First();
            _g = MyAlgorithms.FindPrimitiveRoot(_p);   
            _xa = new Random().NextInt64(1, _p - 1);
            _xb = new Random().NextInt64(1, _p - 1);
        }
        Console.WriteLine($"g: {_g}");
        var alice = new DiffieHellmanUser(_p, _g, _xa);
        var bob = new DiffieHellmanUser(_p, _g, _xb);

        var aliceSharedSecret = alice.ComputeSharedSecret(bob.PublicKey);
        var bobSharedSecret = bob.ComputeSharedSecret(alice.PublicKey);

        Console.WriteLine($"Alice's shared secret: {aliceSharedSecret}");
        Console.WriteLine($"Bob's shared secret: {bobSharedSecret}");
        AttemptToCrack();
        return;

        void AttemptToCrack()
        {
            Console.WriteLine($"Shanks requires ~{(long)Math.Sqrt(_p)} ops");
    
            var gB = new BigInteger(_g);
            var pB = new BigInteger(_p);
            var pkA = new BigInteger(alice.PublicKey);
            var pkB = new BigInteger(bob.PublicKey);

            var time = new Stopwatch().Measure(() =>
            {
                var alicePrivateKey = MyAlgorithms.ShanksMethod(gB, pkA, pB, _p);
                // var bobPrivateKey = MyAlgorythms.ShanksMethod(gB, pkB, pB, p);
    
                Console.WriteLine($"Eve found Alice private: {alicePrivateKey}");
                //Console.WriteLine($"Eve found Bob private: {bobPrivateKey}");
        
                long eveSharedSecret = MyAlgorithms.ModPow(bob.PublicKey, (long)alicePrivateKey, _p);
                Console.WriteLine($"Eve calculated a secret: {eveSharedSecret}");
            }).Elapsed;
            
            Console.WriteLine($"Eve works for {(time.TotalMilliseconds / 1000):F5} seconds");
        }
    }
}

public readonly struct DiffieHellmanUser
{
    private readonly long _p;
    private readonly long _privateKey;
    public long PublicKey { get; }

    public DiffieHellmanUser(long p, long g, long privateKey)
    {
        (_p, _privateKey) = (p, privateKey);
        PublicKey = MyAlgorithms.ModPow(g, _privateKey, p);
    }   
    
    public long ComputeSharedSecret(long otherPublicKey) =>
        MyAlgorithms.ModPow(otherPublicKey, (long?)_privateKey, _p);
}