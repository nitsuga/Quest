using System;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class SearchHit
    {
        public LocationDocument l;
        public double s;

        public override string ToString()
        {
            return l.indextext;
        }
    }
}