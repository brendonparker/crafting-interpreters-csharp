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