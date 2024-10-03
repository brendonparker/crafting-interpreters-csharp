using static CraftingInterpreters.Lox.TokenType;

namespace CraftingInterpreters.Lox;

public class Parser(List<Token> tokens)
{
    private int _current;

    public List<Stmt> Parse()
    {
        List<Stmt> statements = new();

        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(FUN)) return FunctionStatement();
            if (Match(VAR)) return VarStatement();
            return Statement();
        }
        catch (ParserException)
        {
            Synchronize();
            return null!;
        }
    }

    private Stmt Statement()
    {
        if (Match(IF)) return IfStatement();
        if (Match(VAR)) return VarStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(LEFT_BRACE)) return BlockStatement();

        return ExpressionStatement();
    }

    private Stmt.If IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        var expr = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after 'if' condition.");
        var thenBranch = Statement();
        var elseBranch = Match(ELSE) ? Statement() : null;
        return new Stmt.If(expr, thenBranch, elseBranch);
    }

    private Stmt.Block BlockStatement()
    {
        List<Stmt> statements = [];
        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return new Stmt.Block(statements);
    }

    private Stmt PrintStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(expr);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Stmt FunctionStatement(string kind = "function")
    {
        var name = Consume(IDENTIFIER, $"Expect {kind} name.");
        Consume(LEFT_PAREN, $"Expected '(' after {kind} name.");

        List<Token> parameters = new();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(
                    Consume(IDENTIFIER, "Expect parameter name."));
            } while (Match(COMMA));
        }

        Consume(RIGHT_PAREN, "Expect ')' after parameters.");
        var body = BlockStatement();
        return new Stmt.Function(name, parameters, body.Statements);
    }

    private Stmt VarStatement()
    {
        var name = Consume(IDENTIFIER, "Expect variable name.");
        var initializer = Match(EQUAL) ? Expression() : null;
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Var(name, initializer);
    }

    private Expr Expression() =>
        Assignment();

    private Expr Assignment()
    {
        var expr = Or();
        if (Match(EQUAL))
        {
            var equals = Previous();
            var value = Assignment();
            if (expr is Expr.Variable exprAsVariable)
            {
                return new Expr.Assign(exprAsVariable.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(OR))
        {
            var op = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(AND))
        {
            var op = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
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

    private Expr Comparison()
    {
        var expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(PLUS, MINUS))
        {
            var op = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(SLASH, STAR))
        {
            var op = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
            return new Expr.Unary(Previous(), Unary());
        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (!Match(LEFT_PAREN)) break;

            expr = FinishCall(expr);
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> arguments = new();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }

                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        var paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");
        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(FALSE, TRUE, NIL)) return new Expr.Literal(Previous().Type);
        if (Match(NUMBER, STRING)) return new Expr.Literal(Previous().Literal);
        if (Match(IDENTIFIER)) return new Expr.Variable(Previous());

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
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