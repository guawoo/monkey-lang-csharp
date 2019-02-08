using System.Collections.Generic;
using System.Text;

namespace mokeycsharp
{
    public enum ObjectType
    {
        INTEGER_OBJ,
        BOOLEAN_OBJ,
        NULL_OBJ, 
        RETURN_VALUE_OBJ, 
        FUNCTION_OBJ,
        ERROR_OBJ
    }
    
    
    public abstract class MObject
    {
        public abstract ObjectType MType();
        public abstract string Inspect();
    }

    public class MInt : MObject
    {
        internal int value;
        
        public override ObjectType MType()
        {
            return ObjectType.INTEGER_OBJ;
        }

        public override string Inspect()
        {
            return value.ToString();
        }
    }

    public class MBool : MObject
    {
        internal bool value;


        public override ObjectType MType()
        {
            return ObjectType.BOOLEAN_OBJ;
        }

        public override string Inspect()
        {
            return value.ToString();
        }
    }

    public class MNull : MObject
    {
        public override ObjectType MType()
        {
            return ObjectType.NULL_OBJ;
        }

        public override string Inspect()
        {
            return "NuLL";
        }
    }

    public class ReturnValue : MObject
    {
        internal MObject value;
        public override ObjectType MType()
        {
            return ObjectType.RETURN_VALUE_OBJ;
        }

        public override string Inspect()
        {
            return value.Inspect();
        }
    }

    public class Function : MObject
    {
        internal List<Identifier> parameters = new List<Identifier>();
        internal BlockStatement body;
        internal Environment env;


        public override ObjectType MType()
        {
            return ObjectType.FUNCTION_OBJ;
        }

        public override string Inspect()
        {
            var sb = new StringBuilder();

            sb.Append("fn")
                .Append("(")
                .Append(string.Join(",", parameters))
                .Append(") {\n")
                .Append(body.ToString())
                .Append("\n}");

            return sb.ToString();
        }
    }

    public class Error : MObject
    {
        internal string message;


        public override ObjectType MType()
        {
            return ObjectType.ERROR_OBJ;
        }

        public override string Inspect()
        {
            return "ERROR: " + message;
        }
    }
}