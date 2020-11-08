using CEC.Blazor.Components.Modal;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CEC.Blazor.Components.UIControls
{
    public interface IModal
    {
        public ModalOptions Options { get; set; }

        public Task<ModalResult> Show<TModal>(ModalOptions options) where TModal : IComponent;

        public void Update(ModalOptions options = null);

        public void Dismiss();

        public void Close(ModalResult result);
    }
}
