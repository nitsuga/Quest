using System;
using System.Collections.Generic;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class Aggregate
    {
        public List<AggregateItem> Items;
        public string Name { get; set; }
    }
}