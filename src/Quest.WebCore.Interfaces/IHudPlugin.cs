using System.Collections.Generic;
using System.IO;

namespace Quest.WebCore.Interfaces
{
    public interface IHudPlugin
    {
        /// <summary>
        /// The name of the plugin
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Text to display in the menu
        /// </summary>
        string MenuText { get; set; }

        /// <summary>
        /// Is this plugin viewable in the menu?
        /// </summary>
        bool IsMenuItem { get; set; }

        /// <summary>
        /// A collection of key/value pairs of properties required for this plugin to be rendered correctly
        /// </summary>
        Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// A method call to render the Html for the Main Frame of a layout
        /// </summary>
        /// <returns></returns>
        string RenderHtml();

        /// <summary>
        /// A single javascript command that will kick off any front-end initialization
        /// </summary>
        /// <returns></returns>
        string OnInit();

        /// <summary>
        /// Sets the properties of the plugin from a supplied dictioary of properties. Confirms that all the properties required by this plugin
        /// have been set. Will throw an exception of required properties are missing
        /// </summary>
        /// <exception cref="MissingPropertyException"></exception>
        /// <exception cref="InvalidPropertyValueException"></exception>
        void InitializeWithProperties(Dictionary<string, object> properties);

        /// <summary>
        /// Sets the properties of the plugin with some default values
        /// </summary>
        void InitializeWithDefaultProperties();

    }
     
}
