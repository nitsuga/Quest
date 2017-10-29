namespace Quest.Common.Messages.Visual
{
    public class VisualPersist
    {
        public VisualPersist()
        {
        }
        
        public VisualId Id { get; set; }
        
        public byte[] Timeline { get; set; }
        
        public string GeoJson;
    }
}