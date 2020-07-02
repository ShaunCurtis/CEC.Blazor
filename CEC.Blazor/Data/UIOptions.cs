using System;
using System.Collections.Generic;
using System.Text;

namespace CEC.Blazor.Data
{
    public class UIOptions
    {
        /// <summary>
        /// Show the Edit Button on Lists and Views
        /// </summary>
        public bool ShowEdit { get; set; }

        /// <summary>
        /// Show the add button on lists and views
        /// </summary>
        public bool ShowAdd { get; set; }

        /// <summary>
        /// Show the buttons on lists and views
        /// </summary>
        public bool ShowButtons { get; set; }

        public bool ListNavigationToViewer { get; set; }
    }
}
