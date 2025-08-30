using System.Threading.Tasks;

namespace DALProject.DBInitializer
{
    public interface IDBInitializer
    {
         Task Initialize();
    }
}
