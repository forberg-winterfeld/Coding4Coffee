using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Coding4Coffee.Basics.Configuration;

namespace Coding4Coffee.WinForms.Configuration
{
    /// <summary>
    /// A <see cref="Config{T}"/> specialization that stores an <see cref="Image"/> as a Base64-encoded PNG string.
    /// </summary>
    public class ImageConfig : Config<string>
    {
        private static readonly ConditionalWeakTable<Image, MemoryStream> _imageStreams = new();

        /// <summary>
        /// Gets or sets the value as an <see cref="Image"/>.
        /// </summary>
        /// <remarks>
        /// Shadows <see cref="Config{TValue}.Value"/>. Images are persisted as a Base64-encoded PNG string (<see cref="ImageFormat.Png"/>).
        /// If the stored string is empty, whitespace, or invalid Base64 image data, getting this property returns <see langword="null"/>
        /// without throwing an exception. Similarly, if saving the image to PNG fails, <see langword="null"/> is stored.
        /// Each successful get creates a new image that the caller must dispose. Setting the property does not
        /// transfer ownership of the supplied image. The source stream remains open while the returned image is reachable.
        /// </remarks>
        public new Image? Value
        {
            get
            {
                if (string.IsNullOrWhiteSpace(base.Value))
                    return null;

                try
                {
                    byte[] bytes = Convert.FromBase64String(base.Value);
                    var stream = new MemoryStream(bytes);
                    var image = Image.FromStream(stream);
                    // Image.FromStream requires its stream to remain open for the image's lifetime.
                    _imageStreams.Add(image, stream);
                    return image;
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
                    value.Save(ms, ImageFormat.Png);
                    base.Value = Convert.ToBase64String(ms.ToArray());
                }
                catch
                {
                    base.Value = null;
                }
            }
        }

        /// <summary>
        /// Implicitly converts an <see cref="ImageConfig"/> to a nullable <see cref="Image"/>.
        /// </summary>
        /// <remarks>Each successful conversion creates a new image that the caller must dispose.</remarks>
        /// <param name="config">The <see cref="ImageConfig"/> instance to convert.</param>
        /// <returns>The converted <see cref="Image"/> value, or <see langword="null"/> if the configuration has no value or is invalid.</returns>
        public static implicit operator Image?(ImageConfig config) => config.Value;
    }
}
