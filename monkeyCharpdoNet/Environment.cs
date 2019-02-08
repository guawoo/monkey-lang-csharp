using System.Collections.Generic;
using System.Linq;

namespace mokeycsharp
{
    public class Environment
    {
        private Dictionary<string, MObject> store = new Dictionary<string, MObject>();
        private Environment outer;

        public MObject Get(string name)
        {
            var obj = store[name];
            return obj;
        }

        public MObject Set(string name, MObject obj)
        {
            if (store.ContainsKey(name))
            {
                store[name] = obj;
            }
            else
            {
                store.Add(name, obj);
            }
            
            return obj;
        }

        public static Environment NewEnclosedEnvironment(Environment outer)
        {
            var env = new Environment {outer = outer};
            return env;
        }
    }
}