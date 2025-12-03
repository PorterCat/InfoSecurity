using InfoSecurity;

public static class FermatPrimeGenerator
{
    public static long GeneratePrime(long min, long max, int iterations = 10)
    {
        while (true)
        {
            long candidate = RandomLong(min, max);
            if (IsProbablePrime(candidate, iterations))
                return candidate;
        }
    }
    
    private static long RandomLong(long min, long max)
    {
        byte[] buf = new byte[8];
        new Random().NextBytes(buf);
        long longRand = BitConverter.ToInt64(buf, 0);
        return Math.Abs(longRand % (max - min)) + min;
    }
    
    private static bool IsProbablePrime(long n, int iterations)
    {
        if (n < 2) return false;
        if (n == 2 || n == 3) return true;
        if (n % 2 == 0) return false;
        
        for (int i = 0; i < iterations; i++)
        {
            long a = RandomLong(2, n - 1);
            if (MyAlgorithms.ModPow(a, n - 1, n) != 1)
                return false;
        }
        return true;
    }
}