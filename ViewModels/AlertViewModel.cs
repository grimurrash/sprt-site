using System;

namespace NewSprt.ViewModels
{
    [Serializable]
    public class AlertViewModel
    {
        public AlertType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public AlertViewModel(AlertType type, string message = "", string title = "")
        {
            Type = type;
            Title = title;
            Message = message;
        }

        public string GetAlertTypeClass()
        {
            string alertTypeClass;
            switch (Type)
            {
                case AlertType.Success:
                    alertTypeClass = "success";
                    break;
                case AlertType.Error:
                    alertTypeClass = "danger";
                    break;
                case AlertType.Warning:
                    alertTypeClass = "warning";
                    break;
                case AlertType.Info:
                    alertTypeClass = "info";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return alertTypeClass;
        }
    }
    
    public enum AlertType
    {
        Success,
        Error,
        Warning,
        Info
    }
}