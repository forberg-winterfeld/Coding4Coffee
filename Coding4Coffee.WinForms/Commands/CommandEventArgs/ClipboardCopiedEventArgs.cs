namespace Coding4Coffee.WinForms.Commands.CommandEventArgs
{
    /// <summary>
    /// Event arguments for clipboard copy events.
    /// Contains the data or text copied to the clipboard.
    /// </summary>
    /// <param name="data">The data copied to the clipboard.</param>
    public class ClipboardCopiedEventArgs(object? data) : EventArgs
    {
        /// <summary>
        /// Gets the data that was copied to the clipboard.
        /// </summary>
        public object? Data { get; } = data;

        /// <summary>
        /// Gets the data as text if available, or the result of <see cref="object.ToString"/>.
        /// </summary>
        public string? Text => Data as string ?? Data?.ToString();
    }
}
