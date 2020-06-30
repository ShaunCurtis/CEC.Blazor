using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Data
{

    public enum MessageType
    {
        None = 0,
        Success = 1,
        Error = 2,
        Warning = 3,
        Information = 4,
        NotImplemented = 5
    }
}
