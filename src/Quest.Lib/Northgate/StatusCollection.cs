using System.Collections.Generic;
using Quest.Common.Messages.CAD;

namespace Quest.Lib.Northgate
{
    class StatusCollection
    {
        // Declare an array to store the data elements.
        private Dictionary<string, XCChannelStatus> _status = new Dictionary<string, XCChannelStatus>();

        // Define the indexer to allow client code to use [] notation.

        public XCChannelStatus this[string name]
        {
            get
            {
                if (_status.ContainsKey(name))
                    return _status[name];
                else
                    return null;
            }
            set
            {
                if (_status.ContainsKey(name))
                    _status[name] = value;
                else
                    _status.Add(name, value);
            }
        }
    }
}
