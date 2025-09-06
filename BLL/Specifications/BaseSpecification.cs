using DAL.Models;
using System.Linq.Expressions;

namespace BLLProject.Specifications
{
    public class BaseSpecification<T> : ISpecification<T> where T : BaseClass
    {
        public Expression<Func<T, bool>> Criteria { get; set; }
        public List<Expression<Func<T, object>>> Includes { get; set; } = new
            List<Expression<Func<T, object>>>();
        public List<Func<IQueryable<T>, IQueryable<T>>> ComplexIncludes { get; set; } = new();
        public Expression<Func<T, object>> OrderBy { get; set; }
        public Expression<Func<T, object>> OrderByDescending { get; set; }
        public BaseSpecification()
        {

        }

        public BaseSpecification(Expression<Func<T, bool>> CriteriaExpression)
        {
            Criteria = CriteriaExpression;
        }

        public BaseSpecification(Expression<Func<T, bool>> criteriaExpression, Func<IQueryable<T>, IQueryable<T>> includeQuery)
        {
            Criteria = criteriaExpression;
            ComplexIncludes.Add(includeQuery);
        }

        public void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }
        public void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

    }

}
