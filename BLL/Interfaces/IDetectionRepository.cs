using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IDetectionRepository
    {
        Task<IEnumerable<Detection>> GetAllAsync();
        Task<IEnumerable<Detection>> GetByCameraIdAsync(string cameraId);
        Task<IEnumerable<Detection>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<Detection> GetByIdAsync(int id);
        Task<Detection> AddAsync(Detection detection);
        Task UpdateAsync(Detection detection);
        Task DeleteAsync(int id);
        Task<IEnumerable<Detection>> GetByObjectTypeAsync(string objectType);
        Task<IEnumerable<Detection>> GetByConfidenceRangeAsync(float minConfidence, float maxConfidence);
        Task<int> GetDetectionCountByCameraAsync(string cameraId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<Detection>> GetRecentDetectionsAsync(int take = 50);
        Task DeleteOldDetectionsAsync(DateTime cutoffDate);
        Task<IEnumerable<Detection>> GetHighConfidenceDetectionsAsync(float threshold = 0.8f);
        Task<Dictionary<string, int>> GetDetectionStatsByObjectTypeAsync(DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<Detection>> GetPagedDetectionsAsync(int pageNumber, int pageSize, string cameraId = null);

    }
}
