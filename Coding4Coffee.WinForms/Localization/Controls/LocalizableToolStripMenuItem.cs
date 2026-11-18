using System.ComponentModel;
using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A tool strip menu item control that supports dynamic localization at runtime.
    /// </summary>
    public class LocalizableToolStripMenuItem : ToolStripMenuItem, ILocalizable
    {
        /// <summary>
        /// Gets or sets the resource key used to localize the text of the menu item.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Localizes the text of the menu item and its dropdown items using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            if (!string.IsNullOrWhiteSpace(TextResourceKey))
                Text = this.GetResourceManager()?.GetString(TextResourceKey) ?? Text;

            DropDownItems.LocalizeCollection();
        }
    }
}

