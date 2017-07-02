using System.ComponentModel;

namespace Quest.Lib.Constants
{
    public enum Stage
    {
        [Description("Development")] Development = 0,

        [Description("Production")] Production = 1,

        [Description("Test")] Test = 2,

        [Description("Public")] Public = 3
    }
}