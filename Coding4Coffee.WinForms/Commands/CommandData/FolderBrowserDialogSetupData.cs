namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Setup data for <see cref="FolderBrowserDialog"/>.
    /// Extends <see cref="CommonDialogSetupData{TDialog}"/> with properties specific to <see cref="FolderBrowserDialog"/>.
    /// </summary>
    public class FolderBrowserDialogSetupData : CommonDialogSetupData<FolderBrowserDialog>
    {
        /// <summary>
        /// Gets or sets the descriptive text displayed above the tree view control in the dialog box.
        /// </summary>
        public string? Description { get; set; } = null;

        /// <summary>
        /// Gets or sets the root folder where the browsing starts.
        /// The user cannot navigate above this folder in the hierarchy.
        /// </summary>
        public Environment.SpecialFolder? RootFolder { get; set; } = null;

        /// <summary>
        /// Gets or sets the path selected by the user.
        /// When set before showing the dialog, determines the initially highlighted folder.
        /// </summary>
        public string? SelectedPath { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the New Folder button is visible in the dialog box.
        /// </summary>
        public bool? ShowNewFolderButton { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box will automatically upgrade
        /// to enable new features when running on Windows Vista or later.
        /// </summary>
        public bool? AutoUpgradeEnabled { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box uses the value of
        /// <see cref="Description"/> as the dialog title instead of displaying it above the tree view.
        /// Only effective when <see cref="AutoUpgradeEnabled"/> is <see langword="true"/>.
        /// </summary>
        public bool? UseDescriptionForTitle { get; set; } = null;

        // ── Setup-Methode ─────────────────────────────────────────────────────

        /// <inheritdoc />
        public override void SetupDialog(FolderBrowserDialog dialog)
        {
            if (Description is not null)          dialog.Description          = Description;
            if (RootFolder.HasValue)              dialog.RootFolder           = RootFolder.Value;
            if (SelectedPath is not null)         dialog.SelectedPath         = SelectedPath;
            if (ShowNewFolderButton.HasValue)     dialog.ShowNewFolderButton  = ShowNewFolderButton.Value;
            if (AutoUpgradeEnabled.HasValue)      dialog.AutoUpgradeEnabled   = AutoUpgradeEnabled.Value;
            if (UseDescriptionForTitle.HasValue)  dialog.UseDescriptionForTitle = UseDescriptionForTitle.Value;
        }
    }
}
