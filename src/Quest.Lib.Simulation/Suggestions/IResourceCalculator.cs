////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////

namespace Quest.Lib.Suggestions
{
    /// <summary>
    /// Generaic interface for calculating the number and type of resources that need to attend the given incident
    /// </summary>
    public interface IResourceCalculator
    {
        void CalculateResourceRequired(IncidentView inc);
    }
}
