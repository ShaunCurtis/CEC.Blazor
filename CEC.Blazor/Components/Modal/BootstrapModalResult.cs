using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CEC.Blazor.Components.Modal
{
    public class BootstrapModalResult
    {
        public BootstrapModalResultType ResultType { get; private set; } = BootstrapModalResultType.NoSet;

        public object Data { get; set; } = null;


        public static BootstrapModalResult OK() => new BootstrapModalResult() {ResultType = BootstrapModalResultType.OK };

        public static BootstrapModalResult Exit() => new BootstrapModalResult() {ResultType = BootstrapModalResultType.Exit};

        public static BootstrapModalResult Cancel() => new BootstrapModalResult() {ResultType = BootstrapModalResultType.Cancel };

        public static BootstrapModalResult OK(object data) => new BootstrapModalResult() { Data = data, ResultType = BootstrapModalResultType.OK };

        public static BootstrapModalResult Exit(object data) => new BootstrapModalResult() { Data = data, ResultType = BootstrapModalResultType.Exit };

        public static BootstrapModalResult Cancel(object data) => new BootstrapModalResult() { Data = data, ResultType = BootstrapModalResultType.Cancel };
    }
}
