using Coding4Coffee.Basics.Localization;
using Coding4Coffee.WinForms.Commands.CommandData;
using Coding4Coffee.WinForms.Commands.CommandEventArgs;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to display a modern Windows <see cref="TaskDialog"/>.
    /// Manages the configuration, execution, result capture, and event notification for task dialogs.
    /// </summary>
    /// <remarks>
    /// Opens a modal task dialog using <see cref="TaskDialog.ShowDialog(IWin32Window, TaskDialogPage, TaskDialogStartupLocation)"/>
    /// or its overload without an owner. The result is available via <see cref="LastClickedButton"/> and <see cref="LastDialogResult"/>.
    /// Optionally, subscribers can listen to the <see cref="TaskDialogClosed"/> event.
    ///
    /// Typical usage (simple confirmation):
    /// <code>
    /// var confirmCommand = new ShowTaskDialogCommand(
    ///     heading: "Delete File?",
    ///     text: "Are you sure you want to permanently delete this file?",
    ///     caption: "Confirm Delete",
    ///     icon: TaskDialogIcon.Warning,
    ///     buttons: MessageBoxButtons.YesNo);
    ///
    /// confirmCommand.TaskDialogClosed += (_, e) =>
    /// {
    ///     if (e.DialogResult == DialogResult.Yes)
    ///         DeleteFile();
    /// };
    /// confirmCommand.Execute(ownerForm);
    /// </code>
    /// </remarks>
    public class ShowTaskDialogCommand : ICommand
    {
        private readonly TaskDialogSetupData _setupData;
        private readonly Func<object?, bool>? _canExecute;

        /// <summary>
        /// Gets the setup data configuring this task dialog.
        /// </summary>
        public TaskDialogSetupData SetupData => _setupData;

        /// <summary>
        /// Gets the most recent <see cref="TaskDialogButton"/> clicked during the last execution, or <see langword="null"/>.
        /// </summary>
        public TaskDialogButton? LastClickedButton { get; private set; }

        /// <summary>
        /// Gets the most recent <see cref="DialogResult"/> represented by the clicked button.
        /// </summary>
        public DialogResult LastDialogResult { get; private set; }

        /// <summary>
        /// Event that is raised when the command's ability to execute changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event that is raised when a task dialog closes after execution.
        /// </summary>
        public event EventHandler<TaskDialogClosedEventArgs>? TaskDialogClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTaskDialogCommand"/> class with localized text, caption, icon, and standard buttons.
        /// </summary>
        /// <param name="heading">The main instruction text displayed prominently at the top.</param>
        /// <param name="text">The secondary explanatory body text.</param>
        /// <param name="caption">The title bar caption of the dialog window.</param>
        /// <param name="icon">The icon displayed in the task dialog.</param>
        /// <param name="buttons">Standard buttons to display. These are automatically localized.</param>
        public ShowTaskDialogCommand(
            LocalizableString heading,
            LocalizableString? text = null,
            LocalizableString? caption = null,
            TaskDialogIcon? icon = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK)
            : this(new TaskDialogSetupData
            {
                Heading = heading,
                Text = text,
                Caption = caption,
                Icon = icon,
                Buttons = buttons
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTaskDialogCommand"/> class with fallback text, caption, icon, and standard buttons.
        /// </summary>
        /// <param name="heading">The main instruction text displayed prominently at the top.</param>
        /// <param name="text">The secondary explanatory body text.</param>
        /// <param name="caption">The title bar caption of the dialog window.</param>
        /// <param name="icon">The icon displayed in the task dialog.</param>
        /// <param name="buttons">Standard buttons to display. These are automatically localized.</param>
        public ShowTaskDialogCommand(
            string heading,
            string? text = null,
            string? caption = null,
            TaskDialogIcon? icon = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK)
            : this(
                new LocalizableString(heading, heading),
                text is not null ? new LocalizableString(text, text) : null,
                caption is not null ? new LocalizableString(caption, caption) : null,
                icon,
                buttons)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTaskDialogCommand"/> class with a <see cref="TaskDialogSetupData"/> instance.
        /// </summary>
        /// <param name="setupData">The setup data specifying the task dialog appearance and buttons.</param>
        /// <param name="canExecute">Optional function that evaluates the parameter to determine whether the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="setupData"/> is <see langword="null"/>.</exception>
        public ShowTaskDialogCommand(TaskDialogSetupData setupData, Func<object?, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(setupData);
            _setupData = setupData;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Returns whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">An optional parameter that can influence execution eligibility.</param>
        /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
        public virtual bool CanExecute(object? parameter)
            => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command, opening the task dialog modally and capturing the result.
        /// </summary>
        /// <remarks>
        /// If <paramref name="parameter"/> is an <see cref="IWin32Window"/>, it is used as the modal owner of the task dialog.
        /// After the dialog closes, <see cref="LastClickedButton"/> and <see cref="LastDialogResult"/> are updated,
        /// and <see cref="TaskDialogClosed"/> is raised.
        /// </remarks>
        /// <param name="parameter">An optional owner window or custom execution parameter.</param>
        public virtual void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            var page = new TaskDialogPage();
            _setupData.SetupPage(page);

            IWin32Window? owner = parameter as IWin32Window;

            LastClickedButton = owner is not null ? TaskDialog.ShowDialog(owner, page) : TaskDialog.ShowDialog(page);

            var eventArgs = new TaskDialogClosedEventArgs(LastClickedButton, page);
            LastDialogResult = eventArgs.DialogResult;

            OnTaskDialogClosed(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="TaskDialogClosed"/> event.
        /// </summary>
        /// <param name="e">Event arguments containing the clicked button and the dialog page.</param>
        protected virtual void OnTaskDialogClosed(TaskDialogClosedEventArgs e)
            => TaskDialogClosed?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public virtual void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
