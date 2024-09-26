namespace CraftingInterpreters.Lox.Stmt;

public interface IVisitor<out T>
{
    T VisitBlockStmt(Block stmt);
    T VisitExpressionStmt(Expression stmt);
    T VisitIfStmt(If stmt);
    T VisitPrintStmt(Print stmt);
    T VisitVarStmt(Var stmt);
}