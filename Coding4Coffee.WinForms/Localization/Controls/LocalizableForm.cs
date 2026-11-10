using System.ComponentModel;
using System.Diagnostics;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A form that supports dynamic localization at runtime.
    /// </summary>
    public class LocalizableForm : Form, ILocalizable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableForm"/> class.
        /// Registers the load event to trigger localization.
        /// </summary>
        public LocalizableForm()
        {
            Load += LocalizableForm_Load;
            LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;
        }

        private void LocalizableForm_Load(object? sender, EventArgs e) => Localize();

        private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
        {
            if (Created && !IsDisposed)
                Localize();
        }

        /// <summary>
        /// Clean up any resources being used and unsubscribe from events.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
            
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets the resource key used to localize the title text of the form.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Localizes the form and its child controls using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            Debug.WriteLine($"Localizing form: {Name}, TextResourceKey: {TextResourceKey}, CurrentCulture: {LocalizationManager.CurrentCulture?.Name}");

            if (!string.IsNullOrWhiteSpace(TextResourceKey))
                Text = this.GetResourceManager()?.GetString(TextResourceKey) ?? Text;

            Controls.LocalizeCollection();

            if (IsMdiContainer)
                MdiChildren.Where(child => child is not ILocalizable).LocalizeCollection();
        }
    }
}

