using System.Collections.Generic;

namespace mokeycsharp
{
    public class Token
    {
        public string Literal;
        public TokenType Type;

        public Token(TokenType tt, string lt)
        {
            Type = tt;
            Literal = lt;
        }


        static Dictionary<string, TokenType> KeyWords = new Dictionary<string, TokenType>()
        {
            {"fn", TokenType.FUNCTION },
            {"let", TokenType.LET },
            {"true", TokenType.TRUE },
            {"false", TokenType.FALSE },
            {"if", TokenType.IF },
            {"else", TokenType.ELSE },
            {"return", TokenType.RETURN }
        };

        public static TokenType LookupIdent(string ident)
        {
            if (KeyWords.ContainsKey(ident))
            {
                return KeyWords[ident];
            }
            return TokenType.IDENT;
        }
    }

    public enum TokenType: int
    {
        ILLEGAL,
        EOF,

        // Identifiers + literals
        IDENT,
        INT,

        // Operators
        ASSIGN,
        PLUS,
        MINUS,
        BANG,
        ASTERISK,
        SLASH,
        EQ,
        NOT_EQ,

        LT,
        GT,

        // Delimiters
        COMMA,
        SEMICOLON,

        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,

        // Keywords
        FUNCTION,
        LET,
        TRUE,
        FALSE,
        IF,
        ELSE,
        RETURN
    }

}
