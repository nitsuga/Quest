using System;

namespace Quest.Common.Messages.Entities
{
    /// <summary>
    ///     Request a list of entity types. This are items such as:
    ///     Hospital (A&E)
    ///     Hospital (Non-A&E)
    ///     Hospital (Maternity)
    ///     Cardiac Specialist
    ///     Walk-in Centre
    ///     GP Surgery
    ///     Fuel
    ///     Ambulance Station
    /// </summary>
    [Serializable]
    
    public class GetEntityTypesRequest : Request
    {
    }

    
}