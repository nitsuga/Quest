using System.Collections.Generic;

namespace Quest.Lib.Northgate
{
    public interface IXCMessageParser
    {
        List<string> GetGenericSubscriptions(string subtype);
        List<string> GetGenericDeletions(string subtype);
    }
}


