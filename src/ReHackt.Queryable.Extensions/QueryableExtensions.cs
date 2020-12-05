using ReHackt.Queryable.Extensions;
using System.Linq.Expressions;

namespace System.Linq
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Filters a sequence of values based on a filter.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to filter.</param>
        /// <param name="filter">A filter to test each element for a condition.</param>
        /// <returns>An <see cref="IQueryable"/> that contains elements from the input sequence that satisfy the condition specified by filter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source or filter is null.</exception>
        public static IQueryable<T> Filter<T>(this IQueryable<T> source, QueryableFilter<T> filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            return filter.Apply(source);
        }

        /// <summary>
        /// Filters a sequence of values based on a query string.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to filter.</param>
        /// <param name="filterQuery">A query string to be interpreted as a filter to test each element for a condition.</param>
        /// <returns>An <see cref="IQueryable"/> that contains elements from the input sequence that satisfy the condition specified by filter query.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentException">Thrown when filter query is invalid.</exception>
        public static IQueryable<T> Filter<T>(this IQueryable<T> source, string filterQuery)
        {
            return QueryableFilter.TryParse(filterQuery, out QueryableFilter<T> filter)
                ? source.Filter(filter)
                : throw new ArgumentException("Invalid filter query", nameof(filterQuery));
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate if a specified condition is satisfied.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to filter.</param>
        /// <param name="condition">A condition to satisfy in order to filter the sequence.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IQueryable"/> that contains elements from the input sequence that 
        /// satisfy the condition specified by predicate if the main condition is satisfied, or all the 
        /// input sequence otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source or predicate is null.</exception>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? source.Where(predicate) : source;
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to multiple keys.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to order.</param>
        /// <param name="keys">Multiple keys from each element.</param>
        /// <returns>An <see cref="IOrderedQueryable"/> whose elements are sorted in ascending order according to multiple keys.</returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, params string[] keys)
        {
            if (keys == null || keys.Length < 1) throw new ArgumentNullException(nameof(keys));

            var orderedSource = source.OrderBy(keys[0]);
            for (int i = 1; i < keys.Length; i++)
            {
                orderedSource = orderedSource.ThenBy(keys[i]);
            }
            return orderedSource;
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to order.</param>
        /// <param name="key">A key from each element.</param>
        /// <returns>An <see cref="IOrderedQueryable"/> whose elements are sorted in ascending order according to a key.</returns>
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string key)
        {
            return CallOrderedQueryable(source, nameof(Queryable.OrderBy), key);
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to multiple keys.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to order.</param>
        /// <param name="keys">Multiple keys from each element.</param>
        /// <returns>An <see cref="IOrderedQueryable"/> whose elements are sorted in descending order according to multiple keys.</returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, params string[] keys)
        {
            if (keys == null || keys.Length < 1) throw new ArgumentNullException(nameof(keys));

            var orderedSource = source.OrderByDescending(keys[0]);
            for (int i = 1; i < keys.Length; i++)
            {
                orderedSource = orderedSource.ThenByDescending(keys[i]);
            }
            return orderedSource;
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to order.</param>
        /// <param name="key">A key from each element.</param>
        /// <returns>An <see cref="IOrderedQueryable"/> whose elements are sorted in descending order according to a key.</returns>
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string key)
        {
            return CallOrderedQueryable(source, nameof(Queryable.OrderByDescending), key);
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to order.</param>
        /// <param name="key">A key from each element.</param>
        /// <returns>An <see cref="IOrderedQueryable"/> whose elements are sorted in ascending order according to a key.</returns>
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string key)
        {
            return CallOrderedQueryable(source, nameof(Queryable.ThenBy), key);
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to order.</param>
        /// <param name="keys">A key from each element.</param>
        /// <returns>An <see cref="IOrderedQueryable"/> whose elements are sorted in descending order according to a key.</returns>
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string key)
        {
            return CallOrderedQueryable(source, nameof(Queryable.ThenByDescending), key);
        }

        private static IOrderedQueryable<T> CallOrderedQueryable<T>(this IQueryable<T> source, string methodName, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var param = Expression.Parameter(typeof(T));
            var body = key.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);
            return (IOrderedQueryable<T>)source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { typeof(T), body.Type },
                    source.Expression,
                    Expression.Lambda(body, param)
                )
            );
        }

        /// <summary>
        /// Paginate a sequence of values.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">An <see cref="IQueryable"/> to paginate.</param>
        /// <param name="page">A page index.</param>
        /// <param name="pageSize">A page size.</param>
        /// <returns>An <see cref="IQueryable"/> that contains a page of elements from the input sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> source, int page, int pageSize)
        {
            return source.Skip(((page < 1 ? 1 : page) - 1) * pageSize).Take(pageSize);
        }
    }
}