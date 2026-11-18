using Coding4Coffee.WinForms.Localization.Controls;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands.Controls
{
    /// <summary>
    /// A localizable button that updates its enabled state when its command parameter changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The native <see cref="ButtonBase.Command"/> implementation checks
    /// <see cref="ICommand.CanExecute(object?)"/> when the command is assigned and when it raises
    /// <see cref="ICommand.CanExecuteChanged"/>, but not when <see cref="ButtonBase.CommandParameter"/>
    /// changes. This control also checks the command on parameter changes so that, for example,
    /// selecting another item immediately updates whether an action button can be used.
    /// </para>
    /// <para>
    /// The inherited command and parameter properties, click execution, and command event handling
    /// remain in use. A <see langword="null"/> parameter is passed through without a fallback.
    /// The native parameter setter raises its change event only when the old and new values are not
    /// equal. Changes within an existing parameter object require the command to raise
    /// <see cref="ICommand.CanExecuteChanged"/>.
    /// </para>
    /// </remarks>
    public class LocalizableButtonWithCommandSupport : LocalizableButton
    {
        /// <summary>
        /// Binds a command and optional parameter to this button.
        /// </summary>
        /// <param name="command">The command to assign.</param>
        /// <param name="parameter">The parameter to pass to the command, or <see langword="null"/>.</param>
        /// <returns>This button, for fluent method chaining.</returns>
        public LocalizableButtonWithCommandSupport BindCommand(ICommand command, object? parameter = null)
        {
            CommandParameter = parameter;
            Command = command;
            return this;
        }

        /// <summary>
        /// Raises the parameter change event and refreshes the enabled state from the current command.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnCommandParameterChanged(EventArgs e)
        {
            base.OnCommandParameterChanged(e);

            if (Command is { } command)
                Enabled = command.CanExecute(CommandParameter);
        }
    }
}
