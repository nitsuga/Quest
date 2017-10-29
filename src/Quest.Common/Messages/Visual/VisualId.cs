namespace Quest.Common.Messages.Visual
{
    public class VisualId
    {        
        public string Source { get; set; }
        
        public string Name { get; set; }
        
        public string Id { get; set; }
        
        public string VisualType { get; set; }

        public override string ToString()
        {
            return $"{Source} {Name} {Id} {VisualType}";
        }
    }
}