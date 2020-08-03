using CEC.Blazor.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CEC.Blazor.Components.UIControls
{
    /// <summary>
    /// UI Rendering Wrapper to build a Column
    ///  Provides a structured  mechanism for managing Bootstrap class elements used in Editors and Viewers in one place. 
    /// The properties are pretty self explanatory and therefore not decorated with summaries
    /// </summary>

    public class UITColumn<TRecord> : UIComponent
    {
        /// <summary>
        /// Cascaded UICardList
        /// </summary>
        [CascadingParameter]
        public UICardList<TRecord> Card { get; set; }

        /// <summary>
        /// Record Configuration
        /// </summary>
        [CascadingParameter]
        public RecordConfigurationData RecordConfiguration { get; set; }

        /// <summary>
        /// Record ID passed via a cascade
        /// </summary>
        [CascadingParameter(Name = "RecordID")]
        public int RecordID { get; set; } = 0;

        [Parameter]
        public int Column { get; set; } = 1;

        protected override string _Tag => "td";

    }
}
