using System.Windows.Input;

namespace Coding4Coffee.Basics.Commands
{
    /// <summary>
    /// Abstraction for application exit command implementations across different UI frameworks.
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for commands that cleanly terminate an application.
    /// Framework-specific implementations (e.g., WinForms, WPF, MAUI) should provide their own
    /// concrete implementations that invoke the appropriate platform-specific shutdown mechanism.
    ///
    /// Implementations should:
    /// - Allow optional user confirmation before exit
    /// - Enable conditional execution via <see cref="ICommand.CanExecute(object?)"/>
    /// - Raise an event before the application begins to shut down
    /// </remarks>
    public interface IExitApplicationCommand : ICommand
    {
        /// <summary>
        /// Event that is raised immediately before the application exit is initiated.
        /// </summary>
        event EventHandler? Exiting;

        /// <summary>
        /// Raises the <see cref="ICommand.CanExecuteChanged"/> event.
        /// </summary>
        /// <remarks>
        /// Implementations should provide this method to allow external code to signal
        /// that the command's ability to execute may have changed.
        /// </remarks>
        void RaiseCanExecuteChanged();
    }
}
