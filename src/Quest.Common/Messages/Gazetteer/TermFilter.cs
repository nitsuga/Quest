using System;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class TermFilter
    {
        public string field;
        public bool include;
        public string value;
    }
}