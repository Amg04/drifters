using BLLProject.Specifications;
using DAL.Models;
namespace BLLProject.Interfaces
{
    public interface IGenericRepository<T> where T : BaseClass
    {
        void Add(T entity);
        void Delete(T entity);
        T Get(int Id);
        IEnumerable<T> GetAll();
        void Update(T entity);
        T GetEntityWithSpec(ISpecification<T> spec);
        IEnumerable<T> GetAllWithSpec(ISpecification<T> spec);
        void RemoveRange(IEnumerable<T> entities);
    }
}
