using CEC.Blazor.Components.Base;
using CEC.Blazor.Components.Modal;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.UIControls
{
    public interface IModal
    {
        ModalOptions Options { get; set; }

        Task<ModalResult> Show<TModal>(ModalOptions options) where TModal : IComponent;

        void Update(ModalOptions options = null);

        void Dismiss();

        void Close(ModalResult result);
    }
}
