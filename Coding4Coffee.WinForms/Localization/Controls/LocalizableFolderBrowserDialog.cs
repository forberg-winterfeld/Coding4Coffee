using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A wrapper component around <see cref="FolderBrowserDialog"/> that supports dynamic localization
    /// of its description at runtime.
    /// </summary>
    public class LocalizableFolderBrowserDialog : Component, ILocalizable
    {
        private readonly FolderBrowserDialog _dialog;

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizableFolderBrowserDialog"/>.
        /// </summary>
        public LocalizableFolderBrowserDialog()
        {
            _dialog = new FolderBrowserDialog();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizableFolderBrowserDialog"/> with the specified container.
        /// </summary>
        public LocalizableFolderBrowserDialog(IContainer container) : this()
        {
            container.Add(this);
        }

        /// <summary>
        /// Gets the underlying <see cref="FolderBrowserDialog"/> instance.
        /// </summary>
        [Browsable(false)]
        public FolderBrowserDialog Dialog => _dialog;

        /// <summary>
        /// Gets or sets the resource key used to localize the description text shown above the folder tree.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? DescriptionResourceKey { get; set; }

        /// <summary>
        /// Localizes the description of the dialog using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            var resourceManager = this.GetResourceManager();
            if (resourceManager == null) return;

            if (!string.IsNullOrWhiteSpace(DescriptionResourceKey))
                _dialog.Description = resourceManager.GetString(DescriptionResourceKey) ?? _dialog.Description;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _dialog.Dispose();

            base.Dispose(disposing);
        }
    }
}
