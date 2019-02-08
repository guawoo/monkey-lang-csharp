using System;

namespace mokeycsharp
{
    public class Repl
    {
        public static void Start()
        {
            var env = new Environment();
            var eval = new Evaluator();
            
            while (true)
            {
                Console.Write(">>");
                
                var input = Console.ReadLine();
                if (input == null)
                {
                    return;
                }
                
                var lexer = new Lexer(input);
                var parser = new Parser(lexer);

                var program = parser.ParseProgram();

                if (parser.Errors().Length > 0)
                {
                    foreach (var error in parser.Errors())
                    {
                        Console.WriteLine(error);
                        continue;
                    }
                }

                //Console.WriteLine(program.ToString());

                //eval 



                var rs = eval.Eval(program, env);
                if (rs != null)
                {
                    Console.WriteLine(rs.Inspect());
                }
            }
        }
    }
}