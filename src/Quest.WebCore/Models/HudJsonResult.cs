namespace Quest.WebCore.Models
{
    public class HudJsonResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public string Html { get; set; }
        public string OnHtmlRender { get; set; }
        public string OnPanelMoved { get; set; }

        public string NavigateUrl { get; set; }

        public HudJsonResult()
        {
            Success = false;
            Message = string.Empty;
            Html = string.Empty;
            OnHtmlRender = string.Empty;
            OnPanelMoved = string.Empty;
            NavigateUrl = string.Empty;
        }
    }
}