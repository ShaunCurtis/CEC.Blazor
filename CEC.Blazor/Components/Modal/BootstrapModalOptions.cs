using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CEC.Blazor.Components.Modal
{
    public class BootstrapModalOptions
    {
        public string Title { get; set; } = "Modal Dialog";

        public string ContainerCSS { get; set; }

        public string ModalCSS { get; set; }

        public string ModalHeaderCSS { get; set; }

        public string ModalBodyCSS { get; set; }

        public bool ShowCloseButton { get; set; }

        public bool HideHeader { get; set; }

        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    }
}
