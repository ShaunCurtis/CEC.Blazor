using CEC.Routing.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Components
{
    public interface IView : IComponent
    {
        public Guid GUID => Guid.NewGuid();
    }
}
