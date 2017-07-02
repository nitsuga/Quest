using System;
using System.Collections.Generic;

namespace Quest.Lib.MapMatching.HMMViterbi
{
    public class KeyEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, object> _keyExtractor;

        public KeyEqualityComparer(Func<T, object> keyExtractor)
        {
            _keyExtractor = keyExtractor;
        }

        public bool Equals(T x, T y)
        {
            return _keyExtractor(x).Equals(_keyExtractor(y));
        }

        public int GetHashCode(T obj)
        {
            return _keyExtractor(obj).GetHashCode();
        }
    }
}