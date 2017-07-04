using System;
using System.Collections.Generic;
using System.Linq;
using Quest.Lib.DataModel;

namespace Quest.Lib.Simulation
{
    [Export]
    public class Parameters : List<ProfileParameter>
    {
        public void RemoveParameter(string Name)
        {
            var result = (from s in this where s.ProfileParameterType.Name == Name select s).FirstOrDefault();
            if (result != null)
                Remove(result);
        }

        public void SetParameter(string Name, string value)
        {
            var param = (from s in this where s.ProfileParameterType.Name == Name select s).FirstOrDefault();
            if (param != null)
                param.Value = value;
        }

        public string GetFirstParameter(string Name, string defaultValue)
        {
            var result = (from s in this where s.ProfileParameterType.Name == Name select s.Value).FirstOrDefault();
            if (result == null)
                return defaultValue;
            return result;
        }

        public double GetFirstParameter(string Name, double defaultValue)
        {
            var result = (from s in this where s.ProfileParameterType.Name == Name select s.Value).FirstOrDefault();
            if (result == null)
                return defaultValue;
            var value = defaultValue;
            double.TryParse(result, out value);
            return value;
        }

        public int GetFirstParameter(string Name, int defaultValue)
        {
            var result = (from s in this where s.ProfileParameterType.Name == Name select s.Value).FirstOrDefault();
            if (result == null)
                return defaultValue;
            var value = defaultValue;
            int.TryParse(result, out value);
            return value;
        }

        public bool GetFirstParameter(string Name, bool defaultValue)
        {
            var result = (from s in this where s.ProfileParameterType.Name == Name select s.Value).FirstOrDefault();
            if (result == null)
                return defaultValue;
            var value = defaultValue;
            bool.TryParse(result, out value);
            return value;
        }

        public DateTime GetFirstParameter(string Name, DateTime defaultValue)
        {
            var result = (from s in this where s.ProfileParameterType.Name == Name select s.Value).FirstOrDefault();
            if (result == null)
                return defaultValue;
            var value = defaultValue;
            DateTime.TryParse(result, out value);
            return value;
        }
    }
}