using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    public class QuestDocument
    {
        [Text(Index = false)]
        public string ID { get; set; }
    }
}