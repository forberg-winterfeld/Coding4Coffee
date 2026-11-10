namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Base class for setup data of file dialogs.
    /// Contains all configurable properties that are inherited from <see cref="FileDialog"/>.
    /// </summary>
    /// <typeparam name="TDialog">The type of the file dialog. Must inherit from <see cref="FileDialog"/>.</typeparam>
    public abstract class FileDialogSetupData<TDialog> : CommonDialogSetupData<TDialog>
        where TDialog : FileDialog
    {
        /// <summary>
        /// Gets or sets a value indicating whether the dialog box automatically adds an extension
        /// to a file name if the user omits the extension.
        /// </summary>
        public bool? AddExtension { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box adds the file being opened
        /// to the recent list.
        /// </summary>
        public bool? AddToRecent { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether this FileDialog instance should automatically
        /// upgrade appearance and behavior when running on Windows Vista.
        /// </summary>
        public bool? AutoUpgradeEnabled { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box displays a warning
        /// if the user specifies a path that does not exist.
        /// </summary>
        public bool? CheckPathExists { get; set; } = null;

        /// <summary>
        /// Gets or sets the default file name extension.
        /// </summary>
        public string? DefaultExt { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box returns the location of the file
        /// referenced by the shortcut or whether it returns the location of the shortcut (.lnk).
        /// </summary>
        public bool? DereferenceLinks { get; set; } = null;

        /// <summary>
        /// Gets or sets a string containing the file name selected in the file dialog box.
        /// </summary>
        public string? FileName { get; set; } = null;

        /// <summary>
        /// Gets or sets the current file name filter string, which determines the choices
        /// that appear in the "Files of type" box in the dialog box.
        /// </summary>
        public string? Filter { get; set; } = null;

        /// <summary>
        /// Gets or sets the index of the filter currently selected in the file dialog box.
        /// </summary>
        public int? FilterIndex { get; set; } = null;

        /// <summary>
        /// Gets or sets the initial directory displayed by the file dialog box.
        /// </summary>
        public string? InitialDirectory { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the OK button of the dialog box is disabled
        /// until the user navigates the view or edits the filename (if applicable).
        /// </summary>
        public bool? OkRequiresInteraction { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box restores the directory
        /// to the previously selected directory before closing.
        /// </summary>
        public bool? RestoreDirectory { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the Help button is displayed in the file dialog box.
        /// </summary>
        public bool? ShowHelp { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box displays hidden and system files.
        /// </summary>
        public bool? ShowHiddenFiles { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the items shown by default
        /// in the view's navigation pane are shown.
        /// </summary>
        public bool? ShowPinnedPlaces { get; set; } = null;

        /// <summary>
        /// Gets or sets whether the dialog box supports displaying and saving files
        /// that have multiple file name extensions.
        /// </summary>
        public bool? SupportMultiDottedExtensions { get; set; } = null;

        /// <summary>
        /// Gets or sets the file dialog box title.
        /// </summary>
        public string? Title { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the dialog box accepts only valid Win32 file names.
        /// </summary>
        public bool? ValidateNames { get; set; } = null;

        /// <summary>
        /// Applies all configured properties to the given <paramref name="dialog"/>.
        /// Only properties that have been explicitly set (non-null) are applied.
        /// </summary>
        /// <param name="dialog">The file dialog to configure.</param>
        public override void SetupDialog(TDialog dialog)
        {
            if (AddExtension.HasValue)                 dialog.AddExtension              = AddExtension.Value;
            if (AddToRecent.HasValue)                  dialog.AddToRecent               = AddToRecent.Value;
            if (AutoUpgradeEnabled.HasValue)           dialog.AutoUpgradeEnabled        = AutoUpgradeEnabled.Value;
            if (CheckPathExists.HasValue)              dialog.CheckPathExists           = CheckPathExists.Value;
            if (DefaultExt is not null)                dialog.DefaultExt                = DefaultExt;
            if (DereferenceLinks.HasValue)             dialog.DereferenceLinks          = DereferenceLinks.Value;
            if (FileName is not null)                  dialog.FileName                  = FileName;
            if (Filter is not null)                    dialog.Filter                    = Filter;
            if (FilterIndex.HasValue)                  dialog.FilterIndex               = FilterIndex.Value;
            if (InitialDirectory is not null)          dialog.InitialDirectory          = InitialDirectory;
            if (OkRequiresInteraction.HasValue)        dialog.OkRequiresInteraction     = OkRequiresInteraction.Value;
            if (RestoreDirectory.HasValue)             dialog.RestoreDirectory          = RestoreDirectory.Value;
            if (ShowHelp.HasValue)                     dialog.ShowHelp                  = ShowHelp.Value;
            if (ShowHiddenFiles.HasValue)              dialog.ShowHiddenFiles           = ShowHiddenFiles.Value;
            if (ShowPinnedPlaces.HasValue)             dialog.ShowPinnedPlaces          = ShowPinnedPlaces.Value;
            if (SupportMultiDottedExtensions.HasValue) dialog.SupportMultiDottedExtensions = SupportMultiDottedExtensions.Value;
            if (Title is not null)                     dialog.Title                     = Title;
            if (ValidateNames.HasValue)                dialog.ValidateNames             = ValidateNames.Value;
        }
    }
}
