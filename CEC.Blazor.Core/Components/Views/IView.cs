using Microsoft.AspNetCore.Components;
using System;

namespace CEC.Blazor.Core
{
    public interface IView : IComponent
    {
        public Guid GUID => Guid.NewGuid();

        [CascadingParameter] public ViewManager ViewManager { get; set; }

    }
}
