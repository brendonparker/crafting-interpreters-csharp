using CraftingInterpreters.Lox.Expr;
using static CraftingInterpreters.Lox.TokenType;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace CraftingInterpreters.Lox;

public class Interpreter : IVisitor<object>
{
    public void Interpret(Expr.Expr expression)
    {
        try
        {
            var value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeException e)
        {
            LoxRunner.HandleRuntimeException(e);
        }
    }

    private string Stringify(object? obj)
    {
        if (obj == null) return "nil";

        if (obj is double objectAsDouble)
        {
            return $"{objectAsDouble:0.#####}";
        }

        return obj.ToString()!;
    }

    public object VisitBinaryExpr(Binary expr)
    {
        var (left, right) = (Evaluate(expr.Left), Evaluate(expr.Right));

        switch (expr.Op.Type)
        {
            case EQUAL: return AreEqual(left, right);
            case BANG_EQUAL: return !AreEqual(left, right);
        }

        if (left is double leftAsDouble && right is double rightAsDouble)
        {
            return expr.Op.Type switch
            {
                MINUS => leftAsDouble - rightAsDouble,
                SLASH => leftAsDouble / rightAsDouble,
                STAR => leftAsDouble * rightAsDouble,
                PLUS => leftAsDouble + rightAsDouble,
                GREATER => leftAsDouble > rightAsDouble,
                GREATER_EQUAL => leftAsDouble >= rightAsDouble,
                LESS => leftAsDouble < rightAsDouble,
                LESS_EQUAL => leftAsDouble <= rightAsDouble,
            };
        }

        if (left is string leftAsString && right is string rightAsString)
        {
            return expr.Op.Type switch
            {
                PLUS => leftAsString + rightAsString
            };
        }

        switch (expr.Op.Type)
        {
            case MINUS:
            case SLASH:
            case STAR:
            case GREATER:
            case GREATER_EQUAL:
            case LESS:
            case LESS_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                break;
            case PLUS:
                CheckNumberOrStringsOperands(expr.Op);
                break;
        }

        return null!;
    }

    public object VisitGroupingExpr(Grouping expr) =>
        Evaluate(expr.Expression);

    public object VisitLiteralExpr(Literal expr) =>
        expr.Value;

    public object VisitUnaryExpr(Unary expr)
    {
        var right = Evaluate(expr.Right);

        return expr.Op.Type switch
        {
            BANG => !IsTruthy(right),
            MINUS => -CheckNumberOperand(expr.Op, right),
            _ => null!
        };
    }

    private bool IsTruthy(object? val) =>
        val switch
        {
            null => false,
            bool boolVal => boolVal,
            _ => true
        };

    private object Evaluate(Expr.Expr expr) =>
        expr.Accept(this);

    private bool AreEqual(object? a, object? b) =>
        a switch
        {
            null when b == null => true,
            null => false,
            _ => a.Equals(b)
        };

    private double CheckNumberOperand(Token op, object operand)
    {
        if (operand is double doubleVal) return doubleVal;
        throw new RuntimeException(op, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token op, object operand1, object operand2)
    {
        CheckNumberOperand(op, operand1);
        CheckNumberOperand(op, operand2);
    }

    private void CheckNumberOrStringsOperands(Token op) =>
        throw new RuntimeException(op, "Operands must be two numbers or two strings.");

    public class RuntimeException(Token token, string message) : Exception
    {
        public Token Token { get; } = token;
        public override string Message { get; } = message;
    }
}