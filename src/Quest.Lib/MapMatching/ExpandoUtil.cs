using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Quest.Lib.MapMatching
{
    public static class ExpandoUtil
    {
        /// <summary>
        /// sets the fields of a target object from parameters in an dynamic expando object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T SetFromExpando<T>(ExpandoObject parameters) where T : new()
        {
            var o = new T();

            foreach (var field in o.GetType().GetFields())
            {
                var param = parameters.FirstOrDefault(x => x.Key == field.Name);
               // if (param.Key != field.Name)
               //     throw new ApplicationException($"Parameter {field.Name} of type {field.FieldType.Name} not passed");
               if (param.Key!=null)
                field.SetValue(o, param.Value);
            }

            return o;

        }

        public static dynamic SetToExpando<T>(T parameters) where T : class
        {
            var result = new ExpandoObject();
            var dictionary = ((IDictionary<string, object>)result);

            foreach (var field in parameters.GetType().GetFields())
            {
                dictionary.Add(new KeyValuePair<string, object>(field.Name, field.GetValue(parameters)));
            }

            return result;

        }
    }
}
