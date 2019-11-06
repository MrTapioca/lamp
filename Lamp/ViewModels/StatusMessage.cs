using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lamp.ViewModels
{
    public class StatusMessage
    {
        public string Message { get; set; }
        public string Type { get; set; }

        public StatusMessage(string type, string message)
        {
            Type = type;
            Message = message;
        }
    }

    public static class StatusMessageType
    {
        public const string Success = "success";
        public const string Error = "danger";
        public const string Warning = "warning";
        public const string Info = "info";
    }
}