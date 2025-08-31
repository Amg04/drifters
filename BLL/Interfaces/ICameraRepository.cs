using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{

    public interface ICameraRepository
    {
        Task<IEnumerable<Camera>> GetAllAsync();
        Task<Camera> GetByIdAsync(string id);
        Task<IEnumerable<Camera>> GetActiveAsync();
        Task<Camera> AddAsync(Camera camera);
        Task UpdateAsync(Camera camera);
        Task DeleteAsync(string id);
        Task<Camera> GetByNameAsync(string name);
        Task<IEnumerable<Camera>> GetByLocationAsync(string location);
        Task<bool> ExistsAsync(string id);
        Task<bool> IsNameUniqueAsync(string name, string excludeId = null);
        Task<int> GetTotalCountAsync();
        Task<int> GetActiveCountAsync();
        Task SetActiveStatusAsync(string id, bool isActive);
        Task<IEnumerable<Camera>> GetPagedAsync(int pageNumber, int pageSize, bool? isActive = null);
        Task<IEnumerable<Camera>> SearchByNameAsync(string searchTerm);
        Task UpdateSettingsAsync(string id, CameraSettings settings);
        Task<IEnumerable<Camera>> GetCamerasWithDetectionsAsync(DateTime from, DateTime to);
        Task<Camera> GetWithDetectionsAsync(string id, int takeDetections = 10);
        Task BulkUpdateStatusAsync(IEnumerable<string> cameraIds, bool isActive);
        Task<Dictionary<string, int>> GetDetectionCountsAsync();
    }
}
