using System;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class InfoSearchRequest : Request
    {
        public double lat;
        public double lng;
    }
}