using System;
using System.Collections.Generic;

namespace CEC.Blazor.Components
{
    public interface IFilterList
    {
        public enum FilterViewState
        {
            NotSet = 0,
            Show = 1,
            Hide = 2
        }

        public Guid GUID { get;}

        public bool Show { get => this.ShowState == 0; }

        public FilterViewState ShowState { get; set; }

        public Dictionary<string, object> Filters { get; set; }

        public void Reset();

        public IFilterList Copy();

    }
}
