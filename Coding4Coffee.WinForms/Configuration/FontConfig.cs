using System.ComponentModel;
using Coding4Coffee.Basics.Configuration;

namespace Coding4Coffee.WinForms.Configuration
{
    /// <summary>
    /// A <see cref="Config{T}"/> specialization that stores a <see cref="Font"/> as a serialized string.
    /// </summary>
    public class FontConfig : Config<string>
    {
        private static readonly TypeConverter _converter = TypeDescriptor.GetConverter(typeof(Font));

        /// <summary>
        /// Gets or sets the value as a <see cref="Font"/>.
        /// </summary>
        /// <remarks>
        /// Shadows <see cref="Config{TValue}.Value"/>. Fonts are serialized and parsed using <see cref="FontConverter"/>
        /// via invariant culture string representation.
        /// If the stored string is empty, whitespace, or invalid, getting this property returns <see langword="null"/>
        /// without throwing an exception.
        /// </remarks>
        public new Font? Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Value))
                    return null;

                try
                {
                    return _converter.ConvertFromInvariantString(base.Value) as Font;
                }
                catch
                {
                    return null;
                }
            }
            set => base.Value = value != null ? _converter.ConvertToInvariantString(value) : null;
        }

        /// <summary>
        /// Implicitly converts a <see cref="FontConfig"/> to a nullable <see cref="Font"/>.
        /// </summary>
        /// <param name="config">The <see cref="FontConfig"/> instance to convert.</param>
        /// <returns>The converted <see cref="Font"/> value, or <see langword="null"/> if the configuration has no value or is invalid.</returns>
        public static implicit operator Font?(FontConfig config) => config.Value;
    }
}
