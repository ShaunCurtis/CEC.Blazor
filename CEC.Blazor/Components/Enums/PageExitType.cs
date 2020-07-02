using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Components
{

    public enum PageExitType
    {
        None,
        ExitToRoot,
        ExitToList,
        ExitToView,
        ExitToEditor,
        SwitchToEditor,
        ExitToNew,
        ExitToLast
    }
}
