using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A wrapper component around <see cref="SaveFileDialog"/> that supports dynamic localization
    /// of its title and file type filter at runtime.
    /// </summary>
    public class LocalizableSaveFileDialog : Component, ILocalizable
    {
        private readonly SaveFileDialog _dialog;

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizableSaveFileDialog"/>.
        /// </summary>
        public LocalizableSaveFileDialog()
        {
            _dialog = new SaveFileDialog();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizableSaveFileDialog"/> with the specified container.
        /// </summary>
        public LocalizableSaveFileDialog(IContainer container) : this()
        {
            container.Add(this);
        }

        /// <summary>
        /// Gets the underlying <see cref="SaveFileDialog"/> instance.
        /// </summary>
        [Browsable(false)]
        public SaveFileDialog Dialog => _dialog;

        /// <summary>
        /// Gets or sets the resource key used to localize the title of the dialog.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TitleResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the resource key used to localize the file type filter of the dialog.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? FilterResourceKey { get; set; }

        /// <summary>
        /// Localizes the title and filter of the dialog using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            var resourceManager = this.GetResourceManager();
            if (resourceManager == null) return;

            if (!string.IsNullOrWhiteSpace(TitleResourceKey))
                _dialog.Title = resourceManager.GetString(TitleResourceKey) ?? _dialog.Title;

            if (!string.IsNullOrWhiteSpace(FilterResourceKey))
                _dialog.Filter = resourceManager.GetString(FilterResourceKey) ?? _dialog.Filter;
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
