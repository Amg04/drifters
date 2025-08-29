using BLLProject.Interfaces;
using DAL.Data;
using DAL.Models;
using System.Collections;
namespace BLLProject.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DriftersDBContext dbContext;
        private Hashtable _repository;

        public UnitOfWork(DriftersDBContext dbContext)
        {

            this.dbContext = dbContext;
            _repository = new Hashtable();

        }

        public IGenericRepository<T> Repository<T>() where T : BaseClass
        {
            var key = typeof(T).Name;
            if (!_repository.ContainsKey(key))
            {
                _repository.Add(key, new GenericRepository<T>(dbContext));
            }
            return _repository[key] as IGenericRepository<T>;
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        public int Complete()
        {
            return dbContext.SaveChanges();
        }


    }
}
