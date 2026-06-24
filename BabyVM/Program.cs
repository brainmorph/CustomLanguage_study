namespace BabyVM;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 0;
        }

        return args[0].ToLowerInvariant() switch
        {
            "run"      => Run(args[1..]),
            "assemble" => Assemble(args[1..]),
            _          => Error($"Unknown command '{args[0]}'. Run with no arguments for usage.")
        };
    }

    private static int Run(string[] args)
    {
        string storePath  = args.Length > 0 ? args[0] : "STORE.txt";
        string inputPath  = args.Length > 1 ? args[1] : "INPUT.txt";
        string outputPath = args.Length > 2 ? args[2] : "OUTPUT.txt";

        if (!File.Exists(storePath))
            return Error($"Store file not found: {storePath}");

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
            return Error(ex.Message);
        }
    }

    private static int Assemble(string[] args)
    {
        if (args.Length < 2)
            return Error("Usage: BabyVM assemble <source.asm> <store.txt>");

        string sourcePath = args[0];
        string outputPath = args[1];

        if (!File.Exists(sourcePath))
            return Error($"Source file not found: {sourcePath}");

        try
        {
            Assembler.AssembleFile(sourcePath, outputPath);
            Console.WriteLine($"Assembled '{sourcePath}' → '{outputPath}'");
            return 0;
        }
        catch (AssemblerException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("BabyVM — Manchester Baby (SSEM 1948) emulator");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  run      <store.txt> [input.txt] [output.txt]  — execute a store file");
        Console.WriteLine("  assemble <source.asm> <store.txt>              — assemble source to store file");
    }

    private static int Error(string message)
    {
        Console.Error.WriteLine($"Error: {message}");
        return 1;
    }
}
