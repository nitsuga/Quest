namespace Quest.Lib.Northgate
{

    public partial class XCConnector
    {
        /// <summary>
        /// holds a single generic subscription
        /// </summary>
        /// <remarks></remarks>
        public class GenericSubscription : IGenericSubscription
        {

            private string _wksta;
            private string _subtype;
            private double _e;

            private double _n;

            public double e
            {
                get { return _e; }
                set { _e = value; }
            }

            public double n
            {
                get { return _n; }
                set { _n = value; }
            }

            public string subtype
            {
                get { return _subtype; }
                set { _subtype = value; }
            }

            public string wksta
            {
                get { return _wksta; }
                set { _wksta = value; }
            }
        }

    }
}
