using static CraftingInterpreters.Lox.TokenType;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace CraftingInterpreters.Lox;

public class Void;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Void>
{
    private static readonly Void Void = new();
    private readonly LoxEnvironment _env = new();

    public object VisitAssignExpr(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var (left, right) = (Evaluate(expr.Left), Evaluate(expr.Right));

        switch (expr.Op.Type)
        {
            case EQUAL: return AreEqual(left, right);
            case BANG_EQUAL: return !AreEqual(left, right);
        }

        if (left is double leftAsDouble && right is double rightAsDouble)
            return expr.Op.Type switch
            {
                MINUS => leftAsDouble - rightAsDouble,
                SLASH => leftAsDouble / rightAsDouble,
                STAR => leftAsDouble * rightAsDouble,
                PLUS => leftAsDouble + rightAsDouble,
                GREATER => leftAsDouble > rightAsDouble,
                GREATER_EQUAL => leftAsDouble >= rightAsDouble,
                LESS => leftAsDouble < rightAsDouble,
                LESS_EQUAL => leftAsDouble <= rightAsDouble
            };

        if (left is string leftAsString && right is string rightAsString)
            return expr.Op.Type switch
            {
                PLUS => leftAsString + rightAsString
            };

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

    public object VisitGroupingExpr(Expr.Grouping expr) =>
        Evaluate(expr.Expression);

    public object VisitLiteralExpr(Expr.Literal expr) =>
        expr.Value;

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        return expr.Op.Type switch
        {
            BANG => !IsTruthy(right),
            MINUS => -CheckNumberOperand(expr.Op, right),
            _ => null!
        };
    }

    public object VisitVariableExpr(Expr.Variable expr) =>
        _env.Get(expr.Name)!;

    public Void VisitExpressionStmt(Stmt.Expression expr)
    {
        Evaluate(expr.InnerExpression);
        return Void;
    }

    public Void VisitPrintStmt(Stmt.Print expr)
    {
        var result = Evaluate(expr.Expression);
        Print(result);
        return Void;
    }

    public Void VisitVarStmt(Stmt.Var expr)
    {
        var value = expr.Initializer == null ? null : Evaluate(expr.Initializer);
        _env.Define(expr.Name.Lexeme, value);
        return Void;
    }

    public void Interpret(List<Stmt.Stmt> statements)
    {
        try
        {
            foreach (var statement in statements) Execute(statement);
        }
        catch (RuntimeException e)
        {
            LoxRunner.HandleRuntimeException(e);
        }
    }

    private void Execute(Stmt.Stmt statement) =>
        statement.Accept(this);

    private void Print(object value) =>
        Console.WriteLine(Stringify(value));

    private string Stringify(object? obj)
    {
        if (obj == null) return "nil";

        if (obj is double objectAsDouble) return $"{objectAsDouble:0.#####}";

        return obj.ToString()!;
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