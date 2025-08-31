using BLL.Interfaces;
using DAL.Models;

namespace BLLProject.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : BaseClass;
        int Complete();
        IDetectionRepository Detections { get; }
        ICameraRepository Cameras { get; }
        Task<int> SaveChangesAsync();
    }
}
