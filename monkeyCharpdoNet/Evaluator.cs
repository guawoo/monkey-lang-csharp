using System.Collections.Generic;

namespace mokeycsharp
{
    public class Evaluator
    {
        private readonly MNull NIL = new MNull();
        private readonly MBool TRUE = new MBool() {value = true};
        private readonly MBool FALSE = new MBool() {value = false};


        public MObject Eval(Node node, Environment env)
        {

            if (node == null) return null;
            
            var t = node.GetType();
            if (t == typeof(Program))
            {
                return EvalProgram((Program) node, env);
            }
            else if (t == typeof(BlockStatement))
            {
                return EvalBlockStatement((BlockStatement) node, env);
            }
            else if (t == typeof(ExpressionStatement))
            {
                return Eval(((ExpressionStatement) node).Expression, env);
            }
            else if (t == typeof(ReturnStatement))
            {
                var v = Eval(((ReturnStatement) node).ReturnValue, env);
                if (IsError(v))
                {
                    return v;
                }

                return new ReturnValue() {value = v};
            }
            else if (t == typeof(LetStatement))
            {
                var v = Eval(((LetStatement) node).Value, env);
                if (IsError(v))
                {
                    return v;
                }

                env.Set(((LetStatement) node).Name.Value, v);
            }
            else if (t == typeof(IntegerLiteral))
            {
                return new MInt() {value = ((IntegerLiteral) node).Value};
            }
            else if (t == typeof(Boolean))
            {
                return NativeBoolToMBool(((Boolean) node).Value);
                
            }else if (t == typeof(PrefixExpression))
            {
                var right = Eval(((PrefixExpression) node).Right, env);
                if (IsError(right))
                {
                    return right;
                }

                return EvalPrefixExpression(((PrefixExpression) node).Operator, right, env);
            }else if (t == typeof(InfixExpression))
            {
                var left = Eval(((InfixExpression) node).Left, env);
                if (IsError(left))
                {
                    return left;
                }

                var right = Eval(((InfixExpression) node).Right, env);
                if (IsError(right))
                {
                    return right;
                }

                return EvalInfixExpression(((InfixExpression) node).Operator, left, right, env);

            }else if (t == typeof(IfExpression))
            {
                return EvalIfExpression((IfExpression) node, env);
            }else if (t == typeof(Identifier))
            {
                return EvalIdentifier((Identifier) node, env);
            }else if (t == typeof(FunctionLiteral))
            {
                var pas = ((FunctionLiteral) node).Parameters;
                var body = ((FunctionLiteral) node).Body;
                return new Function() {parameters = pas, body = body, env = env};
            }else if (t == typeof(CallExpression))
            {
                var func = Eval(((CallExpression) node).Function, env);
                if (IsError(func))
                {
                    return func;
                }

                var args = EvalExpression(((CallExpression) node).Arguments, env);
                if (args.Length == 1 && IsError(args[0]))
                {
                    return args[0];
                }

                return ApplyFunction(func, args);
            }

            return null;
        }

        private MObject ApplyFunction(MObject fn, MObject[] args)
        {
            var function = (Function) fn;

            var extendedEnv = ExtendFunctionEnv(function, args);

            var evaluated = Eval(function.body, extendedEnv);
            return UnwrapReturnValue(evaluated);
        }

        private MObject UnwrapReturnValue(MObject obj)
        {

            if (obj.GetType() == typeof(ReturnValue))
            {
                return ((ReturnValue) obj).value;
            }
            
            return obj;
        }

        private Environment ExtendFunctionEnv(Function fn, MObject[] args)
        {
            var env = Environment.NewEnclosedEnvironment(fn.env);

            foreach (var param in fn.parameters)
            {
                int index = fn.parameters.IndexOf(param);
                env.Set(param.Value, args[index]);
            }

            return env;
        }

        private MObject[] EvalExpression(List<Expression> exps, Environment env)
        {
            var rs = new List<MObject>();
            foreach (var e in exps)
            {
                var evaluated = Eval(e, env);
                if (IsError(evaluated))
                {
                    return new MObject[]{evaluated};
                }

                rs.Add(evaluated);
            }

            return rs.ToArray();
        }

        private MObject EvalIdentifier(Identifier node, Environment env)
        {
            var val = env.Get(node.Value);

            if (val == null)
            {
                return NewError(string.Format("identifier not found: " + node.Value));
            }

            return val;
        }

        private MObject EvalIfExpression(IfExpression node, Environment env)
        {
            var condition = Eval(node.Condition, env);
            if (IsError(condition))
            {
                return condition;
            }

            if (IsTruthy(condition))
            {
                return Eval(node.Consequence, env);
            }else if (node.Alternative != null)
            {
                return Eval(node.Alternative, env);
            }
            else
            {
                return NIL;
            }
        }

        private bool IsTruthy(MObject obj)
        {
            if (obj == TRUE)
            {
                return true;
            }else if (obj == FALSE)
            {
                return false;
            }else if (obj == NIL)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private MObject EvalInfixExpression(string op, MObject left, MObject right, Environment env)
        {
            if (left.MType() == ObjectType.INTEGER_OBJ && right.MType() == ObjectType.INTEGER_OBJ)
            {
                return EvalIntegerInfixExpression(op, left, right);
            }else if (op == "==")
            {
                return NativeBoolToMBool(left == right);
                
            }else if (op == "!=")
            {
                return NativeBoolToMBool(left != right);
            }else if (left.MType()!=right.MType())
            {
                return NewError(string.Format("type mismatch: %s %s %s", left.MType(), op, right.MType()));
            }
            else
            {
                return NewError(string.Format("unknown operator: %s %s %s", left.MType(), op, right.MType()));
            }
            
            
        }

        private MObject EvalIntegerInfixExpression(string op, MObject left, MObject right)
        {
            var lv = ((MInt) left).value;
            var rv = ((MInt) right).value;
            switch (op)
            {
                case "+":
                    return new MInt() {value = lv + rv};
                case "-":
                    return new MInt() {value = lv - rv};
                case "*":
                    return new MInt() {value = lv * rv};
                case "/":
                    return new MInt() {value = lv / rv};
                case ">":
                    return NativeBoolToMBool(lv > rv);
                case "<":
                    return NativeBoolToMBool(lv < rv);
                case "==":
                    return NativeBoolToMBool(lv == rv);
                case "!=":
                    return NativeBoolToMBool(lv != rv);
                default:
                    return NewError(string.Format("unknown operator: %s %s %s", left.MType(), op, right.MType()));
            }
        }

        private MObject EvalPrefixExpression(string op, MObject right, Environment env)
        {
            switch (op)
            {
                case "!":
                    return EvalBangOperatorExpression(right);
                case "-":
                    return EvalMinusPrefixOperatorExpression(right);
                default:
                    return NewError(string.Format("unknown operator: %s%s", op, right.MType()));
                    
            }
        }

        private MObject EvalMinusPrefixOperatorExpression(MObject right)
        {
            if (right.MType() != ObjectType.INTEGER_OBJ)
            {
                return NewError(string.Format("unknown operator: -%s", right.MType()));
            }

            var val = ((MInt) right).value;
            return new MInt() {value = -val};
        }

        private MObject EvalBangOperatorExpression(MObject right)
        {
            if (right == TRUE)
            {
                return FALSE;
            }else if (right == FALSE)
            {
                return TRUE;
            }else if (right == NIL)
            {
                return TRUE;
            }
            else
            {
                return FALSE;
            }
        }

        private MBool NativeBoolToMBool(bool value)
        {
            if (value)
            {
                return TRUE;
            }
            else
            {
                return FALSE;
            }
        }

        private MObject EvalBlockStatement(BlockStatement block, Environment env)
        {
            MObject rs = null;
            foreach (var statement in block.Statements)
            {
                rs = Eval(statement, env);
                if (rs != null)
                {
                    var t = rs.MType();
                    if (t == ObjectType.RETURN_VALUE_OBJ || t == ObjectType.ERROR_OBJ)
                    {
                        return rs;
                    }
                }
            }

            return rs;
        }

        private MObject EvalProgram(Program program, Environment env)
        {
            MObject rs = null;
            foreach (var statement in program.Statements)
            {
                rs = Eval(statement, env);

                if (rs == null)
                {
                    return rs;
                }
                
                var t = rs.GetType();

                if (t == typeof(ReturnValue))
                {
                    return ((ReturnValue) rs).value;
                }
                else if (t == typeof(Error))
                {
                    return rs;
                }
            }

            return rs;
        }

        private bool IsError(MObject obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.MType() == ObjectType.ERROR_OBJ;
        }

        private Error NewError(string msg)
        {
            return new Error() {message = msg};
        }
    }
}