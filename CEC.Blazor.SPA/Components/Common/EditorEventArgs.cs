using System;
using CEC.Blazor.Data;

namespace CEC.Blazor.SPA.Components
{
    public class EditorEventArgs : EventArgs
    {
        public PageExitType ExitType { get; set; } = PageExitType.None;

        public int ID { get; set; }

        public string RecordName { get; set; } = string.Empty;

        public EditorEventArgs()
        {
            this.ExitType = PageExitType.ExitToRoot;
            this.ID = 0;
        }

        public EditorEventArgs(PageExitType exitType)
        {
            this.ExitType = exitType;
            this.ID = 0;
        }

        public EditorEventArgs(PageExitType exitType, int id)
        {
            this.ExitType = exitType;
            this.ID = id;
        }

        public EditorEventArgs(PageExitType exitType, int id, string recordname)
        {
            this.ExitType = exitType;
            this.ID = id;
            this.RecordName = recordname;
        }

    }
}
