using BLL.Interfaces;
using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Repositories
{
    public class DetectionRepository : IDetectionRepository
    {
        private readonly DriftersDBContext _context;

        public DetectionRepository(DriftersDBContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Detection>> GetAllAsync()
        {
            return await _context.Detections
                .Include(d => d.Camera)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Detection>> GetByCameraIdAsync(string cameraId)
        {
            return await _context.Detections
                .Where(d => d.CameraId == cameraId)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Detection>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Detections
                .Where(d => d.Timestamp >= from && d.Timestamp <= to)
                .Include(d => d.Camera)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }

        public async Task<Detection> GetByIdAsync(int id)
        {
            return await _context.Detections
                .Include(d => d.Camera)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Detection> AddAsync(Detection detection)
        {
            detection.Timestamp = DateTime.UtcNow;
            _context.Detections.Add(detection);
            return detection;
        }

        public async Task UpdateAsync(Detection detection)
        {
            var existingDetection = await _context.Detections.FindAsync(detection.Id);
            if (existingDetection != null)
            {
                _context.Entry(existingDetection).CurrentValues.SetValues(detection);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var detection = await _context.Detections.FindAsync(id);
            if (detection != null)
            {
                _context.Detections.Remove(detection);
            }
        }

        public async Task<IEnumerable<Detection>> GetByObjectTypeAsync(string objectType)
        {
            return await _context.Detections
                .Where(d => d.ObjectType.ToLower() == objectType.ToLower())
                .Include(d => d.Camera)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Detection>> GetByConfidenceRangeAsync(float minConfidence, float maxConfidence)
        {
            return await _context.Detections
                .Where(d => d.Confidence >= minConfidence && d.Confidence <= maxConfidence)
                .Include(d => d.Camera)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }

        public async Task<int> GetDetectionCountByCameraAsync(string cameraId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Detections.Where(d => d.CameraId == cameraId);

            if (from.HasValue)
                query = query.Where(d => d.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(d => d.Timestamp <= to.Value);

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Detection>> GetRecentDetectionsAsync(int take = 50)
        {
            return await _context.Detections
                .Include(d => d.Camera)
                .OrderByDescending(d => d.Timestamp)
                .Take(take)
                .ToListAsync();
        }

        public async Task DeleteOldDetectionsAsync(DateTime cutoffDate)
        {
            var oldDetections = await _context.Detections
                .Where(d => d.Timestamp < cutoffDate)
                .ToListAsync();

            _context.Detections.RemoveRange(oldDetections);
        }

        public async Task<IEnumerable<Detection>> GetHighConfidenceDetectionsAsync(float threshold = 0.8f)
        {
            return await _context.Detections
                .Where(d => d.Confidence >= threshold)
                .Include(d => d.Camera)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetDetectionStatsByObjectTypeAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Detections.AsQueryable();

            if (from.HasValue)
                query = query.Where(d => d.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(d => d.Timestamp <= to.Value);

            return await query
                .GroupBy(d => d.ObjectType)
                .Select(g => new { ObjectType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ObjectType, x => x.Count);
        }

        public async Task<IEnumerable<Detection>> GetPagedDetectionsAsync(int pageNumber, int pageSize, string cameraId = null)
        {
            var query = _context.Detections.AsQueryable();

            if (!string.IsNullOrEmpty(cameraId))
                query = query.Where(d => d.CameraId == cameraId);

            return await query
                .Include(d => d.Camera)
                .OrderByDescending(d => d.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }

}
