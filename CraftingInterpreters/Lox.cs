namespace CraftingInterpreters;

public static class Lox
{
    public static bool HadError { get; private set; } = false;

    public static void RunFile(string fileName)
    {
        var source = File.ReadAllText(fileName);
        Run(source);
        if (HadError)
        {
            Environment.Exit(64);
        }
    }

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            if (HadError)
            {
                Environment.Exit(64);
            }
        }
    }

    public static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message) =>
        Report(line, "", message);

    public static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }
}