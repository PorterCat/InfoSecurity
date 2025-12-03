// foreach(var i in Enumerable.Range(1, 100))
//     SimpleExample();
//
// return 0;

using InfoSecurity;

string inputPath = args[0];
string mode = args[1];
long p = long.Parse(args[2]);
long q = long.Parse(args[3]);
long d = long.Parse(args[4]);
string outputPath = args[5];

var rsaDecoder = new RsaDecoder(p, q, d);
try
{
    if (mode == "-encrypt")
        rsaDecoder.EncryptFile(inputPath, outputPath);
    else if (mode == "-decrypt")
        rsaDecoder.DecryptFile(inputPath, outputPath);
    else
        throw new Exception("Unknown mode: " + mode);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
return 0;

void SimpleExample()
{
    long p = MillerRabinPrimeGenerator.GeneratePrime(100_000, 100_000_000_000); 
    long q = MillerRabinPrimeGenerator.GeneratePrime(100_000, 100_000_000_000);
    
    var alice = new RsaUser(p, q);
    var bob = new RsaUser(
        MillerRabinPrimeGenerator.GeneratePrime(100_000, 100_000_000_000), 
        MillerRabinPrimeGenerator.GeneratePrime(100_000, 100_000_000_000));
    
    string origMessage = "Hello World!";
    Console.WriteLine($"Original message: {origMessage}");
    
    var encrypted = origMessage.Select(c => RsaUser.Encrypt(c, bob.GetPublicKey())).ToArray();
    Console.WriteLine("Encrypted values: " + string.Join(", ", encrypted));
    
    var decrypted = encrypted.Select(c => bob.Decrypt(c)).ToArray();
    char[] finalMessage = decrypted.Select(c => (char)c).ToArray();
    Console.WriteLine($"Final message: {new string(finalMessage)}");
}