#pragma warning disable CS0162 // Unreachable code detected

using CraftingInterpreters.Lox;

// using Expr = CraftingInterpreters.Lox.Expr;
//
// var expression = new Expr.Binary(
//     new Expr.Unary(
//         new Token(TokenType.MINUS, "-", null, 1),
//         new Expr.Literal(123)),
//     new Token(TokenType.STAR, "*", null, 1),
//     new Expr.Grouping(
//         new Expr.Literal(45.67)));
//
// Console.WriteLine(new AstPrinter().Print(expression));
// return;

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