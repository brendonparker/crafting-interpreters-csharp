using static CraftingInterpreters.Lox.TokenType;

namespace CraftingInterpreters.Lox;

public class Parser(List<Token> tokens)
{
    private int _current = 0;

    private Expr.Expr Expression() =>
        Equality();

    private Expr.Expr Equality()
    {
        var expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr.Expr Comparison()
    {
        throw new NotImplementedException();
    }

    private bool Match(params TokenType[] tokenTypes)
    {
        foreach (var tokenType in tokenTypes)
        {
            if (Check(tokenType))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type) =>
        !IsAtEnd() && Peek().Type == type;

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() =>
        Peek().Type == EOF;

    private Token Peek() =>
        tokens[_current];

    private Token Previous() =>
        tokens[_current - 1];
}