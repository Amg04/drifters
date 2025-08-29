using DAL.Models;
using System.Linq.Expressions;

namespace BLLProject.Specifications
{
    public interface ISpecification<T> where T : BaseClass
    {
        public Expression<Func<T, bool>> Criteria { get; protected set; }
        public List<Expression<Func<T, object>>> Includes { get; protected set; }
        public List<Func<IQueryable<T>, IQueryable<T>>> ComplexIncludes { get; protected set; }
    }
}
