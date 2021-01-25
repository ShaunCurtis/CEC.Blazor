using CEC.Blazor.SPA.Components.UIControls;
using CEC.Blazor.Data;

namespace CEC.Blazor.SPA.Components
{
    public class Alert
    {

        /// <summary>
        /// Alert Message to display
        /// can contain HTML tags
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Css for the Alert
        /// </summary>
        public Bootstrap.ColourCode ColourCode { get; set; } = Bootstrap.ColourCode.info;

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
            this.ColourCode = Bootstrap.ColourCode.info;
            this.IsAlert = false;
        }

        /// <summary>
        /// Method to set the Alert
        /// </summary>
        /// <param name="message"></param>
        /// <param name="Css"></param>
        public void SetAlert(string message, Bootstrap.ColourCode colourcode)
        {
            this.Message = message;
            this.ColourCode = colourcode;
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
                    this.ColourCode = Bootstrap.ColourCode.danger;
                    break;
                case MessageType.Information:
                    this.ColourCode = Bootstrap.ColourCode.info;
                    break;
                case MessageType.Success:
                    this.ColourCode = Bootstrap.ColourCode.success;
                    break;
                case MessageType.Warning:
                    this.ColourCode = Bootstrap.ColourCode.warning;
                    break;
                default:
                    this.ColourCode = Bootstrap.ColourCode.primary;
                    this.IsAlert = false;
                    break;
            }
        }
    }
}
