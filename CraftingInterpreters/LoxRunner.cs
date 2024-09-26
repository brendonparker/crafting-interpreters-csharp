namespace CraftingInterpreters.Lox;

public static class LoxRunner
{
    private static Interpreter _interpreter = new();
    public static bool HadError { get; private set; } = false;
    public static bool HadRuntimeError { get; private set; } = false;

    public static void RunFile(string fileName)
    {
        var source = File.ReadAllText(fileName);
        Run(source);
        HandleErrors();
    }

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            HandleErrors();
        }
    }

    private static void HandleErrors()
    {
        if (HadError)
        {
            Environment.Exit(64);
        }

        if (HadRuntimeError)
        {
            Environment.Exit(70);
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
        
        _interpreter.Interpret(expression!);
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

    public static void HandleRuntimeException(Interpreter.RuntimeException ex)
    {
        Console.WriteLine(ex.Message +
                          "\n[line " + ex.Token.Line + "]");
        HadRuntimeError = true;
    }
}