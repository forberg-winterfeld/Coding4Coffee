namespace Coding4Coffee.WinForms.Commands.CommandEventArgs
{
    /// <summary>
    /// Event arguments for task dialog closed events.
    /// Contains the clicked <see cref="TaskDialogButton"/> and the associated <see cref="TaskDialogPage"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TaskDialogClosedEventArgs"/> class.
    /// </remarks>
    /// <param name="clickedButton">The button that was clicked to close the dialog, or <see langword="null"/>.</param>
    /// <param name="page">The task dialog page that was displayed.</param>
    public class TaskDialogClosedEventArgs(TaskDialogButton? clickedButton, TaskDialogPage page) : EventArgs
    {
        /// <summary>
        /// Gets the button that was clicked to close the dialog.
        /// </summary>
        public TaskDialogButton? ClickedButton { get; } = clickedButton;

        /// <summary>
        /// Gets the task dialog page that was closed.
        /// </summary>
        public TaskDialogPage Page { get; } = page;

        /// <summary>
        /// Gets the <see cref="System.Windows.Forms.DialogResult"/> represented by the clicked button,
        /// if mapped or stored in the button's <see cref="TaskDialogControl.Tag"/> property.
        /// </summary>
        public DialogResult DialogResult
        {
            get
            {
                if (ClickedButton?.Tag is DialogResult result)
                    return result;

                if (ClickedButton == TaskDialogButton.OK) return DialogResult.OK;
                if (ClickedButton == TaskDialogButton.Cancel) return DialogResult.Cancel;
                if (ClickedButton == TaskDialogButton.Yes) return DialogResult.Yes;
                if (ClickedButton == TaskDialogButton.No) return DialogResult.No;
                if (ClickedButton == TaskDialogButton.Retry) return DialogResult.Retry;
                if (ClickedButton == TaskDialogButton.Abort) return DialogResult.Abort;
                if (ClickedButton == TaskDialogButton.Ignore) return DialogResult.Ignore;

                return DialogResult.None;
            }
        }
    }
}
