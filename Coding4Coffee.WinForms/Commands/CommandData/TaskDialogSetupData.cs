using Coding4Coffee.Basics.Localization;

namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Setup data specifying the appearance and behavior of a <see cref="TaskDialogPage"/>.
    /// Follows the setup data pattern used across the dialog commands.
    /// </summary>
    public class TaskDialogSetupData
    {
        /// <summary>
        /// Gets or sets the text displayed in the title bar of the task dialog.
        /// </summary>
        public LocalizableString? Caption { get; set; } = null;

        /// <summary>
        /// Gets or sets the main instruction / heading text displayed prominently at the top.
        /// </summary>
        public LocalizableString? Heading { get; set; } = null;

        /// <summary>
        /// Gets or sets the primary descriptive body text.
        /// </summary>
        public LocalizableString? Text { get; set; } = null;

        /// <summary>
        /// Gets or sets the main icon displayed next to the heading.
        /// </summary>
        public TaskDialogIcon? Icon { get; set; } = null;

        /// <summary>
        /// Gets or sets standard message box buttons to be displayed.
        /// When configured, buttons will automatically be created and localized using <see cref="LocalizableString"/>.
        /// </summary>
        public MessageBoxButtons? Buttons { get; set; } = null;

        /// <summary>
        /// Gets or sets the resource keys used to localize standard dialog buttons.
        /// Defaults to <see cref="DialogButtonKeys.Default"/>, which can be configured once at application startup.
        /// Assign a custom instance to override keys for this dialog only.
        /// </summary>
        public DialogButtonKeys ButtonKeys { get; set; } = DialogButtonKeys.Default;

        /// <summary>
        /// Gets or sets custom buttons to be added to the task dialog page.
        /// </summary>
        public IList<TaskDialogButton>? CustomButtons { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the task dialog can be closed via the ESC key or window close button.
        /// </summary>
        public bool? AllowCancel { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the dialog can be minimized when opened modelessly.
        /// </summary>
        public bool? AllowMinimize { get; set; } = null;

        /// <summary>
        /// Gets or sets whether text links should be rendered as clickable hyperlinks.
        /// </summary>
        public bool? EnableLinks { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the dialog sizes to content layout.
        /// </summary>
        public bool? SizeToContent { get; set; } = null;

        /// <summary>
        /// Gets or sets optional expander text to provide collapsable details.
        /// </summary>
        public LocalizableString? ExpanderText { get; set; } = null;

        /// <summary>
        /// Gets or sets optional verification check box text (e.g., "Do not ask again").
        /// </summary>
        public LocalizableString? VerificationText { get; set; } = null;

        /// <summary>
        /// Gets or sets the initial state of the verification check box.
        /// </summary>
        public bool? VerificationChecked { get; set; } = null;

        /// <summary>
        /// Configures the provided <see cref="TaskDialogPage"/> instance with the specified settings.
        /// Resolves localized strings using current culture settings at the time of invocation.
        /// </summary>
        /// <param name="page">The page instance to configure.</param>
        public virtual void SetupPage(TaskDialogPage page)
        {
            ArgumentNullException.ThrowIfNull(page);

            page.Caption = Caption;
            page.Heading = Heading;
            page.Text = Text;

            if (Icon is not null) page.Icon = Icon;
            if (AllowCancel.HasValue) page.AllowCancel = AllowCancel.Value;
            if (AllowMinimize.HasValue) page.AllowMinimize = AllowMinimize.Value;
            if (EnableLinks.HasValue) page.EnableLinks = EnableLinks.Value;
            if (SizeToContent.HasValue) page.SizeToContent = SizeToContent.Value;

            if (ExpanderText is not null)
                page.Expander = new TaskDialogExpander(ExpanderText.ToString());

            if (VerificationText is not null)
            {
                page.Verification = new TaskDialogVerificationCheckBox(VerificationText.ToString())
                {
                    Checked = VerificationChecked ?? false
                };
            }

            if (CustomButtons is not null)
                foreach (var button in CustomButtons)
                    page.Buttons.Add(button);
            else if (Buttons.HasValue)
                foreach (var button in CreateLocalizedButtons(Buttons.Value, ButtonKeys))
                    page.Buttons.Add(button);
        }

        /// <summary>
        /// Creates a list of localized <see cref="TaskDialogButton"/> instances corresponding to the specified <see cref="MessageBoxButtons"/>.
        /// Each button text uses <see cref="LocalizableString"/> with standard fallback values (e.g. "Dialog_OK" / "OK").
        /// </summary>
        /// <param name="buttons">The button combination to create.</param>
        /// <param name="keys">
        /// Optional resource keys to use for button labels.
        /// When <see langword="null"/>, <see cref="DialogButtonKeys.Default"/> is used.
        /// </param>
        /// <returns>A list of localized <see cref="TaskDialogButton"/> instances.</returns>
        public static IList<TaskDialogButton> CreateLocalizedButtons(MessageBoxButtons buttons, DialogButtonKeys? keys = null)
        {
            keys ??= DialogButtonKeys.Default;

            var result = new List<TaskDialogButton>();

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    result.Add(CreateButton(keys.OK, "OK", DialogResult.OK));
                    break;

                case MessageBoxButtons.OKCancel:
                    result.Add(CreateButton(keys.OK, "OK", DialogResult.OK));
                    result.Add(CreateButton(keys.Cancel, "Cancel", DialogResult.Cancel));
                    break;

                case MessageBoxButtons.YesNo:
                    result.Add(CreateButton(keys.Yes, "Yes", DialogResult.Yes));
                    result.Add(CreateButton(keys.No, "No", DialogResult.No));
                    break;

                case MessageBoxButtons.YesNoCancel:
                    result.Add(CreateButton(keys.Yes, "Yes", DialogResult.Yes));
                    result.Add(CreateButton(keys.No, "No", DialogResult.No));
                    result.Add(CreateButton(keys.Cancel, "Cancel", DialogResult.Cancel));
                    break;

                case MessageBoxButtons.RetryCancel:
                    result.Add(CreateButton(keys.Retry, "Retry", DialogResult.Retry));
                    result.Add(CreateButton(keys.Cancel, "Cancel", DialogResult.Cancel));
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    result.Add(CreateButton(keys.Abort, "Abort", DialogResult.Abort));
                    result.Add(CreateButton(keys.Retry, "Retry", DialogResult.Retry));
                    result.Add(CreateButton(keys.Ignore, "Ignore", DialogResult.Ignore));
                    break;

                case MessageBoxButtons.CancelTryContinue:
                    result.Add(CreateButton(keys.Cancel, "Cancel", DialogResult.Cancel));
                    result.Add(CreateButton(keys.TryAgain, "Try Again", DialogResult.Retry));
                    result.Add(CreateButton(keys.Continue, "Continue", DialogResult.Ignore));
                    break;
            }

            return result;
        }

        private static TaskDialogButton CreateButton(string resourceKey, string fallbackValue, DialogResult result)
        {
            var text = new LocalizableString(resourceKey, fallbackValue).ToString();
            return new TaskDialogButton(text) { Tag = result };
        }
    }
}
