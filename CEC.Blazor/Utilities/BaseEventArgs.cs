using System;
using System.Collections.Generic;

namespace CEC.Blazor.Utilities
{
    public class BaseEventArgs : EventArgs
    {
        public object Caller { get; set; }

        public object This { get; set; }

        public string FunctionName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public List<BaseEventArgs> Events { get; set; } = new List<BaseEventArgs>();

        public BaseEventArgs(object Caller, object This, string FunctionName, string Message, LinkedList<BaseEventArgs> Events )
        {
            this.Caller = Caller;
            this.This = This;
            this.FunctionName = FunctionName;
            this.Message = Message;
            this.Events.AddRange(Events);
        }

        public BaseEventArgs(object Caller, object This, string FunctionName, string Message, BaseEventArgs Event)
        {
            this.Caller = Caller;
            this.This = This;
            this.FunctionName = FunctionName;
            this.Message = Message;
            this.Events.Add(Event);
        }

        public BaseEventArgs(object Caller, object This, string FunctionName, string Message)
        {
            this.Caller = Caller;
            this.This = This;
            this.FunctionName = FunctionName;
            this.Message = Message;
        }

        public BaseEventArgs(object This, string FunctionName, string Message, BaseEventArgs Event)
        {
            this.This = This;
            this.FunctionName = FunctionName;
            this.Message = Message;
            this.Events.Add(Event);
        }

        public BaseEventArgs( object This, string FunctionName, string Message)
        {
            this.This = This;
            this.FunctionName = FunctionName;
            this.Message = Message;
        }

        public BaseEventArgs(object This, string Message, BaseEventArgs Event)
        {
            this.This = This;
            this.Message = Message;
        }

        public BaseEventArgs(object This, string Message)
        {
            this.This = This;
            this.Message = Message;
        }

        public BaseEventArgs()
        {
        }
    }
}
