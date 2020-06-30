using System;
using CEC.Blazor.Data;

namespace CEC.Blazor.Components
{
    public class EditorEventArgs : EventArgs
    {
        public PageExitType ExitType { get; set; } = PageExitType.None;

        public long ID { get; set; }

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

        public EditorEventArgs(PageExitType exitType, long id)
        {
            this.ExitType = exitType;
            this.ID = id;
        }

        public EditorEventArgs(PageExitType exitType, long id, string recordname)
        {
            this.ExitType = exitType;
            this.ID = id;
            this.RecordName = recordname;
        }

    }
}
