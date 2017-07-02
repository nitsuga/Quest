////////////////////////////////////////////////////////////////////////////////////////////////
//   
//   Copyright (C) 2014 Extent Ltd. Copying is only allowed with the express permission of Extent Ltd
//   
//   Use of this code is not permitted without a valid license from Extent Ltd
//   
////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace Quest.Lib.AutoDispatch
{
    /// <summary>
    /// Generaic interface for calculating the number and type of resources that need to attend the given incident
    /// </summary>
    [Export(typeof(IResourceCalculator))]
    public class ResourceCalculator : IResourceCalculator
    {

        Dictionary<string, DeterminantResponse> cache = null;

        /// <summary>
        /// build a cache of determinants and their corresponding dispatch policy
        /// </summary>
        /// <param name="simEngine"></param>
        private void BuildCache()
        {
            using (QuestEntities db = new QuestEntities())
            {
                    cache = new Dictionary<string, DeterminantResponse>();
                    foreach (DeterminantResponse ampds in from f in db.DeterminantResponses select f)
                        cache.Add(ampds.determinant, ampds);
            }
        }

        /// <summary>
        /// setup the number of ambulances and fru's required
        /// </summary>
        /// <param name="inc"></param>
        public void CalculateResourceRequired(IncidentView inc)
        {
            try
            {
                if (cache == null)
                    BuildCache();


                if (inc.IncidentDeterminant == null)
                    return;

                if (inc.AMBRequired == 0 && inc.FRURequired == 0)
                {
                    // does this AMPDS code warrant an dispatch??
                    DeterminantResponse fred;
                    cache.TryGetValue(inc.IncidentDeterminant, out fred);


                    // no record or no paramedic required
                    if (fred != null)
                    {
                        inc.AMBRequired = int.Parse(fred.ambulances);
                        inc.FRURequired = int.Parse(fred.paramedics);
                        inc.TotalRequired = int.Parse(fred.all_responders);
                    }

                    // this is an override on normal behaviour.. these normally get dispatched manually

                    if (inc.FRURequired == 0 && inc.AMBRequired == 0)
                    {
                        inc.AMBRequired = 1;
                        inc.FRURequired = 1;
                        inc.TotalRequired = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO:
                // simEngine.OnError(ex);
            }
        }
    }
}