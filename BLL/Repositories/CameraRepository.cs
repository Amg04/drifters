using DAL.Data;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Repositories
{
    public class CameraRepository
    {
        private readonly DriftersDBContext _context;

        public CameraRepository(DriftersDBContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Camera>> GetAllAsync()
        {
            return await _context.Cameras
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Camera> GetByIdAsync(string id)
        {
            return await _context.Cameras
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Camera>> GetActiveAsync()
        {
            return await _context.Cameras
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Camera> AddAsync(Camera camera)
        {
            camera.Id = camera.Id ?? Guid.NewGuid().ToString();
            _context.Cameras.Add(camera);
            return camera;
        }

        public async Task UpdateAsync(Camera camera)
        {
            var existingCamera = await _context.Cameras.FindAsync(camera.Id);
            if (existingCamera != null)
            {
                _context.Entry(existingCamera).CurrentValues.SetValues(camera);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera != null)
            {
                _context.Cameras.Remove(camera);
            }
        }

        public async Task<Camera> GetByNameAsync(string name)
        {
            return await _context.Cameras
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Camera>> GetByLocationAsync(string location)
        {
            return await _context.Cameras
                .Where(c => c.Settings.Location.ToLower().Contains(location.ToLower()))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Cameras.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> IsNameUniqueAsync(string name, string excludeId = null)
        {
            var query = _context.Cameras.Where(c => c.Name.ToLower() == name.ToLower());

            if (!string.IsNullOrEmpty(excludeId))
                query = query.Where(c => c.Id != excludeId);

            return !await query.AnyAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Cameras.CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Cameras.CountAsync(c => c.IsActive);
        }

        public async Task SetActiveStatusAsync(string id, bool isActive)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera != null)
            {
                camera.IsActive = isActive;
            }
        }

        public async Task<IEnumerable<Camera>> GetPagedAsync(int pageNumber, int pageSize, bool? isActive = null)
        {
            var query = _context.Cameras.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            return await query
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Camera>> SearchByNameAsync(string searchTerm)
        {
            return await _context.Cameras
                .Where(c => c.Name.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task UpdateSettingsAsync(string id, CameraSettings settings)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera != null)
            {
                camera.Settings = settings;
            }
        }

        public async Task<IEnumerable<Camera>> GetCamerasWithDetectionsAsync(DateTime from, DateTime to)
        {
            return await _context.Cameras
                .Where(c => c.Detections.Any(d => d.Timestamp >= from && d.Timestamp <= to))
                .Include(c => c.Detections.Where(d => d.Timestamp >= from && d.Timestamp <= to))
                .ToListAsync();
        }

        public async Task<Camera> GetWithDetectionsAsync(string id, int takeDetections = 10)
        {
            return await _context.Cameras
                .Include(c => c.Detections.OrderByDescending(d => d.Timestamp).Take(takeDetections))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task BulkUpdateStatusAsync(IEnumerable<string> cameraIds, bool isActive)
        {
            var cameras = await _context.Cameras
                .Where(c => cameraIds.Contains(c.Id))
                .ToListAsync();

            foreach (var camera in cameras)
            {
                camera.IsActive = isActive;
            }
        }

        public async Task<Dictionary<string, int>> GetDetectionCountsAsync()
        {
            return await _context.Cameras
                .Select(c => new { c.Id, c.Name, DetectionCount = c.Detections.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.DetectionCount);
        }
    }
}
