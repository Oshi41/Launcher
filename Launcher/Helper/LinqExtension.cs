using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Launcher.Helper
{
    public static class LinqExtension
    {
        public static bool IsTermwiseEquals<T>(this IEnumerable<T> orignal, IEnumerable<T> sequence, IEqualityComparer<T> comparer = null)
        {
            if (orignal.IsNullOrEmpty() && sequence.IsNullOrEmpty())
                return true;

            if (orignal.IsNullOrEmpty() || sequence.IsNullOrEmpty())
                return true;

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            var any = orignal.Except(sequence, comparer);
            return !any.Any();
        }

        public static IList<T> SelectRecursive<T>(T source, Func<T, IEnumerable<T>> search)
        {
            if (source == null || search == null)
                return new List<T>();

            var result = new List<T>();

            // копируем, так как результат вычисления может быть отложенным
            var children = search(source).ToList();

            if (!children.IsNullOrEmpty())
            {
                result.AddRange(children);

                foreach (var child in children)
                {
                    var childrenOfChildren = SelectRecursive(child, search);

                    if (!childrenOfChildren.IsNullOrEmpty())
                        result.AddRange(childrenOfChildren);
                }
            }

            return result;
        }

        public static bool IsNullOrEmpty(this IEnumerable sequence)
        {
            return sequence == null || !sequence.GetEnumerator().MoveNext();
        }
    }
}
