using System.Text;
using CraftingInterpreters.Lox.Expr;

namespace CraftingInterpreters.Lox;

public class AstPrinter : IVisitor<string>
{
    public string VisitBinaryExpr(Binary expr) =>
        Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);

    public string VisitGroupingExpr(Grouping expr) =>
        Parenthesize("group", expr.Expression);

    public string VisitLiteralExpr(Literal expr) =>
        expr.Value == null ? "nil" : expr.Value!.ToString() ?? "";

    public string VisitUnaryExpr(Unary expr) =>
        Parenthesize(expr.Op.Lexeme, expr.Right);

    public string Print(Expr.Expr expr) =>
        expr.Accept(this);

    private string Parenthesize(string name, params Expr.Expr[] expressions)
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