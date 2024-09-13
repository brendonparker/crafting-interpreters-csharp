using CraftingInterpreters;

if (args.Length > 1)
{
    Console.WriteLine("Usage: lox [script]");
    Environment.Exit(64);
    return;
}

if (args.Length == 1)
{
    Lox.RunFile(args[0]);
    if (Lox.HadError)
    {
        Environment.Exit(64);
    }

    return;
}

Lox.RunPrompt();