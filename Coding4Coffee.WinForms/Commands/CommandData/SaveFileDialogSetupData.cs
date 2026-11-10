namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Setup data for <see cref="SaveFileDialog"/>.
    /// Extends <see cref="FileDialogSetupData{TDialog}"/> with properties specific to <see cref="SaveFileDialog"/>.
    /// </summary>
    public class SaveFileDialogSetupData : FileDialogSetupData<SaveFileDialog>
    {
        // ── SaveFileDialog-spezifische Properties ─────────────────────────────

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box prompts the user for
        /// permission to create a file if the user specifies a file that does not exist.
        /// </summary>
        public bool? CreatePrompt { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box displays a warning
        /// if the user specifies the name of a file that already exists.
        /// </summary>
        public bool? OverwritePrompt { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box is always opened in the expanded mode.
        /// </summary>
        public bool? ExpandedMode { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box displays a warning
        /// if the user specifies a file name that is read-only.
        /// </summary>
        public bool? CheckWriteAccess { get; set; } = null;

        // ── Setup-Methode ─────────────────────────────────────────────────────

        /// <inheritdoc />
        public override void SetupDialog(SaveFileDialog dialog)
        {
            base.SetupDialog(dialog);

            if (CreatePrompt.HasValue)    dialog.CreatePrompt    = CreatePrompt.Value;
            if (OverwritePrompt.HasValue) dialog.OverwritePrompt = OverwritePrompt.Value;
            if (ExpandedMode.HasValue)    dialog.ExpandedMode    = ExpandedMode.Value;
            if (CheckWriteAccess.HasValue) dialog.CheckWriteAccess = CheckWriteAccess.Value;
        }
    }
}
