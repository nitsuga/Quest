using System;

namespace Quest.Lib.Simulation
{
    public interface IResManager
    {
        ObservableCollectionEx<Resource> Resources { get; set; }
        String GetMPCNameById(int ResourceId);
        Resource GetResourceByName(string MPCName);
        Resource FindResource(int resourceId);
        Resource FindResource(String Callsign);
    }
}
