using System.Text;

namespace CraftingInterpreters.Lox;

public class AstPrinter : Expr.IVisitor<string>
{
    public string VisitAssignExpr(Expr.Assign expr) =>
        throw new NotImplementedException();

    public string VisitBinaryExpr(Expr.Binary expr) =>
        Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);

    public string VisitCallExpr(Expr.Call expr)
    {
        throw new NotImplementedException();
    }

    public string VisitGroupingExpr(Expr.Grouping expr) =>
        Parenthesize("group", expr.Expression);

    public string VisitLiteralExpr(Expr.Literal expr) =>
        expr.Value == null ? "nil" : expr.Value!.ToString() ?? "";

    public string VisitLogicalExpr(Expr.Logical expr) =>
        throw new NotImplementedException();

    public string VisitUnaryExpr(Expr.Unary expr) =>
        Parenthesize(expr.Op.Lexeme, expr.Right);

    public string VisitVariableExpr(Expr.Variable expr) =>
        throw new NotImplementedException();

    public string Print(Expr expr) =>
        expr.Accept(this);

    private string Parenthesize(string name, params Expr[] expressions)
    {
        StringBuilder builder = new();

        builder.Append('(').Append(name);
        foreach (var expr in expressions)
        {
            builder.Append(' ');
            builder.Append(expr.Accept(this));
        }

        builder.Append(')');

        return builder.ToString();
    }
}