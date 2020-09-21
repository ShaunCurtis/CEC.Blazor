using CEC.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Components
{
    public class FilterList : IFilterList
    {
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();

        public IFilterList.FilterViewState ShowState { get; set; } = IFilterList.FilterViewState.NotSet;

        public virtual void Reset()
        {
            this.ShowState = IFilterList.FilterViewState.NotSet;
        }

        public virtual IFilterList Copy()
        {
            var list = new FilterList()
            {
                ShowState = this.ShowState
            };
            foreach (var set in this.Filters) list.Filters.Add(set.Key, set.Value);
            return list;
        }
    }
}
