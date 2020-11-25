using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CEC.Blazor.Components
{
    public class ModalResult
    {
        public ModalResultType ResultType { get; private set; } = ModalResultType.NoSet;

        public object Data { get; set; } = null;


        public static ModalResult OK() => new ModalResult() {ResultType = ModalResultType.OK };

        public static ModalResult Exit() => new ModalResult() {ResultType = ModalResultType.Exit};

        public static ModalResult Cancel() => new ModalResult() {ResultType = ModalResultType.Cancel };

        public static ModalResult OK(object data) => new ModalResult() { Data = data, ResultType = ModalResultType.OK };

        public static ModalResult Exit(object data) => new ModalResult() { Data = data, ResultType = ModalResultType.Exit };

        public static ModalResult Cancel(object data) => new ModalResult() { Data = data, ResultType = ModalResultType.Cancel };
    }
}
