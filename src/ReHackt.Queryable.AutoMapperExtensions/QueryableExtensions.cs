using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using ReHackt.Queryable.Extensions;
using System.Linq.Expressions;

namespace System.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<TData> Filter<TModel, TData>(this IQueryable<TData> source, IMapper mapper, QueryableFilter<TModel> filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            var f = mapper.MapExpression<Expression<Func<TData, bool>>>(filter.FilterExpression);
            return source.WhereIf(f != null, f);
        }

        public static IQueryable<TData> Filter<TModel, TData>(this IQueryable<TData> source, IMapper mapper, string filterQuery)
        {
            return QueryableFilter.TryParse(filterQuery, out QueryableFilter<TModel> filter)
                ? source.Filter(mapper, filter)
                : throw new ArgumentException("Invalid filter query", nameof(filterQuery));
        }
    }
}
