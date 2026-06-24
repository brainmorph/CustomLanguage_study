namespace BabyVM;

internal static class Program
{
    private static int Main(string[] args)
    {
        string storePath  = args.Length > 0 ? args[0] : "STORE.txt";
        string inputPath  = args.Length > 1 ? args[1] : "INPUT.txt";
        string outputPath = args.Length > 2 ? args[2] : "OUTPUT.txt";

        if (!File.Exists(storePath))
        {
            Console.Error.WriteLine($"Error: store file not found: {storePath}");
            return 1;
        }

        try
        {
            var store   = new Store(storePath);
            var io      = new MachineIO(inputPath, outputPath);
            var machine = new Machine(store, io);
            machine.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
