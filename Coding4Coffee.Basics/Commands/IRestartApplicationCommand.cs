using System.Windows.Input;

namespace Coding4Coffee.Basics.Commands
{
    /// <summary>
    /// Abstraction for application restart command implementations across different UI frameworks.
    /// </summary>
    /// <remarks>
    /// This interface defines the contract for commands that cleanly restart an application.
    /// Framework-specific implementations (e.g., WinForms, WPF, MAUI) should provide their own
    /// concrete implementations that invoke the appropriate platform-specific restart mechanism.
    ///
    /// Implementations should:
    /// - Allow optional user confirmation before restart
    /// - Enable conditional execution via <see cref="ICommand.CanExecute(object?)"/>
    /// - Raise an event before the restart process begins
    /// - Cleanly shut down the current instance and launch a new one
    /// </remarks>
    public interface IRestartApplicationCommand : ICommand
    {
        /// <summary>
        /// Event that is raised immediately before the application restart is initiated.
        /// </summary>
        event EventHandler? Restarting;

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
