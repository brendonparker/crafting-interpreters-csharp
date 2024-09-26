namespace CraftingInterpreters.Lox.Stmt;

// TODO: Somehow convert this to generated code?

public abstract record Stmt
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record Expression(Expr.Expr InnerExpression) : Stmt
{
    public override T Accept<T>(IVisitor<T> visitor) =>
        visitor.VisitExpressionStmt(this);
}

public record Print(Expr.Expr Expression) : Stmt
{
    public override T Accept<T>(IVisitor<T> visitor) =>
        visitor.VisitPrintStmt(this);
}