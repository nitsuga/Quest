using System.Collections.Generic;
using GeoJSON.Net.Feature;
using Quest.Common.Messages;
using Autofac;

namespace Quest.Lib.Visuals
{
    public interface IVisualProvider
    {
        List<Visual> GetVisualsCatalogue(ILifetimeScope scope, GetVisualsCatalogueRequest request);

        /// <summary>
        /// get a list of visuals 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        FeatureCollection GetVisualsData(ILifetimeScope scope, GetVisualsDataRequest request);

        /// <summary>
        /// provides a method that can be used for adhoc query to find visuals
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        QueryVisualResponse QueryVisual(ILifetimeScope scope, QueryVisualRequest request);


    }
}
