using static CraftingInterpreters.Lox.TokenType;

namespace CraftingInterpreters.Lox;

public class Scanner(string source)
{
    private int _current;
    private int _line = 1;
    private int _start;

    private List<Token> _tokens = [];

    private bool IsAtEnd =>
        _current >= source.Length;

    public List<Token> ScanTokens()
    {
        _start = 0;
        _current = 0;
        _line = 1;
        _tokens = [];

        while (!IsAtEnd)
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(EOF, "", null!, _line));
        return _tokens;
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
                    // A comment goes until the end of the line.
                    while (Peek() != '\n' && !IsAtEnd)
                        Advance();
                else
                    AddToken(SLASH);

                break;
            // Whitespace
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                _line++;
                break;
            // String literals
            case '"':
                String();
                break;
            // Numbers
            case var x when IsDigit(x):
                Number();
                break;
            // Identifiers
            case var x when IsAlpha(x):
                Identifier();
                break;
            default:
                LoxRunner.Error(_line, "Unexpected character.");
                break;
        }
    }

    private void AddToken(TokenType type, object? literal = null) =>
        _tokens.Add(new Token(type, source.Substring(_start, _current - _start), literal!, _line));

    private bool Match(char expected)
    {
        if (IsAtEnd) return false;
        if (source[_current] != expected) return false;
        _current++;
        return true;
    }

    private char Peek() =>
        IsAtEnd ? '\0' : source[_current];

    private char PeekNext() =>
        _current + 1 >= source.Length ? '\0' : source[_current + 1];

    private char Advance() =>
        source[_current++];

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
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd)
        {
            LoxRunner.Error(_line, "Unterminated string.");
            return;
        }

        // The closing "
        Advance();

        AddToken(STRING, source.Substring(_start + 1, _current - _start - 2));
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

        AddToken(NUMBER, double.Parse(source.Substring(_start, _current - _start)));
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = source.Substring(_start, _current - _start);
        AddToken(IdentifiersHelper.Get(text) ?? IDENTIFIER);
    }
}