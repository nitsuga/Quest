/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{

    public enum SearchMode
    {
        EXACT,
        RELAXED,
        FUZZY
    }

    public enum SearchResultDisplayGroup
    {
        none = 1,
        description = 2,
        thoroughfare = 3,
        type = 4
    }
}