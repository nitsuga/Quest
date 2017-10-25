using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Quest.WebCore.Behaviours
{
    /// <summary>
    /// behaviours link events to actions. e.g. if the system sees an event such as a Mobile phone eisec event
    /// targeted at this workstation then we want to display and ellipse on the map. The map needs to expose a
    /// drawEllipse method and the TelephonyManager needs to emit a CallerDetail message that the behaviour 
    /// can receive and translate into a DrawEllipse behaviour.
    /// </summary>
    public interface IBehaviour
    {
    }
}
