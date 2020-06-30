using CEC.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Server.Data
{
    public class FilterList : IFilterList
    {

        public Guid GUID { get; set; } = Guid.NewGuid();

        public int IssueID { get; set; }

        public int DaysToDisplay { get; set; } = 0;

        public bool Show { get => this.ShowState == 0; }

        public IFilterList.FilterViewState ShowState { get; set; } = IFilterList.FilterViewState.NotSet;

        public void Reset()
        {
            DaysToDisplay = 0;
            this.ShowState = IFilterList.FilterViewState.NotSet;
        }

        public IFilterList Copy()
        {
            return new FilterList()
            {
                DaysToDisplay = this.DaysToDisplay,
                ShowState = this.ShowState
            };
        }
    }
}
