using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CEC.Blazor.Components.Base
{
    public class ModalComponentBase : ApplicationComponentBase
    {
        /// <summary>
        /// Used to determine if the page can display data
        /// </summary>
        protected virtual bool IsError { get => false; }

        /// <summary>
        /// EditContext for the component
        /// </summary>
        protected EditContext EditContext { get; set; }


        protected override void OnInitialized() { }

    }
}
