using System;

namespace CEC.Blazor.Components
{
    public interface IForm
    {
        public Guid GUID => Guid.NewGuid();
    }
}
