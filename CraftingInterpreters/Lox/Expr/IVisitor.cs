namespace CraftingInterpreters.Lox.Expr;

public interface IVisitor<out T>
{
    T VisitBinaryExpr(Binary expr);
    T VisitGroupingExpr(Grouping expr);
    T VisitLiteralExpr(Literal expr);
    T VisitUnaryExpr(Unary expr);
}