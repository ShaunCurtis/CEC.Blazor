using CEC.Blazor.Data;

namespace CEC.Blazor.Utilities
{
    public class DbTaskResult
    {

        public string Message { get; set; } = "New Object Message";

        public MessageType Type { get; set; } = MessageType.None;

        public bool IsOK { get; set; } = true;

        public int NewID { get; set; } = 0;

    }
}
