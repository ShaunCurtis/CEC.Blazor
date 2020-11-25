using System.Collections.Generic;

namespace CEC.Blazor.Components
{
    public class FilterList : IFilterList
    {
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();

        public IFilterList.FilterViewState ShowState { get; set; } = IFilterList.FilterViewState.NotSet;

        public bool OnlyLoadIfFilters { get; set; } = false;
    }
}
