using System;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class AggregateItem
    {
        public string Name { get; set; }
        public long? Value { get; set; }
    }
}