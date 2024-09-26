namespace CraftingInterpreters.Lox;

public class LoxEnvironment(LoxEnvironment? enclosing = null)
{
    private readonly Dictionary<string, object?> _values = new();

    public void Define(string name, object? value) =>
        _values[name] = value;

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value)) return value;
        if (enclosing != null) return enclosing.Get(name);
        throw new Interpreter.RuntimeException(name, $"Undefined variable: {name.Lexeme}");
    }

    public object? Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return value;
        }

        if (enclosing != null) return enclosing.Assign(name, value);
        throw new Interpreter.RuntimeException(name, $"Undefined variable: {name.Lexeme}");
    }
}