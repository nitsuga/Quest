////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////
using System;

namespace Quest.Lib.Suggestions

{
    public partial class AssignedResource : ICloneable 
    {
        public DateTime? Convey { get; set; }
        public DateTime? Dispatched { get; set; }
        public DateTime? Enroute { get; set; }
        public DateTime? Hospital { get; set; }
        public DateTime? Onscene { get; set; }
        public DateTime? Released { get; set; }
        public ResourceView Resource { get; set; }

        public object Clone()
        {
            AssignedResource i = new AssignedResource()
            {
                Convey = this.Convey,
                Dispatched = this.Dispatched,
                Enroute = this.Enroute,
                Hospital = this.Hospital,
                Onscene = this.Onscene,
                Released = this.Released,
                Resource = this.Resource
            };
            return i;
        }
    }
}
