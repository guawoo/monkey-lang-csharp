using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mokeycsharp
{
    public abstract class Node
    {
        public abstract string TokenLiteral();
    }

    public abstract class Statement: Node
    {
        
    }

    public abstract class Expression: Node
    {
        
    }


    public class Program: Node
    {
        internal List<Statement> Statements { get; } = new List<Statement>();

        public override string TokenLiteral()
        {
            return "Program";
        }

        public void Append(Statement stmt)
        {
            Statements.Add(stmt);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var item in Statements)
            {
                sb.Append(item.ToString());
            }
            return sb.ToString();
        }
    }

    public class LetStatement : Statement
    {

        public Token Token { get; set; }
        public Identifier Name { get; set; }
        public Expression Value { get; set; }

       

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(TokenLiteral())
                .Append(" ")
                .Append(Name.ToString())
                .Append(" = ");

            if (Value != null)
            {
                sb.Append(Value.ToString());
            }

            sb.Append(";");

            return sb.ToString();
        }
    }

    public class ReturnStatement : Statement
    {
        public Token Token { get; set; }
        public Expression ReturnValue { get; set; }
        
         
        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(TokenLiteral())
                .Append(" ");

            if (ReturnValue != null)
            {
                sb.Append(ReturnValue.ToString());
            }

            sb.Append(";");

            return sb.ToString();
        }
    }

    public class ExpressionStatement: Statement
    {
        public Token Token { get; set; }
        public Expression Expression;

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            if (Expression != null)
            {
                return Expression.ToString();
            }

            return "";
        }
    }


    public class Identifier: Expression
    {
        public Token Token { get; set; }
        public string Value { get; set; }

        public Identifier(Token t, string v)
        {
            Token = t;
            Value = v;
        }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class IntegerLiteral: Expression
    {
        public Token Token { get; set; }
        public int Value { get; set; }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            return Token.Literal;
        }
    }

    public class PrefixExpression: Expression
    {
        public Token Token { get; set; }
        public string Operator { get; set; }
        public Expression Right { get; set; }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(")
                .Append(Operator)
                .Append(Right.ToString())
                .Append(")");

            return sb.ToString();
        }
    }

    public class InfixExpression: Expression
    {
        public Token Token { get; set; }
        public Expression Left { get; set; }
        public string Operator { get; set; }
        public Expression Right { get; set; }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("(")
                .Append(Left.ToString())
                .Append(" " + Operator + " ")
                .Append(Right.ToString())
                .Append(")");

            return sb.ToString();
        }
    }

    public class Boolean: Expression
    {
        public Token Token { get; set; }
        public bool Value { get; set; }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            return Token.Literal;
        }
    }

    public class BlockStatement: Statement
    {
        public Token Token { get; set; }
        internal List<Statement> Statements { get; }= new List<Statement>();

        public void Append(Statement stmt)
        {
            Statements.Add(stmt);
        }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            Console.WriteLine("blockstatements has {0} nodes", Statements.Count);

            foreach (var item in Statements)
            {
                sb.Append(item.ToString());
            }

            return sb.ToString();
        }
    }


    public class IfExpression: Expression
    {
        public Token Token { get; set; }
        public Expression Condition { get; set; }

        public BlockStatement Consequence { get; set; }
        public BlockStatement Alternative { get; set; }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("if")
                .Append(Condition.ToString())
                .Append(" ")
                .Append(Consequence.ToString());

            if (Alternative != null)
            {
                sb.Append("else")
                    .Append(Alternative.ToString());
            }

            return sb.ToString();
        }
    }

    public class FunctionLiteral: Expression
    {
        public Token Token { get; set; }
        public List<Identifier> Parameters = new List<Identifier>();
        public BlockStatement Body { get; set; }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Token.Literal)
                .Append("(");

            var strs = new List<string>();

            if (Parameters.Count > 0)
            {
                foreach (var item in Parameters)
                {
                    strs.Add(item.ToString());
                }
            }

            sb.Append(string.Join(",", strs.ToArray()));
                

            sb.Append(")")
                .Append(Body.ToString());

            return sb.ToString();
        }
    }

    public class CallExpression: Expression
    {
        public Token Token { get; set; }
        public Expression Function { get; set; }
        public List<Expression> Arguments { get; set; }

        public override string TokenLiteral()
        {
            return Token.Literal;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Function.ToString())
                .Append("(");

            var strs = new List<string>();

            if (Arguments.Count > 0)
            {
                foreach (var item in Arguments)
                {
                    strs.Add(item.ToString());
                }
            }

            sb.Append(string.Join(",", strs.ToArray()));


            sb.Append(")");

            return sb.ToString();
        }
    }
}
