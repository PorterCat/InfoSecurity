using System.Numerics;
using System.Security.Cryptography;

namespace InfoSecurity;

public static class MillerRabinPrimeGenerator
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    
    public static long GeneratePrime(long min, long max, int iterations = 10)
    {
        while (true)
        {
            long candidate = RandomLong(min, max);
            
            if (candidate % 2 == 0 && candidate != 2)
                candidate++;
            
            if (MillerRabinTest(new BigInteger(candidate), iterations))
                return candidate;
        }
    }
    
    private static long RandomLong(long min, long max)
    {
        byte[] buf = new byte[8];
        Rng.GetBytes(buf);
        long longRand = BitConverter.ToInt64(buf, 0);
        
        long range = max - min;
        if (range <= 0) 
            throw new ArgumentException("Max must be greater than min");
            
        return Math.Abs(longRand % range) + min;
    }
    
    public static bool MillerRabinTest(BigInteger n, int k)
    {
        if (n == 2 || n == 3)
            return true;
        if (n < 2 || n % 2 == 0)
            return false;
        
        BigInteger d = n - 1;
        int s = 0;
        while (d % 2 == 0)
        {
            d /= 2;
            s += 1;
        }

        byte[] bytes = n.ToByteArray();
        
        for (int i = 0; i < k; i++)
        {
            BigInteger a;
            do
            {
                byte[] aBytes = new byte[bytes.Length + 1];
                Rng.GetBytes(aBytes);
                a = new BigInteger(aBytes);
            }
            while (a < 2 || a >= n - 2);
            
            BigInteger x = BigInteger.ModPow(a, d, n);
            
            if (x == 1 || x == n - 1)
                continue;
            
            bool continueOuter = false;
            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, n);
                if (x == 1)
                    return false;
                if (x == n - 1)
                {
                    continueOuter = true;
                    break;
                }
            }
            
            if (!continueOuter)
                return false;
        }
        
        return true;
    }
    
    public static long GenerateLargePrime(int bitSize = 1024)
    {
        if (bitSize < 8) 
            throw new ArgumentException("Bit size must be at least 8");
        
        long min = (long)Math.Pow(2, bitSize - 1);
        long max = bitSize < 64 ? (long)Math.Pow(2, bitSize) - 1 : long.MaxValue;
        
        return GeneratePrime(min, max, 20);
    }
    
    public static (long p, long q) GenerateDistinctPrimes(long min, long max, int iterations = 10)
    {
        long p = GeneratePrime(min, max, iterations);
        long q;
        
        do
        {
            q = GeneratePrime(min, max, iterations);
        } while (q == p);
        
        return (p, q);
    }
}