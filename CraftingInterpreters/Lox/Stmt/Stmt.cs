namespace CraftingInterpreters.Lox.Stmt;

// TODO: Somehow convert this to generated code?

public abstract record Stmt
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}

public record Block(List<Stmt> Statements) : Stmt
{
    public override T Accept<T>(IVisitor<T> visitor) =>
        visitor.VisitBlockStmt(this);
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

public record Var(Token Name, Expr.Expr? Initializer) : Stmt
{
    public override T Accept<T>(IVisitor<T> visitor) =>
        visitor.VisitVarStmt(this);
}