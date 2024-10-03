namespace CraftingInterpreters.Lox;

public interface ILoxCallable
{
    int Arity { get; }
    object Call(Interpreter interpreter, object[] arguments);
}