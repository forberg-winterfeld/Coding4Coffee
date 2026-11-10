namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Holds the resource keys used to localize standard dialog buttons in a <see cref="TaskDialog"/>.
    /// </summary>
    /// <remarks>
    /// Configure <see cref="Default"/> once at application startup to apply custom resource keys globally.
    /// For per-dialog customization, assign a dedicated instance to <see cref="TaskDialogSetupData.ButtonKeys"/>.
    ///
    /// Example — configure globally at startup:
    /// <code>
    /// DialogButtonKeys.Default.OK     = "MyApp_Btn_OK";
    /// DialogButtonKeys.Default.Cancel = "MyApp_Btn_Cancel";
    /// DialogButtonKeys.Default.Yes    = "MyApp_Btn_Yes";
    /// DialogButtonKeys.Default.No     = "MyApp_Btn_No";
    /// </code>
    ///
    /// Example — override for a single dialog:
    /// <code>
    /// var setupData = new TaskDialogSetupData
    /// {
    ///     Heading  = new LocalizableString("ConfirmDelete_Heading", "Delete item?"),
    ///     Buttons  = MessageBoxButtons.YesNo,
    ///     ButtonKeys = new DialogButtonKeys { Yes = "Common_Yes", No = "Common_No" }
    /// };
    /// </code>
    /// </remarks>
    public class DialogButtonKeys
    {
        /// <summary>
        /// Gets the default <see cref="DialogButtonKeys"/> instance shared across all task dialogs.
        /// Configure its properties once at application startup to apply globally.
        /// </summary>
        public static DialogButtonKeys Default { get; } = new DialogButtonKeys();

        /// <summary>Gets or sets the resource key for the OK button.</summary>
        public string OK { get; set; } = "Dialog_OK";

        /// <summary>Gets or sets the resource key for the Cancel button.</summary>
        public string Cancel { get; set; } = "Dialog_Cancel";

        /// <summary>Gets or sets the resource key for the Yes button.</summary>
        public string Yes { get; set; } = "Dialog_Yes";

        /// <summary>Gets or sets the resource key for the No button.</summary>
        public string No { get; set; } = "Dialog_No";

        /// <summary>Gets or sets the resource key for the Retry button.</summary>
        public string Retry { get; set; } = "Dialog_Retry";

        /// <summary>Gets or sets the resource key for the Abort button.</summary>
        public string Abort { get; set; } = "Dialog_Abort";

        /// <summary>Gets or sets the resource key for the Ignore button.</summary>
        public string Ignore { get; set; } = "Dialog_Ignore";

        /// <summary>Gets or sets the resource key for the Try Again button.</summary>
        public string TryAgain { get; set; } = "Dialog_TryAgain";

        /// <summary>Gets or sets the resource key for the Continue button.</summary>
        public string Continue { get; set; } = "Dialog_Continue";
    }
}
