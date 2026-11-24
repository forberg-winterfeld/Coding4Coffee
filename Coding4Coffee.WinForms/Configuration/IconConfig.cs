using Coding4Coffee.Basics.Configuration;

namespace Coding4Coffee.WinForms.Configuration
{
    /// <summary>
    /// A <see cref="Config{T}"/> specialization that stores an <see cref="Icon"/> as a Base64-encoded string.
    /// </summary>
    public class IconConfig : Config<string>
    {
        /// <summary>
        /// Gets or sets the value as an <see cref="Icon"/>.
        /// </summary>
        /// <remarks>
        /// Shadows <see cref="Config{TValue}.Value"/>. Icons are persisted as a Base64-encoded string representing the raw icon stream.
        /// If the stored string is empty, whitespace, or invalid Base64 icon data, getting this property returns <see langword="null"/>
        /// without throwing an exception. Similarly, if saving the icon fails, <see langword="null"/> is stored.
        /// Each successful get creates a new icon that the caller must dispose. Setting the property does not
        /// transfer ownership of the supplied icon.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "WFO2000", Justification = "Each get intentionally creates an independent icon owned by the caller.")]
        public new Icon? Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Value))
                    return null;

                try
                {
                    byte[] bytes = Convert.FromBase64String(base.Value);
                    using var ms = new MemoryStream(bytes);
                    return new Icon(ms);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    base.Value = null;
                    return;
                }

                try
                {
                    using var ms = new MemoryStream();
                    value.Save(ms);
                    base.Value = Convert.ToBase64String(ms.ToArray());
                }
                catch
                {
                    base.Value = null;
                }
            }
        }

        /// <summary>
        /// Implicitly converts an <see cref="IconConfig"/> to a nullable <see cref="Icon"/>.
        /// </summary>
        /// <remarks>Each successful conversion creates a new icon that the caller must dispose.</remarks>
        /// <param name="config">The <see cref="IconConfig"/> instance to convert.</param>
        /// <returns>The converted <see cref="Icon"/> value, or <see langword="null"/> if the configuration has no value or is invalid.</returns>
        public static implicit operator Icon?(IconConfig config) => config.Value;
    }
}
