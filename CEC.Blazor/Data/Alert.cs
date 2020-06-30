
using CEC.Blazor.Utilities;

namespace CEC.Blazor.Data
{
    public class Alert
    {
        public static string AlertPrimary = "alert-primary";
        public static string AlertSecondary = "alert-secondary";
        public static string AlertSuccess = "alert-success";
        public static string AlertDanger = "alert-danger";
        public static string AlertWarning = "alert-warning";
        public static string AlertInfo = "alert-info";
        public static string AlertLight = "alert-light";
        public static string AlertDark = "alert-dark";

        /// <summary>
        /// Alert Message to display
        /// can contain HTML tags
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Css for the Alert
        /// </summary>
        public string CSS { get; set; } = string.Empty;

        /// <summary>
        /// Bool Property to set the alert
        /// </summary>
        public bool IsAlert { get; set; } = false;

        /// <summary>
        /// Method to clear the alert
        /// </summary>
        public void ClearAlert()
        {
            this.Message = string.Empty;
            this.CSS = Alert.AlertInfo;
            this.IsAlert = false;
        }

        /// <summary>
        /// Method to set the Alert
        /// </summary>
        /// <param name="message"></param>
        /// <param name="Css"></param>
        public void SetAlert(string message, string Css)
        {
            this.Message = message;
            this.CSS = Css;
            this.IsAlert = true;
        }
        
        /// <summary>
        /// Method to built an Alert from a DbTaskResult
        /// </summary>
        /// <param name="result"></param>
        public void SetAlert(DbTaskResult result)
        {
            this.Message = result.Message;
            this.IsAlert = true;
            switch (result.Type)
            {
                case MessageType.Error:
                    this.CSS = Alert.AlertDanger;
                    break;
                case MessageType.Information:
                    this.CSS = Alert.AlertInfo;
                    break;
                case MessageType.Success:
                    this.CSS = Alert.AlertSuccess;
                    break;
                case MessageType.Warning:
                    this.CSS = Alert.AlertWarning;
                    break;
                default:
                    this.CSS = Alert.AlertPrimary;
                    this.IsAlert = false;
                    break;
            }
        }
    }
}
