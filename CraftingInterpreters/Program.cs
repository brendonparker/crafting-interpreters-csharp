using CraftingInterpreters.Lox;

if (args.Length > 1)
{
    Console.WriteLine("Usage: lox [script]");
    Environment.Exit(64);
    return;
}

if (args.Length == 1)
{
    LoxRunner.RunFile(args[0]);
    if (LoxRunner.HadError) Environment.Exit(64);

    return;
}

LoxRunner.RunPrompt();
