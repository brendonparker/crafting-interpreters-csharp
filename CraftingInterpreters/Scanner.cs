using static CraftingInterpreters.TokenType;

namespace CraftingInterpreters;

public class Scanner(string source)
{
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private List<Token> tokens = [];

    public List<Token> ScanTokens()
    {
        tokens = [];
        while (!IsAtEnd)
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(EOF, "", null, line));
        return tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            // Lexems
            case '(':
                AddToken(LEFT_PAREN);
                break;
            case ')':
                AddToken(RIGHT_PAREN);
                break;
            case '{':
                AddToken(LEFT_BRACE);
                break;
            case '}':
                AddToken(RIGHT_BRACE);
                break;
            case ',':
                AddToken(COMMA);
                break;
            case '.':
                AddToken(DOT);
                break;
            case '-':
                AddToken(MINUS);
                break;
            case '+':
                AddToken(PLUS);
                break;
            case ';':
                AddToken(SEMICOLON);
                break;
            case '*':
                AddToken(STAR);
                break;
            // Operators
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;
            case '/':
                if (Match('/'))
                {
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd) Advance();
                }
                else
                {
                    AddToken(SLASH);
                }

                break;
            // Whitespace
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;
            // String literals
            case '"':
                @String();
                break;
            // Numbers
            case var x when IsDigit(x):
                @Number();
                break;
            // Identifiers
            case var x when IsAlpha(x):
                @Identifier();
                break;
            default:
                Lox.Error(line, "Unexpected character.");
                break;
        }
    }

    private void AddToken(TokenType type, object? literal = null) =>
        tokens.Add(new Token(type, source.Substring(start, current - start), literal, line));

    private bool Match(char expected)
    {
        if (IsAtEnd) return false;
        if (source[current] != expected) return false;
        current++;
        return true;
    }

    private char Peek() =>
        IsAtEnd ? '\0' : source[current];

    private char PeekNext() =>
        current + 1 >= source.Length ? '\0' : source[current + 1];

    private char Advance() =>
        source[current++];

    private bool IsAtEnd =>
        current >= source.Length;

    private bool IsDigit(char c) =>
        c is >= '0' and <= '9';

    private bool IsAlpha(char c) =>
        c is >= 'a' and <= 'z' ||
        c is >= 'A' and <= 'Z' ||
        c is '_';

    private bool IsAlphaNumeric(char c) =>
        IsAlpha(c) || IsDigit(c);

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd)
        {
            Lox.Error(line, "Unterminated string.");
            return;
        }

        // The closing "
        Advance();

        AddToken(STRING, source.Substring(start + 1, current - start - 1));
    }

    private void Number()
    {
        while (IsDigit(Peek())) Advance();

        // Look for a fractional part.
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            // consume the .
            Advance();

            while (IsDigit(Peek())) Advance();
        }

        AddToken(NUMBER, Double.Parse(source.Substring(start, current - start)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = source.Substring(start, current - start);
        AddToken(IdentifiersHelper.Get(text) ?? IDENTIFIER);
    }
}