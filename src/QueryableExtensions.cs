using ReHackt.Queryable.Extensions;
using System.Linq.Expressions;

namespace System.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, QueryableFilter<T> filter)
        {
            return filter == null ? query : filter.Apply(query);
        }

        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }

        public static IQueryable<T> PageBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderBy, int page, int pageSize, bool orderByDescending = false)
        {
            query = orderByDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            return query.Skip(((page < 1 ? 1 : page) - 1) * pageSize).Take(pageSize);
        }

        #region Ordering

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, nameof(Queryable.OrderBy), propertyName);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, nameof(Queryable.OrderByDescending), propertyName);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, nameof(Queryable.ThenBy), propertyName);
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, nameof(Queryable.ThenByDescending), propertyName);
        }

        private static IOrderedQueryable<T> CallOrderedQueryable<T>(this IQueryable<T> query, string methodName, string propertyName)
        {
            var param = Expression.Parameter(typeof(T), "x");
            var body = propertyName.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);
            return (IOrderedQueryable<T>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new[] { typeof(T), body.Type },
                    query.Expression,
                    Expression.Lambda(body, param)
                )
            );
        }

        #endregion
    }
}