using System;
using System.Collections.Generic;

namespace mokeycsharp
{
    enum Precedence{
        LOWEST,
        EQUALS,         // ==
        LESSGREATER,    // > or <
        SUM,            // +
        PRODUCT,        // *
        PREFIX,         // -X or !X
        CALL            // myFunc(X)
    }


    delegate Expression PrefixParseFn();
    delegate Expression InfixParseFn(Expression exp);
    class Parser
    {
        private Lexer l;
        private Token curToken;
        private Token peekToken;

        private List<string> errors = new List<string>();

        private Dictionary<TokenType, PrefixParseFn> prefixParseFns = new Dictionary<TokenType, PrefixParseFn>();
        private Dictionary<TokenType, InfixParseFn> infixParseFns = new Dictionary<TokenType, InfixParseFn>();

        private Dictionary<TokenType, Precedence> precedences = new Dictionary<TokenType, Precedence>();

        public Parser(Lexer lexer)
        {
            l = lexer;


            precedences.Add(TokenType.EQ, Precedence.EQUALS);
            precedences.Add(TokenType.NOT_EQ, Precedence.EQUALS);
            precedences.Add(TokenType.LT, Precedence.LESSGREATER);
            precedences.Add(TokenType.GT, Precedence.LESSGREATER);
            precedences.Add(TokenType.PLUS, Precedence.SUM);
            precedences.Add(TokenType.MINUS, Precedence.SUM);
            precedences.Add(TokenType.SLASH, Precedence.PRODUCT);
            precedences.Add(TokenType.ASTERISK, Precedence.PRODUCT);
            precedences.Add(TokenType.LPAREN, Precedence.CALL);

            RegisterPaserFns();

            NextToken();
            NextToken();
        }

        public void NextToken()
        {
            curToken = peekToken;
            peekToken = l.NextToken();
        }

        public string[] Errors()
        {
            return errors.ToArray();
        }

        public void PeekError(TokenType t)
        {
            var msg = string.Format("expected next token to be {0}, got {1} instead", t, peekToken.Type);
            errors.Add(msg);
        }

        private void RegisterPaserFns()
        {
            //prefix
            prefixParseFns.Add(TokenType.IDENT, ParseIdentifier);
            prefixParseFns.Add(TokenType.INT, ParseIntegerLiteral);
            prefixParseFns.Add(TokenType.BANG, ParsePrefixExpression);
            prefixParseFns.Add(TokenType.MINUS, ParsePrefixExpression);
            prefixParseFns.Add(TokenType.TRUE, ParseBoolean);
            prefixParseFns.Add(TokenType.FALSE, ParseBoolean);
            prefixParseFns.Add(TokenType.LPAREN, ParseGroupedExpression);
            prefixParseFns.Add(TokenType.IF, ParseIfExpression);
            prefixParseFns.Add(TokenType.FUNCTION, ParseFunctionLiteral);

            //infix
            infixParseFns.Add(TokenType.PLUS, ParseInfixExpression);
            infixParseFns.Add(TokenType.MINUS, ParseInfixExpression);
            infixParseFns.Add(TokenType.SLASH, ParseInfixExpression);
            infixParseFns.Add(TokenType.ASTERISK, ParseInfixExpression);
            infixParseFns.Add(TokenType.EQ, ParseInfixExpression);
            infixParseFns.Add(TokenType.NOT_EQ, ParseInfixExpression);
            infixParseFns.Add(TokenType.LT, ParseInfixExpression);
            infixParseFns.Add(TokenType.GT, ParseInfixExpression);
            infixParseFns.Add(TokenType.LPAREN, ParseCallExpression);
        }

        private Expression ParseCallExpression(Expression func)
        {
            var expr = new CallExpression();
            expr.Token = curToken;
            expr.Function = func;

            expr.Arguments = ParseCallArguments();
            return expr;
        }

        private List<Expression> ParseCallArguments()
        {
            var args = new List<Expression>();

            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return args;
            }

            NextToken();

            args.Add(ParseExpression(Precedence.LOWEST));

            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken();
                NextToken();

                args.Add(ParseExpression(Precedence.LOWEST));
            }

            if (!ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }

            return args;

        }

        private Expression ParseFunctionLiteral()
        {
            var lit = new FunctionLiteral();
            lit.Token = curToken;

            if (!ExpectPeek(TokenType.LPAREN))
            {
                return null;
            }

            lit.Parameters = ParseFunctionParameters();

            if (! ExpectPeek(TokenType.LBRACE))
            {
                return null;
            }

            lit.Body = ParseBlockStatement();

            return lit;
        }

        private List<Identifier> ParseFunctionParameters()
        {
            var idents = new List<Identifier>();

            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return idents;
            }

            NextToken();

            var id = new Identifier(curToken, curToken.Literal);

            idents.Add(id);

            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken();
                NextToken();

                id = new Identifier(curToken, curToken.Literal);
                idents.Add(id);
            }

            if (!ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }

            return idents;
        }

        private Expression ParseIfExpression()
        {
            var expr = new IfExpression();
            expr.Token = curToken;

            if (!ExpectPeek(TokenType.LPAREN))
            {
                return null;
            }

            NextToken();

            expr.Condition = ParseExpression(Precedence.LOWEST);

            if (!ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }

            expr.Consequence = ParseBlockStatement();

            if (PeekTokenIs(TokenType.ELSE))
            {
                NextToken();

                if (!ExpectPeek(TokenType.LBRACE))
                {
                    return null;
                }

                expr.Alternative = ParseBlockStatement();
            }

            return expr;
        }

        private BlockStatement ParseBlockStatement()
        {
            var block = new BlockStatement();
            block.Token = curToken;

            NextToken();

            while (!CurTokenIs(TokenType.RBRACE))
            {
                var stmt = ParseStatement();

                if (stmt != null)
                {
                    block.Append(stmt);
                }

                NextToken();
            }

            return block;
        }

        private Expression ParseGroupedExpression()
        {
            NextToken();

            var expr = ParseExpression(Precedence.LOWEST);

            if (!ExpectPeek(TokenType.RPAREN))
            {
                return null;
            }

            return expr;
        }

        private Expression ParsePrefixExpression()
        {
            var expr = new PrefixExpression();
            expr.Token = curToken;
            expr.Operator = curToken.Literal;

            NextToken();

            expr.Right = ParseExpression(Precedence.PREFIX);

            return expr;
        }

        public Program ParseProgram()
        {
            var program = new Program();
            while (!CurTokenIs(TokenType.EOF))
            {
                var stmt = ParseStatement();

                if (stmt != null)
                {
                    program.Append(stmt);
                }

                NextToken();
            }

            return program;
        }

        public Statement ParseStatement()
        {
            switch (curToken.Type)
            {
                case TokenType.LET:
                    return ParseLetStatement();
               
                case TokenType.RETURN:
                    return ParseReturnStatement();
                   
                default:
                    return ParseExpressionStatement();
            }
            
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var stmt = new ExpressionStatement();
            stmt.Token = curToken;
            stmt.Expression = ParseExpression(Precedence.LOWEST);

            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;
        }


        private LetStatement ParseLetStatement()
        {
            var stmt = new LetStatement();
            stmt.Token = curToken;

            if (!ExpectPeek(TokenType.IDENT))
            {
                return null;
            }

            stmt.Name = new Identifier(curToken, curToken.Literal);

            if (!ExpectPeek(TokenType.ASSIGN))
            {
                return null;
            }

            NextToken();

            stmt.Value = ParseExpression(Precedence.LOWEST);

            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;
        }

        private ReturnStatement ParseReturnStatement()
        {
            var stmt = new ReturnStatement();
            stmt.Token = curToken;

            NextToken();

            stmt.ReturnValue = ParseExpression(Precedence.LOWEST);

            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;
        }

        private Expression ParseExpression(Precedence p)
        {
            PrefixParseFn prefix;
            try
            {
                prefix = prefixParseFns[curToken.Type];
            }catch (KeyNotFoundException)
            {
                NoPrefixParseFnError(curToken.Type);
                return null;
            }


            var leftExp = prefix();

            while (!PeekTokenIs(TokenType.SEMICOLON) && p < PeekPrecedence())
            {
                InfixParseFn infix;
                try
                {
                    infix = infixParseFns[peekToken.Type];
                }
                catch (KeyNotFoundException)
                {

                    return leftExp;
                }

                NextToken();

                leftExp = infix(leftExp);
            }
            return leftExp;
        }

        private Expression ParseInfixExpression(Expression left)
        {
            var expr = new InfixExpression();
            expr.Token = curToken;
            expr.Operator = curToken.Literal;
            expr.Left = left;

            var precedence = CurPrecedence();
            NextToken();
            expr.Right = ParseExpression(precedence);

            return expr;
        }

        private Precedence PeekPrecedence()
        {
            Precedence pre;
            try
            {
                pre = precedences[peekToken.Type];
            }
            catch (KeyNotFoundException)
            {
                return Precedence.LOWEST;
            }
            return pre;
        }

        private Precedence CurPrecedence()
        {
            Precedence pre;
            try
            {
                pre = precedences[curToken.Type];
            }
            catch (KeyNotFoundException)
            {
                return Precedence.LOWEST;
            }
            return pre;
        }

        public bool CurTokenIs(TokenType t)
        {
            return curToken.Type == t;
        }

        public bool PeekTokenIs(TokenType t)
        {
            return peekToken.Type == t;
        }

        public bool ExpectPeek(TokenType t)
        {
            if (PeekTokenIs(t))
            {
                NextToken();
                return true;
            }else
            {
                PeekError(t);
                return false;
            }
        }

        private Expression ParseIdentifier()
        {
            return new Identifier(curToken, curToken.Literal);
        }

        private Expression ParseIntegerLiteral()
        {
            var lit = new IntegerLiteral();
            lit.Token = curToken;
            lit.Value = int.Parse(curToken.Literal);

            return lit;
        }

        private Expression ParseBoolean()
        {
            var b = new Boolean();
            b.Token = curToken;
            b.Value = CurTokenIs(TokenType.TRUE);

            return b;
        }

        private void NoPrefixParseFnError(TokenType ty)
        {
            var msg = string.Format("no prefix parse function for {0} found", ty);
            errors.Add(msg);
        }
    }
}
