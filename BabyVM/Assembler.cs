namespace BabyVM;

// Translates a .asm source file into a 32-word STORE.txt.
//
// Source format:
//   ORG n       — set address counter to n
//   DAT n       — place signed decimal n at current address, then advance
//   <mnemonic>  — encode instruction at current address, then advance
//   ; comment   — ignored from ';' to end of line
//   blank lines — ignored
public static class Assembler
{
    private static readonly Dictionary<string, int> Opcodes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "JMP", 0 },
        { "JRP", 1 },
        { "LDN", 2 },
        { "STO", 3 },
        { "SUB", 4 },
        { "CMP", 6 },
        { "STP", 7 },
    };

    private static readonly HashSet<string> NoOperand =
        new(StringComparer.OrdinalIgnoreCase) { "CMP", "STP" };

    public static void AssembleFile(string sourcePath, string outputPath)
    {
        int[] store = new int[32];
        int address = 0;
        int lineNumber = 0;

        foreach (string raw in File.ReadLines(sourcePath))
        {
            lineNumber++;

            // Strip comment and whitespace.
            string line = raw;
            int commentIdx = line.IndexOf(';');
            if (commentIdx >= 0) line = line[..commentIdx];
            line = line.Trim();

            if (line.Length == 0) continue;

            string[] parts = line.Split(new char[] { ' ', '\t' },
                StringSplitOptions.RemoveEmptyEntries);
            string mnemonic = parts[0];

            if (mnemonic.Equals("ORG", StringComparison.OrdinalIgnoreCase))
            {
                address = ParseOperand(parts, lineNumber, mnemonic);
                ValidateAddress(address, lineNumber, "ORG target");
                continue;
            }

            if (mnemonic.Equals("DAT", StringComparison.OrdinalIgnoreCase))
            {
                ValidateAddress(address, lineNumber, "DAT placement");
                store[address++] = ParseOperand(parts, lineNumber, mnemonic);
                continue;
            }

            if (!Opcodes.TryGetValue(mnemonic, out int opcode))
                throw new AssemblerException(lineNumber, $"Unknown mnemonic '{mnemonic}'.");

            ValidateAddress(address, lineNumber, $"{mnemonic} placement");

            int operand = 0;
            if (!NoOperand.Contains(mnemonic))
            {
                operand = ParseOperand(parts, lineNumber, mnemonic);
                if (operand < 0 || operand > 31)
                    throw new AssemblerException(lineNumber,
                        $"Operand {operand} is out of range (must be 0–31).");
            }

            store[address++] = (opcode << 13) | operand;
        }

        WriteStore(store, outputPath);
    }

    private static int ParseOperand(string[] parts, int lineNumber, string mnemonic)
    {
        if (parts.Length < 2)
            throw new AssemblerException(lineNumber, $"'{mnemonic}' requires an operand.");

        if (!int.TryParse(parts[1], out int value))
            throw new AssemblerException(lineNumber,
                $"'{parts[1]}' is not a valid integer operand.");

        return value;
    }

    private static void ValidateAddress(int address, int lineNumber, string context)
    {
        if (address < 0 || address > 31)
            throw new AssemblerException(lineNumber,
                $"{context} address {address} is out of range (must be 0–31).");
    }

    private static void WriteStore(int[] store, string outputPath)
    {
        string[] lines = new string[32];
        for (int i = 0; i < 32; i++)
            lines[i] = Store.WordToBits(store[i]);
        File.WriteAllLines(outputPath, lines);
    }
}

public sealed class AssemblerException : Exception
{
    public int LineNumber { get; }

    public AssemblerException(int lineNumber, string message)
        : base($"Line {lineNumber}: {message}")
    {
        LineNumber = lineNumber;
    }
}
