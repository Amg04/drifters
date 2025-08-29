using DAL.Models;

namespace BLLProject.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseClass;
        int Complete();
    }
}
