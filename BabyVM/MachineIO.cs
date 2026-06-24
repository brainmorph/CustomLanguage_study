namespace BabyVM;

// Handles memory-mapped I/O at reserved store address 31.
//   LDN 31 — reads next word from INPUT.txt, negates it, places in ACC
//   STO 31 — appends ACC as a 32-bit word to OUTPUT.txt
public class MachineIO
{
    private readonly string _outputPath;
    private readonly Queue<int> _inputQueue = new();

    public MachineIO(string inputPath, string outputPath)
    {
        _outputPath = outputPath;

        if (File.Exists(inputPath))
        {
            foreach (string line in File.ReadAllLines(inputPath))
            {
                string trimmed = line.Trim();
                if (trimmed.Length > 0)
                    _inputQueue.Enqueue((int)Convert.ToUInt32(trimmed, 2));
            }
        }
    }

    public int ReadInput()
    {
        if (_inputQueue.Count == 0)
            throw new InvalidOperationException("INPUT.txt exhausted — no more words to read.");
        return _inputQueue.Dequeue();
    }

    public void WriteOutput(int value)
    {
        File.AppendAllText(_outputPath, Store.WordToBits(value) + Environment.NewLine);
    }
}
