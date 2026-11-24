using Coding4Coffee.Basics.Configuration;

namespace Coding4Coffee.WinForms.Configuration
{
    /// <summary>
    /// A <see cref="Config{T}"/> specialization that stores a <see cref="Color"/> as an HTML color string.
    /// </summary>
    public class ColorConfig : Config<string>
    {
        /// <summary>
        /// Gets or sets the value as a <see cref="Color"/>.
        /// </summary>
        /// <remarks>
        /// Shadows <see cref="Config{TValue}.Value"/>. Colors are serialized using <see cref="ColorTranslator.ToHtml(Color)"/>
        /// and parsed using <see cref="ColorTranslator.FromHtml(string)"/>.
        /// If the underlying string is empty, whitespace, or invalid HTML color syntax, getting this property
        /// returns <see langword="null"/> without throwing an exception.
        /// </remarks>
        public new Color? Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Value))
                    return null;

                try
                {
                    return ColorTranslator.FromHtml(base.Value);
                }
                catch
                {
                    return null;
                }
            }
            set => base.Value = value.HasValue ? ColorTranslator.ToHtml(value.Value) : null;
        }

        /// <summary>
        /// Implicitly converts a <see cref="ColorConfig"/> to a nullable <see cref="Color"/>.
        /// </summary>
        /// <param name="config">The <see cref="ColorConfig"/> instance to convert.</param>
        /// <returns>The converted <see cref="Color"/> value, or <see langword="null"/> if the configuration has no value or is invalid.</returns>
        public static implicit operator Color?(ColorConfig config) => config.Value;
    }
}
