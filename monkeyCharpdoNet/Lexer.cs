using System.Text;

namespace mokeycsharp
{
    public class Lexer
    {
        public string inputText;
        private int position;
        private int readPosition;

        private char ch;

        public Lexer(string input)
        {
            inputText = input;
            ReadChar();
        }

        private void ReadChar()
        {
            if (readPosition >= inputText.Length)
            {
                ch = '\0';
            }
            else
            {
                ch = inputText[readPosition];
            }

            position = readPosition;
            readPosition += 1;
        }

        private char PeekChar()
        {
            if (readPosition >= inputText.Length)
            {
                return '\0';
            }
            else
            {
                return inputText[readPosition];
            }
        }

        private string ReadIdentifier()
        {

            int startPos = position;

            while (IsLetter(ch))
            {
                ReadChar();
            }

            return inputText.Substring(startPos, position - startPos);
        }

        private string ReadNumber()
        {

            int startPos = position;

            while (IsDigit(ch))
            {
                ReadChar();
            }

            return inputText.Substring(startPos, position - startPos);
        }


        public Token NextToken()
        {
            Token token;

            SkipWhiteSpace();

            switch (ch)
            {
                case '=':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        token = new Token(TokenType.EQ, "==");
                    }
                    else
                    {
                        token = new Token(TokenType.ASSIGN, "=");
                    }
                    break;
                case '+':
                    token = new Token(TokenType.PLUS, "+");
                    break;
                case '-':
                    token = new Token(TokenType.MINUS, "-");
                    break;
                case '*':
                    token = new Token(TokenType.ASTERISK, "*");
                    break;
                case '/':
                    token = new Token(TokenType.SLASH, "/");
                    break;
                case '!':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        token = new Token(TokenType.NOT_EQ, "!=");
                    }
                    else
                    {
                        token = new Token(TokenType.BANG, "!");
                    }
                    break;
                case '<':
                    token = new Token(TokenType.LT, "<");
                    break;
                case '>':
                    token = new Token(TokenType.GT, ">");
                    break;
                case ';':
                    token = new Token(TokenType.SEMICOLON, ";");
                    break;
                case '(':
                    token = new Token(TokenType.LPAREN, "(");
                    break;
                case ')':
                    token = new Token(TokenType.RPAREN, ")");
                    break;
                case ',':
                    token = new Token(TokenType.COMMA, ",");
                    break;
                case '{':
                    token = new Token(TokenType.LBRACE, "{");
                    break;
                case '}':
                    token = new Token(TokenType.RBRACE, "}");
                    break;
                case '\0':
                    token = new Token(TokenType.EOF, "EOF");
                    break;
                default:
                    if (IsLetter(ch))
                    {
                        string ltl = ReadIdentifier();
                        token = new Token(Token.LookupIdent(ltl), ltl);
                        return token;

                    }
                    else if (IsDigit(ch))
                    {
                        string ltl = ReadNumber();
                        token = new Token(TokenType.INT, ltl);
                        return token;
                    }
                    else
                    {
                        token = new Token(TokenType.ILLEGAL, ch.ToString());
                    }
                    break;
            }
            ReadChar();
            return token;

        }

        private void SkipWhiteSpace()
        {
            while (ch==' ' || ch == '\t' || ch == '\n' || ch == '\r')
            {
                ReadChar();
            }
        }

        private bool IsLetter(char c)
        {
            return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_';
        }

        private bool IsDigit(char c)
        {
            return '0' <= ch && ch <= '9';
        }
    }
}
