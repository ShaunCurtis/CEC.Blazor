using System;
using System.Collections.Generic;
using System.Reflection;

namespace CEC.Blazor.Data
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DbAccessAttribute : Attribute
    {
        public string CreateSP;

        public string UpdateSP;

        public string DeleteSP;

        public string ListView;

    }
}
