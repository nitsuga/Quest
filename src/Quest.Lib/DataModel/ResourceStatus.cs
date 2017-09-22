﻿using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class ResourceStatus
    {
        public ResourceStatus()
        {
            Devices = new HashSet<Devices>();
            ResourceResourceStatus = new HashSet<Resource>();
            ResourceResourceStatusPrev = new HashSet<Resource>();
            ResourceStatusHistory = new HashSet<ResourceStatusHistory>();
        }

        public int ResourceStatusId { get; set; }
        public string Status { get; set; }
        public bool? Available { get; set; }
        public bool? Busy { get; set; }
        public bool? Rest { get; set; }
        public bool? Offroad { get; set; }
        public bool? NoSignal { get; set; }
        public bool? BusyEnroute { get; set; }

        public ICollection<Devices> Devices { get; set; }
        public ICollection<Resource> ResourceResourceStatus { get; set; }
        public ICollection<Resource> ResourceResourceStatusPrev { get; set; }
        public ICollection<ResourceStatusHistory> ResourceStatusHistory { get; set; }
    }
}