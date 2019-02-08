
using System;
using System.Collections.Generic;

namespace mokeycsharp
{
    class P1
    {
        static void Main(string[] args)
        {
            //var ll = new Lexer("fn(name, age){let k=1;} add(5+9,8);let j=fn(x){};");
            //while (true)
            //{
            //    var tok = ll.NextToken();
            //    Console.WriteLine("type: {0}, literal: {1}", tok.Type, tok.Literal);

            //    if (tok.Type == TokenType.EOF)
            //    {
            //        break;
            //    }
            //}
        
            Repl.Start();

//            var fun = new Function();
//            Console.WriteLine(fun.GetType());
           
        }
    }
}
