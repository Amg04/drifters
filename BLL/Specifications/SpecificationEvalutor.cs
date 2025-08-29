using DAL.Models;
using Microsoft.EntityFrameworkCore;
namespace BLLProject.Specifications
{
    public static class SpecificationEvalutor<TEntity> where TEntity : BaseClass
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> input, ISpecification<TEntity> spec)
        {
            var query = input;
            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            if (spec.Includes?.Any() == true)
                query = spec.Includes.Aggregate(query, (Current, IncludeExpression) => Current.Include(IncludeExpression));
            if (spec.ComplexIncludes?.Any() == true)
                query = spec.ComplexIncludes.Aggregate(query, (current, includeQuery) => includeQuery(current));
            return query;
        }
    }

}
