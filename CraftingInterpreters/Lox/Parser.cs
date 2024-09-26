using CraftingInterpreters.Lox.Expr;
using CraftingInterpreters.Lox.Stmt;
using static CraftingInterpreters.Lox.TokenType;

namespace CraftingInterpreters.Lox;

public class Parser(List<Token> tokens)
{
    private int _current;

    public List<Stmt.Stmt> Parse()
    {
        List<Stmt.Stmt> statements = new();

        while (!IsAtEnd()) statements.Add(Statement());

        return statements;
    }

    private Stmt.Stmt Statement()
    {
        if (Match(PRINT)) return PrintStatement();
        return ExpressionStatement();
    }

    private Stmt.Stmt PrintStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Print(expr);
    }

    private Stmt.Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new Expression(expr);
    }

    private Expr.Expr Expression() =>
        Equality();

    private Expr.Expr Equality()
    {
        var expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr.Expr Comparison()
    {
        var expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr.Expr Term()
    {
        var expr = Factor();

        while (Match(PLUS, MINUS))
        {
            var op = Previous();
            var right = Factor();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr.Expr Factor()
    {
        var expr = Unary();

        while (Match(SLASH, STAR))
        {
            var op = Previous();
            var right = Unary();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr.Expr Unary()
    {
        if (Match(BANG, MINUS))
            return new Unary(Previous(), Unary());
        return Primary();
    }

    private Expr.Expr Primary()
    {
        if (Match(FALSE, TRUE, NIL)) return new Literal(Previous().Type);
        if (Match(NUMBER, STRING)) return new Literal(Previous().Literal);

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Grouping(expr);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private bool Match(params TokenType[] tokenTypes)
    {
        foreach (var tokenType in tokenTypes)
            if (Check(tokenType))
            {
                Advance();
                return true;
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

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON) return;

            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            Advance();
        }
    }

    private ParserException Error(Token token, string message)
    {
        LoxRunner.Error(token, message);
        return new ParserException();
    }

    private class ParserException : ApplicationException;
}