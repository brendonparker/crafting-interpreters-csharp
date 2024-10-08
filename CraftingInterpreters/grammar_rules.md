Expressions:

```
expression     → assignment ;

assignment     → IDENTIFIER "=" assignment
               | logic_or ;
               
logic_or       → logic_and ( "or" logic_and )* ;

logic_and      → equality ( "and" equality )* ;

equality       → comparison ( ( "!=" | "==" ) comparison )* ;

comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;

term           → factor ( ( "-" | "+" ) factor )* ;

factor         → unary ( ( "/" | "*" ) unary )* ;

unary          → ( "!" | "-" ) unary
               | call ;

call           → primary ( "(" arguments? ")" )* ;

primary        → NUMBER | STRING | "true" | "false" | "nil"
               | "(" expression ")" 
               | IDENTIFIER;
```

Statements:

```
program        → declaration* EOF ;

declaration    → funDecl
               | varDecl
               | statement ;

funDecl        → "fun" function ;

function       → IDENTIFIER "(" parameters? ")" block ;

varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;

statement      → exprStmt
               | ifStmt
               | printStmt
               | whileStmt
               | forStmt
               | returnStmt
               | block ;
               
whileStmt      → "while" "(" expression ")" statement ;

forStmt        → "for" "(" ( varDecl | exprStmt | ";" )
                 expression? ";"
                 expression? ")" statement ;
               
returnStmt     → "return" expression? ";" ;

ifStmt         → "if" "(" expression ")" statement
               ( "else" statement )? ;

block          → "{" declaration* "}" ;exprStmt       → expression ";" ;

exprStmt       → expression ";" ;

printStmt      → "print" expression ";" ;
```
