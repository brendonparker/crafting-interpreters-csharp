using static CraftingInterpreters.Lox.TokenType;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace CraftingInterpreters.Lox;

public class Void;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<Void>
{
    private static readonly Void Void = new();
    public LoxEnvironment Globals { get; }
    private LoxEnvironment _env;

    public Interpreter()
    {
        Globals = _env = new LoxEnvironment();
        Globals.Define("clock", ClockCallable.Instance);
    }

    public object VisitAssignExpr(Expr.Assign expr) =>
        _env.Assign(expr.Name, Evaluate(expr.Value))!;

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var (left, right) = (Evaluate(expr.Left), Evaluate(expr.Right));

        switch (expr.Op.Type)
        {
            case EQUAL_EQUAL: return AreEqual(left, right);
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

    public object VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);

        var arguments = expr.Arguments
            .Select(Evaluate)
            .ToArray();
        if (callee is ILoxCallable function)
        {
            if (function.Arity != arguments.Length)
            {
                throw new RuntimeException(expr.Paren,
                    $"Expected {function.Arity} arguments but got {arguments.Length}.");
            }

            return function.Call(this, arguments)!;
        }

        throw new RuntimeException(expr.Paren, "Can only call functions and classes.");
    }

    public object VisitGroupingExpr(Expr.Grouping expr) =>
        Evaluate(expr.Expression);

    public object VisitLiteralExpr(Expr.Literal expr) =>
        expr.Value;

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);
        var leftTruthy = IsTruthy(left);

        if (expr.Op.Type == OR && leftTruthy) return left;
        if (!leftTruthy) return left;
        return Evaluate(expr.Right);
    }

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

    public Void VisitIfStmt(Stmt.If stmt)
    {
        var result = Evaluate(stmt.Condition);
        if (IsTruthy(result))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return Void;
    }

    public Void VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new LoxEnvironment(_env));
        return Void;
    }

    public Void VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.InnerExpression);
        return Void;
    }

    public Void VisitPrintStmt(Stmt.Print stmt)
    {
        var result = Evaluate(stmt.ExpressionToPrint);
        Print(result);
        return Void;
    }

    public Void VisitVarStmt(Stmt.Var stmt)
    {
        var value = stmt.Initializer == null ? null : Evaluate(stmt.Initializer);
        _env.Define(stmt.Name.Lexeme, value);
        return Void;
    }

    public Void VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        return Void;
    }

    public Void VisitFunctionStmt(Stmt.Function stmt)
    {
        LoxFunction function = new(stmt);
        _env.Define(stmt.Name.Lexeme, function);
        return Void;
    }

    public Void VisitReturnStmt(Stmt.Return stmt)
    {
        if (stmt.Value is null) throw new ReturnException();
        throw new ReturnException(Evaluate(stmt.Value));
    }

    public void Interpret(List<Stmt> statements)
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

    private Void Execute(Stmt statement) =>
        statement.Accept(this);

    public void ExecuteBlock(List<Stmt> statements, LoxEnvironment environment)
    {
        var prev = _env;
        try
        {
            _env = environment;
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _env = prev;
        }
    }

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
            double doubleVal => doubleVal == 0,
            _ => true
        };

    private object Evaluate(Expr expr) =>
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