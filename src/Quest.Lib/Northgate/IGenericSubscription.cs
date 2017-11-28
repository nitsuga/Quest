namespace Quest.Lib.Northgate
{
    public interface IGenericSubscription
    {
        string wksta { get; set; }
        string subtype { get; set; }
        double e { get; set; }
        double n { get; set; }
    }
}


