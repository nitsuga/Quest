namespace Quest.WebCore.Models
{
    public class HudPanelAction
    {
        /// <summary>
        /// target window
        /// </summary>
        public int Target;

        /// <summary>
        /// position of action button e.g. panel-btn-bottom
        /// </summary>
        public string Position;

        /// <summary>
        /// action to be take: swap, expand, menu, fullscreen
        /// </summary>
        public string Action;

        /// <summary>
        /// label for the action: e.g. Expand left
        /// </summary>
        public string Label;

        /// <summary>
        /// e.g. glyphicon-triangle-bottom, glyphicon-triangle-left
        /// </summary>
        public string Icon;
    }
}