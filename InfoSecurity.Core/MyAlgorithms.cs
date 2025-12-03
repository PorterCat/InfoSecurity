using System.Net.Sockets;
using System.Numerics;
using Open.Numeric.Primes.Extensions;

namespace InfoSecurity;

public static class MyAlgorithms
{
    public static long? ShanksMethod(long a, long p, long y)
    {
        if (a >= p || y >= p)
            return null;

        long m = (long)Math.Sqrt(p) + 1;
        
        var babySteps = new Dictionary<long, long>();
        long current = y;
        for (long j = 0; j < m; j++)
        {
            babySteps.TryAdd(current, j);
            current = MultiplyMod(current, a, p);
        }
        
        var giantSteps = new Dictionary<long, long>();
        current = 1;
        
        long baseM = ModPow(a, m, p);
        
        for (long i = 1; i <= m; i++)
        {
            giantSteps.TryAdd(current, i);
            current = MultiplyMod(current, baseM, p);
        }
        
        foreach (var kv in giantSteps)
        {
            long value = kv.Key;
            long i = kv.Value;

            if (babySteps.TryGetValue(value, out long j))
            {
                long x = i * m - j;

                if (ModPow(a, x, p) == y)
                    return x;
            }
        }

        return null;
    }
    
    public static BigInteger ShanksMethod(BigInteger @base, BigInteger result, BigInteger mod, long limit)
    {
        var m = (long)Math.Ceiling(Math.Sqrt(limit));

        var storage = new Dictionary<BigInteger, long>();

        for (var j = 0; j < m; j++)
        {
            var value = BigInteger.ModPow(@base, j, mod);
            storage.TryAdd(value, j);
        }

        var baseInverse = BigInteger.ModPow(@base.ModInverse(mod), m, mod);
        for (var i = 0; i < m; i++)
        {
            var value = BigInteger.Remainder(result * BigInteger.ModPow(baseInverse, i, mod), mod);
            if (storage.TryGetValue(value, out var j))
            {
                return new BigInteger(i * m + j);
            }
        }

        throw new Exception("i and j not found");
    }
    
    public static long ModPow(long a, long x, long p)
    {
        long result = 1;
        a %= p;
    
        while (x > 0)
        {
            if ((x & 1) == 1)
                result = (result * a) % p;
        
            a = (a * a) % p;
            x >>= 1;
        }
    
        return result;
    }
    
    public static long SafeModPow(long a, long x, long p)
    {
        BigInteger bigA = a;
        BigInteger bigX = x;
        BigInteger bigP = p;
        BigInteger result = BigInteger.ModPow(bigA, bigX, bigP);
        return (long)result;
    }

    public static long MultiplyMod(long a, long b, long mod) =>
         (long)(((ulong)a * (ulong)b) % (ulong)mod);
    
    public static long FindPrimitiveRoot(long p)
    {
        long q = ((p - 1) / 2);
        if (!(p.IsPrime() && q.IsPrime()))
            return -1;

        if (p == 2) return 1;
        if (p == 3) return 2;
        
        for (long g = 2; g < p; g++)
        {
            if (ModPow(g, q, p) != 1)
                return g;
        }
        
        return -1;
    }
    
    public static long Gcd(long a, long b)
    {
        while (a != 0 && b != 0)
        {
            if (a > b)
                a %= b;
            else
                b %= a;
        }

        return a | b;
    }

    public static (long gcd, long a, long b) ExtGcd(long a, long b)
    {
        long oldR = a, r = b;
        long oldS = 1, s = 0;
        long oldT = 0, t = 1;

        while (r != 0)
        {
            long q = oldR / r;

            long tmp = oldR - q * r;
            oldR = r; r = tmp;

            tmp = oldS - q * s;
            oldS = s; s = tmp;

            tmp = oldT - q * t;
            oldT = t; t = tmp;
        }
        
        return (oldR, oldS, oldT);
    }
    
    public static long ModInverse(long c, long p)
    {
        var (gcd, aCoeff, _) = ExtGcd(c, p);
        
        if (gcd != 1)
            throw new ArgumentException($"Inverse doesn't exist: gcd({c}, {p}) = {gcd}, expected 1");
        
        long inverse = aCoeff % p;
        if (inverse < 0)
            inverse += p;
    
        return inverse;
    }
}