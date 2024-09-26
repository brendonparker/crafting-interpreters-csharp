namespace CraftingInterpreters.Lox.Expr;

public interface IVisitor<out T>
{
    T VisitAssignExpr(Assign expr);
    T VisitBinaryExpr(Binary expr);
    T VisitGroupingExpr(Grouping expr);
    T VisitLiteralExpr(Literal expr);
    T VisitLogicalExpr(Logical expr);
    T VisitUnaryExpr(Unary expr);
    T VisitVariableExpr(Variable expr);
}