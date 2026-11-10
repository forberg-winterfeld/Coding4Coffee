namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Setup data for <see cref="OpenFileDialog"/>.
    /// Extends <see cref="FileDialogSetupData{TDialog}"/> with properties specific to <see cref="OpenFileDialog"/>.
    /// </summary>
    public class OpenFileDialogSetupData : FileDialogSetupData<OpenFileDialog>
    {
        // ── OpenFileDialog-spezifische Properties ─────────────────────────────

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box allows multiple files to be selected.
        /// </summary>
        public bool? Multiselect { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box displays a warning
        /// if the user specifies a file name that does not exist.
        /// </summary>
        public bool? CheckFileExists { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the read-only check box is selected.
        /// </summary>
        public bool? ReadOnlyChecked { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box contains a read-only check box.
        /// </summary>
        public bool? ShowReadOnly { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box allows selection of read-only files.
        /// </summary>
        public bool? SelectReadOnly { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box shows a preview for selected files.
        /// </summary>
        public bool? ShowPreview { get; set; } = null;

        // ── Setup-Methode ─────────────────────────────────────────────────────

        /// <inheritdoc />
        public override void SetupDialog(OpenFileDialog dialog)
        {
            base.SetupDialog(dialog);

            if (Multiselect.HasValue)     dialog.Multiselect     = Multiselect.Value;
            if (CheckFileExists.HasValue) dialog.CheckFileExists = CheckFileExists.Value;
            if (ReadOnlyChecked.HasValue) dialog.ReadOnlyChecked = ReadOnlyChecked.Value;
            if (ShowReadOnly.HasValue)    dialog.ShowReadOnly    = ShowReadOnly.Value;
            if (SelectReadOnly.HasValue)  dialog.SelectReadOnly  = SelectReadOnly.Value;
            if (ShowPreview.HasValue)     dialog.ShowPreview     = ShowPreview.Value;
        }
    }
}
