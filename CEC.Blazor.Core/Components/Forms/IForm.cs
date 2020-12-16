using System;

namespace CEC.Blazor.Core
{
    public interface IForm
    {
        public Guid GUID => Guid.NewGuid();
    }
}
