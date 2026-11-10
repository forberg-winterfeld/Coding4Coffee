using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Coding4Coffee.WinForms.Commands
{
    /// <summary>
    /// Command to display context help for a Windows Forms window.
    /// </summary>
    public sealed partial class ShowContextHelpCommand : ICommand
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CONTEXTHELP = 0xF180;

        /// <summary>
        /// Sends a message to the specified window.
        /// </summary>
        /// <remarks>
        /// Wraps Windows API SendMessage function.
        /// Reference: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessage
        /// 
        /// This P/Invoke call is used exclusively by ShowContextHelpCommand to trigger the WM_SYSCOMMAND
        /// message with SC_CONTEXTHELP parameter, enabling the native Windows context help UI.
        /// Only called for windows that have a valid handle and are not disposed.
        /// </remarks>
        /// <param name="hWnd">Handle to the window that will receive the message.</param>
        /// <param name="msg">The message to be sent (WM_SYSCOMMAND).</param>
        /// <param name="wParam">Additional message-dependent information (SC_CONTEXTHELP).</param>
        /// <param name="lParam">Additional message-dependent information.</param>
        /// <returns>The return value specifies the result of the message processing.</returns>
        [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private readonly IWin32Window _window;
 
        /// <summary>
        /// Initializes a new command for displaying context help.
        /// </summary>
        /// <param name="window">The window or control for which context help should be displayed.</param>
        public ShowContextHelpCommand(IWin32Window window)
        {
            ArgumentNullException.ThrowIfNull(window);
            _window = window;
        }

        /// <summary>
        /// This event is not used by this command. The command state only depends on whether
        /// the underlying window is disposed, which cannot be reliably tracked without accessing
        /// the window's protected lifetime. Always call CanExecute directly before Execute.
        /// </summary>
        /// <remarks>
        /// Not applicable for this command. Call CanExecute directly instead.
        /// The event is provided only to fulfill the ICommand contract. It is never raised.
        /// </remarks>
        [Obsolete("Not applicable for this command. Call CanExecute directly instead.", error: false)]
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Determines whether the command can be executed.
        /// Returns false if the window is disposed or has an invalid handle.
        /// </summary>
        public bool CanExecute(object? _)
        {
            try
            {
                // Attempt to read the handle - throws an exception if the window is disposed
                return _window.Handle != IntPtr.Zero;
            }
            catch
            {
                // If the handle cannot be read (e.g., window disposed), the command cannot be executed
                return false;
            }
        }

        /// <summary>
        /// Triggers the context help for the window.
        /// </summary>
        public void Execute(object? _)
        {
            if (!CanExecute(null))
                return;

            SendMessage(_window.Handle, WM_SYSCOMMAND, (IntPtr)SC_CONTEXTHELP, IntPtr.Zero);
        }
    }
}