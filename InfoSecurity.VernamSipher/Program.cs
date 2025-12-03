using InfoSecurity.VernamSipher;

// program -genkey p g privateKey [output.json]
// program -encrypt inputFile partnerPublicKey.json myPrivateKey [outputFile]
// program -decrypt inputFile partnerPublicKey.json myPrivateKey [outputFile]

string mode = args[0];
switch (mode)
{
    case "-genkey":
        HandleKeyGeneration(args);
        break;

    case "-encrypt":
    case "-decrypt":
        HandleEncryptionDecryption(args, mode);
        break;

    default:
        return;
}

static void HandleKeyGeneration(string[] args)
{
    DiffieHellmanKeyGenerator dh;
    string outputFile;

    if (args.Length == 2)
    {
        long privateKey = long.Parse(args[1]);
        dh = new DiffieHellmanKeyGenerator(privateKey);
        outputFile = "public_key.json";
    }
    else if (args.Length == 3)
    {
        long privateKey = long.Parse(args[1]);
        dh = new DiffieHellmanKeyGenerator(privateKey);
        outputFile = args[2];
    }
    else if (args.Length == 4)
    {
        long p = long.Parse(args[1]);
        long g = long.Parse(args[2]);
        long privateKey = long.Parse(args[3]);
        dh = new DiffieHellmanKeyGenerator(p, g, privateKey);
        outputFile = "public_key.json";
    }
    else if (args.Length == 5)
    {
        long p = long.Parse(args[1]);
        long g = long.Parse(args[2]);
        long privateKey = long.Parse(args[3]);
        dh = new DiffieHellmanKeyGenerator(p, g, privateKey);
        outputFile = args[4];
    }
    else
    {
        return;
    }

    dh.SaveToFile(outputFile);
}

static void HandleEncryptionDecryption(string[] args, string mode)
{
    if (args.Length < 4) return;
    
    string inputPath = args[1];
    string partnerKeyFile = args[2];
    long myPrivateKey = long.Parse(args[3]);
    string outputPath = args.Length >= 5 ? args[4] : (mode == "-encrypt" ? "encrypted.bin" : "decrypted.bin");
    
    var (partnerP, partnerG, partnerPublicKey) = DiffieHellmanKeyGenerator.LoadFromFile(partnerKeyFile);
    
    var dh = new DiffieHellmanKeyGenerator(partnerP, partnerG, myPrivateKey);
    long sharedSecret = dh.CalculateSharedSecret(partnerPublicKey, myPrivateKey);
    
    byte[] fileBytes = File.ReadAllBytes(inputPath);
    byte[] keyBytes = GenerateKeyFromLong(sharedSecret, fileBytes.Length);
    byte[] resultBytes = Encrypt(fileBytes, keyBytes);
    
    File.WriteAllBytes(outputPath, resultBytes);
}

static byte[] GenerateKeyFromLong(long key, int length)
{
    Random random = new Random((int) key);
    
    byte[] keyBytes = new byte[length];
    byte[] longBytes = BitConverter.GetBytes(key);

    for (int i = 0; i < length / sizeof(long); i += sizeof(long))
    {
        long rnd = random.Next();
        for (int j = i; j < (i + 1) * sizeof(long); j++)
        {
            keyBytes[j] = (byte) ((j + (rnd % byte.MaxValue)) % byte.MaxValue);
        }
    }
    //Console.WriteLine($"Vernam Key: {keyBytes}");
    return keyBytes;
}

static byte[] Encrypt(byte[] plainTextBytes, byte[] key)
{
    if (plainTextBytes.Length != key.Length)
        throw new ArgumentException("Key length must be equal to plaintext length for Vernam Cipher.");

    byte[] cipherTextBytes = new byte[plainTextBytes.Length];
    for (int i = 0; i < plainTextBytes.Length; i++)
        cipherTextBytes[i] = (byte)(plainTextBytes[i] ^ key[i]);
    return cipherTextBytes;
}

static byte[] Decrypt(byte[] cipherTextBytes, byte[] key) =>
    Encrypt(cipherTextBytes, key);