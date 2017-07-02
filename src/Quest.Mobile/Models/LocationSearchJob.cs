using System;
using System.Collections.Generic;

namespace Quest.Mobile.Models
{
    [Serializable]
    public class LocationSearchJob
    {
        // ReSharper disable once InconsistentNaming
        public int jobid;
        // ReSharper disable once InconsistentNaming
        public List<SingleSearch> request;
        // ReSharper disable once InconsistentNaming
        public bool cancelflag;
        // ReSharper disable once InconsistentNaming
        public bool complete;
    }
}