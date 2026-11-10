namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Base class for setup data of common dialogs.
    /// Provides a common contract for configuring and applying settings to any <see cref="CommonDialog"/>.
    /// </summary>
    /// <typeparam name="TDialog">The type of the dialog. Must inherit from <see cref="CommonDialog"/>.</typeparam>
    public abstract class CommonDialogSetupData<TDialog>
        where TDialog : CommonDialog
    {
        /// <summary>
        /// Applies all configured properties to the given <paramref name="dialog"/>.
        /// Only properties that have been explicitly set (non-null) are applied.
        /// </summary>
        /// <param name="dialog">The dialog to configure.</param>
        public abstract void SetupDialog(TDialog dialog);
    }
}
