namespace Coding4Coffee.Basics.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Iterates over the elements of <paramref name="collection"/> and invokes the supplied <paramref name="action"/> for each element.
        /// The <paramref name="cancellationToken"/> is passed to the action and is also observed before each iteration.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="collection">The enumerable to iterate.</param>
        /// <param name="action">An <see cref="Action{T, CancellationToken}"/> that receives the current element and the cancellation token.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T, CancellationToken> action, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(collection);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in collection)
            {
                cancellationToken.ThrowIfCancellationRequested();
                action(item, cancellationToken);
            }
        }

        /// <summary>
        /// Iterates over the elements of <paramref name="collection"/> and invokes the supplied <paramref name="action"/> for each element.
        /// The <paramref name="cancellationToken"/> is observed before each iteration, but the token is **not** passed to the action.
        /// </summary>
        /// <typeparam name="T">The element type of the enumerable.</typeparam>
        /// <param name="collection">The enumerable to iterate.</param>
        /// <param name="action">An <see cref="Action{T}"/> that receives the current element.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(collection);
            ArgumentNullException.ThrowIfNull(action);

            foreach (var item in collection)
            {
                cancellationToken.ThrowIfCancellationRequested();
                action(item);
            }
        }
    }
}

