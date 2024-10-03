namespace CraftingInterpreters.Lox;

public interface ILoxCallable
{
    int Arity { get; }
    object Call(Interpreter interpreter, object[] arguments);
}

public class ClockCallable : ILoxCallable
{
    public static ClockCallable Instance { get; } = new();

    public int Arity => 0;

    public object Call(Interpreter interpreter, object[] arguments) =>
        DateTime.UtcNow.Ticks;
}

public class LoxFunction(Stmt.Function declaration) : ILoxCallable
{
    public int Arity => declaration.Params.Count;

    public object Call(Interpreter interpreter, object[] arguments)
    {
        LoxEnvironment env = new(interpreter.Globals);

        for (var i = 0; i < declaration.Params.Count; i++)
        {
            var argument = declaration.Params[i];
            env.Define(argument.Lexeme, arguments[i]);
        }

        interpreter.ExecuteBlock(declaration.Body, env);
        return null!;
    }

    public override string ToString() =>
        $"<fn {declaration.Name.Lexeme}>";
}