namespace Coding4Coffee.Basics.Localization
{
    /// <summary>
    /// Represents a string that can be dynamically localized at runtime using a resource key.
    /// </summary>
    /// <param name="resourceKey">The key used to retrieve the string from the resource manager.</param>
    /// <param name="fallbackValue">The fallback value to return if the resource key is not found.</param>
    public class LocalizableString(string resourceKey, string fallbackValue = "")
    {
        private readonly string _resourceKey = resourceKey;
        private readonly string _fallbackValue = fallbackValue;

        /// <summary>
        /// Returns the localized string for the specified resource key, or the fallback value if not found.
        /// </summary>
        /// <returns>The localized string or fallback value.</returns>
        override public string ToString() => this.GetResourceManager()?.GetString(_resourceKey) ?? _fallbackValue;

        /// <summary>
        /// Formats the localized string using the specified arguments.
        /// </summary>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns>The formatted localized string.</returns>
        public string Format(params object[] args) => string.Format(ToString(), args);

        /// <summary>
        /// Implicitly converts a <see cref="LocalizableString"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="localizableString">The <see cref="LocalizableString"/> instance to convert, or <see langword="null"/>.</param>
        public static implicit operator string?(LocalizableString? localizableString) => localizableString?.ToString();
    }
}
