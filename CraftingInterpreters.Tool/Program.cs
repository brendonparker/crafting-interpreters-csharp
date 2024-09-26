if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: generate_ast <output directory>");
    Environment.Exit(64);
    return;
}

var outputDir = args[0];

DefineAst(outputDir, "Expr", [
    "Binary   : Expr left, Token op, Expr right",
    "Grouping : Expr expression",
    "Literal  : Object value",
    "Unary    : Token op, Expr right"
]);

static void DefineAst(string outputDir, string baseName, string[] types)
{
    if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
    var path = $"{outputDir}/{baseName}.cs";
    using var stream = File.OpenWrite(path);
    using var writer = new StreamWriter(stream, leaveOpen: true);
    writer.WriteLine($"namespace CraftingInterpreters.Lox.{baseName};");

    writer.WriteLine();
    writer.WriteLine($"public abstract record {baseName};");
    writer.WriteLine();
    foreach (var record in types)
    {
        var parts = record.Split(':').Select(x => x.Trim()).ToArray();
        var (className, fields) = (parts[0], parts[1]);
        writer.WriteLine($"public record {className}({fields}) : {baseName};");
    }
}