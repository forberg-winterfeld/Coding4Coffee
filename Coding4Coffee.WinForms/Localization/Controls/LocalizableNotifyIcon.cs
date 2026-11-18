using System.ComponentModel;
using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Localization.Controls
{
    /// <summary>
    /// A wrapper component around <see cref="NotifyIcon"/> that supports dynamic localization
    /// of its tooltip text, balloon tip title, and balloon tip text at runtime.
    /// </summary>
    public class LocalizableNotifyIcon : Component, ILocalizable
    {
        private readonly NotifyIcon _notifyIcon;

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizableNotifyIcon"/>.
        /// </summary>
        public LocalizableNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizableNotifyIcon"/> with the specified container.
        /// </summary>
        public LocalizableNotifyIcon(IContainer container) : this()
        {
            container.Add(this);
        }

        /// <summary>
        /// Gets the underlying <see cref="NotifyIcon"/> instance.
        /// </summary>
        [Browsable(false)]
        public NotifyIcon NotifyIcon => _notifyIcon;

        /// <summary>
        /// Gets or sets the resource key used to localize the tooltip text of the notify icon.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized tooltip text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? TextResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the resource key used to localize the balloon tip title of the notify icon.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized balloon tip title from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? BalloonTipTitleResourceKey { get; set; }

        /// <summary>
        /// Gets or sets the resource key used to localize the balloon tip text of the notify icon.
        /// </summary>
        [Browsable(true)]
        [Category("Localization")]
        [Description("The resource key used to retrieve the localized balloon tip text from the resource manager.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string? BalloonTipTextResourceKey { get; set; }

        /// <summary>
        /// Localizes the tooltip text, balloon tip title, and balloon tip text of the notify icon
        /// using the associated resource manager.
        /// </summary>
        public void Localize()
        {
            var resourceManager = this.GetResourceManager();
            if (resourceManager == null) return;

            if (!string.IsNullOrWhiteSpace(TextResourceKey))
                _notifyIcon.Text = resourceManager.GetString(TextResourceKey) ?? _notifyIcon.Text;

            if (!string.IsNullOrWhiteSpace(BalloonTipTitleResourceKey))
                _notifyIcon.BalloonTipTitle = resourceManager.GetString(BalloonTipTitleResourceKey) ?? _notifyIcon.BalloonTipTitle;

            if (!string.IsNullOrWhiteSpace(BalloonTipTextResourceKey))
                _notifyIcon.BalloonTipText = resourceManager.GetString(BalloonTipTextResourceKey) ?? _notifyIcon.BalloonTipText;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _notifyIcon.Dispose();

            base.Dispose(disposing);
        }
    }
}
