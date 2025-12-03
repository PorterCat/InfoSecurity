using InfoSecurity.RsaSignature;

string inputPath = args[0];
string mode = args[1];

// test.txt -sign 7919 7867 58123013 test.sig

if (mode == "-sign")
{
    if (args.Length == 6)
    {
        long p = long.Parse(args[2]);
        long q = long.Parse(args[3]);
        long d = long.Parse(args[4]);
        string signaturePath = args[5];
            
        var rsaSignature = new RsaSignatureDecoder(p, q, d);
        rsaSignature.SignFile(inputPath, signaturePath);
            
        Console.WriteLine($"File signed successfully: {signaturePath}");
        var publicKey = rsaSignature.GetPublicKey();
        Console.WriteLine($"Public key (E, N): ({publicKey.E}, {publicKey.N})");
    }
    else if (args.Length == 4)
    {
        long d = long.Parse(args[2]);
        string signaturePath = args[3];
            
        var rsaSignature = new RsaSignatureDecoder(d);
        rsaSignature.SignFile(inputPath, signaturePath);
            
        Console.WriteLine($"File signed successfully: {signaturePath}");
        var publicKey = rsaSignature.GetPublicKey();
        Console.WriteLine($"Public key (E, N): ({publicKey.E}, {publicKey.N})");
    }
    else
    {
        throw new Exception("Invalid arguments for -sign mode");
    }
}
else if (mode == "-verify")
{
    string signaturePath = args[2];
    bool isValid = RsaSignatureDecoder.VerifySignature(inputPath, signaturePath);
        
    Console.WriteLine(isValid ? "Signature is VALID" : "Signature is INVALID");
}
else
{
    throw new Exception("Unknown mode: " + mode);
}