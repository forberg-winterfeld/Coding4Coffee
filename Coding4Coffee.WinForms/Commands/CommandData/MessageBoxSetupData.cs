namespace Coding4Coffee.WinForms.Commands.CommandData
{
    /// <summary>
    /// Setup data specifying the appearance and behavior of a <see cref="MessageBox"/>.
    /// </summary>
    public class MessageBoxSetupData
    {
        /// <summary>
        /// Gets or sets the message text displayed in the message box.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text displayed in the title bar of the message box.
        /// </summary>
        public string Caption { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the buttons displayed in the message box.
        /// Defaults to <see cref="MessageBoxButtons.OK"/>.
        /// </summary>
        public MessageBoxButtons Buttons { get; set; } = MessageBoxButtons.OK;

        /// <summary>
        /// Gets or sets the icon displayed in the message box.
        /// Defaults to <see cref="MessageBoxIcon.None"/>.
        /// </summary>
        public MessageBoxIcon Icon { get; set; } = MessageBoxIcon.None;

        /// <summary>
        /// Gets or sets the default button for the message box.
        /// Defaults to <see cref="MessageBoxDefaultButton.Button1"/>.
        /// </summary>
        public MessageBoxDefaultButton DefaultButton { get; set; } = MessageBoxDefaultButton.Button1;

        /// <summary>
        /// Gets or sets options for the message box.
        /// Defaults to 0 (default options).
        /// </summary>
        public MessageBoxOptions Options { get; set; } = 0;
    }
}

