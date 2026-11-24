using Coding4Coffee.Basics.Extensions;

namespace Coding4Coffee.Basics.Tests
{
    public class IEnumerableExtensionsTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ForEach_InvokesActionOncePerItemInEnumerationOrder(bool passToken)
        {
            using var source = new CancellationTokenSource();
            var actual = new List<int>();
            var tokens = new List<CancellationToken>();
            IEnumerable<int> items = Enumerable.Range(2, 3);

            if (passToken)
                items.ForEach((value, token) => { actual.Add(value); tokens.Add(token); }, source.Token);
            else
                items.ForEach(actual.Add, source.Token);

            Assert.Collection(actual, value => Assert.Equal(2, value), value => Assert.Equal(3, value), value => Assert.Equal(4, value));
            if (passToken)
                Assert.All(tokens, token => Assert.Equal(source.Token, token));
            Assert.Equal(passToken ? 3 : 0, tokens.Count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ForEach_CanceledBeforeEnumerationDoesNotInvokeAction(bool passToken)
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var actual = new List<int>();

            var error = Assert.Throws<OperationCanceledException>(() => Invoke(Enumerable.Range(1, 2), actual.Add, passToken, source.Token));

            Assert.Equal(source.Token, error.CancellationToken);
            Assert.Empty(actual);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ForEach_CancellationDuringActionStopsBeforeNextActionAndDisposesEnumerator(bool passToken)
        {
            using var source = new CancellationTokenSource();
            var actual = new List<int>();
            bool disposed = false;
            IEnumerable<int> Items()
            {
                try
                {
                    yield return 1;
                    yield return 2;
                    yield return 3;
                }
                finally
                {
                    disposed = true;
                }
            }

            var error = Assert.Throws<OperationCanceledException>(() => Invoke(Items(), value =>
            {
                actual.Add(value);
                if (value == 2)
                    source.Cancel();
            }, passToken, source.Token));

            Assert.Equal(source.Token, error.CancellationToken);
            Assert.Collection(actual, value => Assert.Equal(1, value), value => Assert.Equal(2, value));
            Assert.True(disposed);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ForEach_EmptyCollectionDoesNotThrowWhenCanceled(bool passToken)
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var actual = new List<int>();

            Invoke([], actual.Add, passToken, source.Token);

            Assert.Empty(actual);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ForEach_CancellationOnLastItemDoesNotThrow(bool passToken)
        {
            using var source = new CancellationTokenSource();
            var actual = new List<int>();

            Invoke(Enumerable.Repeat(1, 1), value => { actual.Add(value); source.Cancel(); }, passToken, source.Token);

            Assert.Equal(1, Assert.Single(actual));
            Assert.True(source.IsCancellationRequested);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ForEach_ActionExceptionPropagatesAndDisposesEnumerator(bool passToken)
        {
            var expected = new InvalidOperationException("Action failed");
            var actual = new List<int>();
            bool disposed = false;
            IEnumerable<int> Items()
            {
                try
                {
                    yield return 1;
                    yield return 2;
                    yield return 3;
                }
                finally
                {
                    disposed = true;
                }
            }

            var error = Assert.Throws<InvalidOperationException>(() => Invoke(Items(), value =>
            {
                actual.Add(value);
                if (value == 2)
                    throw expected;
            }, passToken, CancellationToken.None));

            Assert.Same(expected, error);
            Assert.Collection(actual, value => Assert.Equal(1, value), value => Assert.Equal(2, value));
            Assert.True(disposed);
        }

        [Fact]
        public void ForEach_TokenAwareActionCanObserveTokenAndCancelItself()
        {
            using var source = new CancellationTokenSource();
            var actual = new List<int>();

            var error = Assert.Throws<OperationCanceledException>(() => Enumerable.Range(1, 3).ForEach((item, token) =>
            {
                actual.Add(item);
                source.Cancel();
                token.ThrowIfCancellationRequested();
            }, source.Token));

            Assert.Equal(source.Token, error.CancellationToken);
            Assert.Equal(1, Assert.Single(actual));
        }

        [Fact]
        public void ForEach_NullArgumentsAreRejectedByBothOverloads()
        {
            void action(int _) { }
            void tokenAction(int _1, CancellationToken _2) { }
            IEnumerable<int> items = [];

            Assert.Equal("collection", Assert.Throws<ArgumentNullException>(() => IEnumerableExtensions.ForEach<int>(null!, action, CancellationToken.None)).ParamName);
            Assert.Equal("collection", Assert.Throws<ArgumentNullException>(() => IEnumerableExtensions.ForEach<int>(null!, tokenAction, CancellationToken.None)).ParamName);
            Assert.Equal("action", Assert.Throws<ArgumentNullException>(() => items.ForEach((Action<int>)null!, CancellationToken.None)).ParamName);
            Assert.Equal("action", Assert.Throws<ArgumentNullException>(() => items.ForEach((Action<int, CancellationToken>)null!, CancellationToken.None)).ParamName);
        }

        private static void Invoke(IEnumerable<int> items, Action<int> action, bool passToken, CancellationToken token)
        {
            if (passToken)
                items.ForEach((value, _) => action(value), token);
            else
                items.ForEach(action, token);
        }
    }
}
