// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
    public static class MemoryExtensions
    {
        /// <summary>
        /// Returns an enumerator that iterates through a <see cref="ReadOnlySpan{T}"/>,
        /// which is split by separator <paramref name="separator"/>.
        /// </summary>
        /// <param name="span">The source span which should be iterated over.</param>
        /// <param name="separator">The separator used to separate the <paramref name="span"/>.</param>
        /// <param name="options">The <see cref="StringSplitOptions"/> which should be applied with this operation.</param>
        /// <returns>Returns an enumerator for the specified sequence.</returns>
        public static SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> span,
            T separator, StringSplitOptions options = StringSplitOptions.None) where T : IEquatable<T>
        {
            if (!Enum.IsDefined(typeof(StringSplitOptions), options))
            {
                throw new ArgumentException($"Invalid value for {nameof(options)}: {options}");
            }
            return new SpanSplitEnumerator<T>(span, separator, options == StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
    {
        private ReadOnlySpan<T> _sequence;
        private readonly T _separator;
        private SpanSplitInfo _spanSplitInfo;

        private bool ShouldRemoveEmptyEntries => _spanSplitInfo.HasFlag(SpanSplitInfo.RemoveEmptyEntries);
        private bool IsFinished => _spanSplitInfo.HasFlag(SpanSplitInfo.FinishedEnumeration);

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public ReadOnlySpan<T> Current { get; private set; }

        /// <summary>
        /// Returns the current enumerator.
        /// </summary>
        /// <returns>Returns the current enumerator.</returns>
        public SpanSplitEnumerator<T> GetEnumerator() => this;

        internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator, bool removeEmptyEntries)
        {
            Current = default;
            _sequence = span;
            _separator = separator;
            _spanSplitInfo = default(SpanSplitInfo) | (removeEmptyEntries ? SpanSplitInfo.RemoveEmptyEntries : 0);
        }

        /// <summary>
        /// Advances the enumerator to the next element in the <see cref="ReadOnlySpan{T}"/>.
        /// </summary>
        /// <returns>Returns whether there is another item in the enumerator.</returns>
        public bool MoveNext()
        {
            if (IsFinished) { return false; }

            do
            {
                int index = _sequence.IndexOf(_separator);
                if (index < 0)
                {
                    Current = _sequence;
                    _spanSplitInfo |= SpanSplitInfo.FinishedEnumeration;
                    return !(ShouldRemoveEmptyEntries && Current.IsEmpty);
                }

                Current = _sequence.Slice(index);
                _sequence = _sequence.Slice(index + 1);
            } while (Current.IsEmpty && ShouldRemoveEmptyEntries);

            return true;
        }

        [Flags]
        private enum SpanSplitInfo : byte
        {
            RemoveEmptyEntries = 0x1,
            FinishedEnumeration = 0x2
        }
    }
}