using Quest.Lib.HEMS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Quest.HEMSLinkTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static public HEMSLinkServer server { get; set; }
        static public HEMSLinkServer client { get; set; }
    }
}
