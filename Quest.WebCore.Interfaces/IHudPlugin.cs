using System.Collections.Generic;

namespace Quest.WebCore.Interfaces
{
    public interface IHudPlugin
    {
        /// <summary>
        /// The name of the plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Text to display in the menu
        /// </summary>
        string MenuText { get; }

        /// <summary>
        /// Is this plugin viewable in the menu?
        /// </summary>
        bool IsMenuItem { get; }

        /// <summary>
        /// A collection of key/value pairs of properties required for this plugin to be rendered correctly
        /// </summary>
        Dictionary<string, object> Properties { get; set; }

        ///// <summary>
        ///// By default, most plugings should be able to be rendered multiple times on the same screen.
        ///// However, some components using external technologies - eg.g Google Maps - require that only 
        ///// single instances be created per page. For such components, set this property to TRUE
        ///// </summary>
        //bool IsSingleton { get; }

        /// <summary>
        /// A method call to render the Html for the Main Frame of a layout
        /// </summary>
        /// <returns></returns>
        string RenderHtml(string role);

        /// <summary>
        /// A single javascript command that will kick off any front-end initialization
        /// </summary>
        /// <returns></returns>
        string OnInit();

        /// <summary>
        /// A single javascript command that will be executed when the plugin has been moved to a new container
        /// </summary>
        /// <returns></returns>
        string OnPanelMoved();

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
