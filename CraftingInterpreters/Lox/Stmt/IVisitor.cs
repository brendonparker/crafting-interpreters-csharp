namespace CraftingInterpreters.Lox.Stmt;

public interface IVisitor<out T>
{
    T VisitExpressionStmt(Expression expr);
    T VisitPrintStmt(Print expr);
}