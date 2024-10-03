namespace CraftingInterpreters.Lox;

public class ReturnException(object? value = null) : Exception
{
    public object? Value { get; } = value;
}