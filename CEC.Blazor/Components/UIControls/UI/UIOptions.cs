using System;
using System.Collections.Generic;
using System.Text;

namespace CEC.Blazor.Components.UIControls
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

        /// <summary>
        /// Disaplay the viewer in a Modal
        /// </summary>
        public bool UseModalViewer { get; set; }

        /// <summary>
        /// Display the editor in a Modal
        /// </summary>
        public bool UseModalEditor { get; set; }

        /// <summary>
        /// Column Number for the maximum width column
        /// </summary>
        public int MaxColumn { get; set; } = 2;

        /// <summary>
        /// Max Width Column max size in percent
        /// </summary>
        public int MaxColumnPercent { get; set; } = 50;
    }
}
