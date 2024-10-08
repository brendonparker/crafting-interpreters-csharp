namespace CraftingInterpreters.Lox;

public static class IdentifiersHelper
{
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["and"] = TokenType.AND,
        ["class"] = TokenType.CLASS,
        ["else"] = TokenType.ELSE,
        ["false"] = TokenType.FALSE,
        ["for"] = TokenType.FOR,
        ["fun"] = TokenType.FUN,
        ["if"] = TokenType.IF,
        ["nil"] = TokenType.NIL,
        ["or"] = TokenType.OR,
        ["print"] = TokenType.PRINT,
        ["return"] = TokenType.RETURN,
        ["super"] = TokenType.SUPER,
        ["this"] = TokenType.THIS,
        ["true"] = TokenType.TRUE,
        ["var"] = TokenType.VAR,
        ["while"] = TokenType.WHILE
    };

    public static TokenType? Get(string keyword) =>
        Keywords.TryGetValue(keyword, out var tokenType) ? tokenType : null;
}