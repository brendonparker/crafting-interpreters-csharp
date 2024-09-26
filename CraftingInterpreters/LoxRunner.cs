namespace CraftingInterpreters.Lox;

public static class LoxRunner
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
        Parser parser = new(tokens);
        
        var expression = parser.Parse();

        // Stop if there was a syntax error.
        if (HadError) return;

        Console.WriteLine(new AstPrinter().Print(expression!));
        
        // foreach (var token in tokens)
        // {
        //     Console.WriteLine(token);
        // }
    }

    public static void Error(int line, string message) =>
        Report(line, "", message);

    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }

    public static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }
}