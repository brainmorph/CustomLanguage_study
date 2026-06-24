namespace BabyVM;

// Represents the 32-word unified store (STORE.txt).
// Each word is 32 bits, stored MSB-first as ASCII '0'/'1'.
// Line number (0-indexed) equals the word's address.
public class Store
{
    private readonly string _path;
    private readonly int[] _words = new int[32];

    public Store(string path)
    {
        _path = path;
        string[] lines = File.ReadAllLines(path);

        if (lines.Length < 32)
            throw new FormatException($"STORE.txt must have exactly 32 lines, found {lines.Length}.");

        for (int i = 0; i < 32; i++)
            _words[i] = Parse(lines[i], i);
    }

    public int Read(int address) => _words[address & 0x1F];

    public void Write(int address, int value)
    {
        address &= 0x1F;
        _words[address] = value;

        // Mutate STORE.txt in-place so the state is always visible on disk.
        string[] lines = File.ReadAllLines(_path);
        lines[address] = WordToBits(value);
        File.WriteAllLines(_path, lines);
    }

    // Converts a 32-char MSB-first bit string to a signed 32-bit int.
    private static int Parse(string bits, int lineIndex)
    {
        bits = bits.Trim();
        if (bits.Length != 32)
            throw new FormatException(
                $"Line {lineIndex} must be exactly 32 bits, got {bits.Length}: '{bits}'");

        uint value = Convert.ToUInt32(bits, 2);
        return (int)value;   // reinterpret the bit pattern as signed
    }

    // Converts a signed 32-bit int to a 32-char MSB-first bit string.
    public static string WordToBits(int value)
    {
        char[] bits = new char[32];
        uint uval = (uint)value;
        for (int i = 0; i < 32; i++)
            bits[i] = ((uval >> (31 - i)) & 1u) == 1u ? '1' : '0';
        return new string(bits);
    }
}
