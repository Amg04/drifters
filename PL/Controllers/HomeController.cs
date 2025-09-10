using BLLProject.Interfaces;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PL.DTOs;
using PL.Services.RtspUrlBuilder;
using System.Security.Claims;
using Utilities;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IDataProtector _protector;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRtspUrlBuilder _urlBuilder;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(IUnitOfWork unitOfWork,
            IDataProtectionProvider dp,
            IRtspUrlBuilder urlBuilder,
            UserManager<AppUser> userManager)
        {

            _protector = dp.CreateProtector("cam-secrets");
            _unitOfWork = unitOfWork;
            _urlBuilder = urlBuilder;
            _userManager = userManager;
        }

        #region For Flutter

        #region GetHls 

        [Authorize]
        [HttpGet("hls/{id}")]
        public IActionResult GetHls(int id)
        {
            var cam = _unitOfWork.Repository<Camera>().Get(id);

            if (cam == null || !cam.Enabled)
                return NotFound();

            if (string.IsNullOrEmpty(cam.HlsPublicUrl))
                return Problem(
                    statusCode: 503,
                    title: "Stream not ready",
                    detail: "The requested stream is currently not available."
                );

            return Ok(new { url = cam.HlsPublicUrl });
        }

        #endregion

        #region LiveCamera

        [Authorize]
        [HttpGet("LiveCamera")]
        public async Task<IActionResult> LiveCamera()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if(User.IsInRole(SD.ObserverRole))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user?.ManagerId == null)
                    return NotFound("Observer has no manager assigned.");
                userId = user?.ManagerId;
            }

            var spec = new BaseSpecification<MonitoredEntity>(m => m.UserId == userId);
            spec.AddOrderByDescending(m => m.LastUpdate);
            var latestMonitored = _unitOfWork.Repository<MonitoredEntity>().GetEntityWithSpec(spec);

            if (latestMonitored == null)
                return NotFound("No monitored entity found.");

            var cameraSpec = new BaseSpecification<Camera>
              (cam => cam.MonitoredEntityId == latestMonitored.Id && cam.Enabled && !string.IsNullOrEmpty(cam.HlsPublicUrl));
          
            var cams = _unitOfWork.Repository<Camera>().GetAllWithSpec(cameraSpec)
                .Select(cam => new { cam.Id, url = cam.HlsPublicUrl})
                .ToList();

            if (!cams.Any())
                return NotFound("No active streams available.");

            return Ok(cams);
        }

        #endregion

        #region CameraDetection

        [HttpPost("CameraDetection")]
        public IActionResult CameraDetection([FromBody] IEnumerable<CameraDetectionDto> results)
        {
            var dangerResults = results
                .Where(item => item.Status != null && (item.Status.ToLower() == "abnormal" || item.Crowd_density > 0.4))
                .ToList();

            var dangerResults_CameraLocationList = new List<DangerResults_CameraLocationDto>();

            foreach (var item in dangerResults)
            {
                var cam = _unitOfWork.Repository<Camera>().Get(item.CameraId);
                cam.Type = (item.Status.ToLower() == "abnormal") ? "abnormal" : "Crowd Density";
                string? camLocation = cam?.CameraLocation;
                var dangerDto = new DangerResults_CameraLocationDto()
                {
                    CameraDetectionDto = item,
                    CameraLocation = camLocation,
                };

                dangerResults_CameraLocationList.Add(dangerDto);

                _unitOfWork.Repository<CameraDetection>().Add((CameraDetection)item);
            }

            _unitOfWork.Complete();

            return Ok(new { DangerResults = dangerResults_CameraLocationList });
        }

        #endregion

        #endregion

        #region  For AI

        #region GetRtsp

        [HttpGet("rtsp/{id}")]
        public IActionResult GetRtsp(int id)
        {
            var cam = _unitOfWork.Repository<Camera>().Get(id);

            if (cam == null)
                return NotFound();

            var pwd = _protector.Unprotect(cam.PasswordEnc);
            var rtsp = _urlBuilder.Build(cam, pwd);

            return Ok(new { url = rtsp });
        }

        #endregion

        #region GetAllRtsp

        [HttpGet("rtsp")]
        public async Task<IActionResult> GetAllRtspAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (User.IsInRole(SD.ObserverRole))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user?.ManagerId == null)
                    return NotFound("Observer has no manager assigned.");
                userId = user?.ManagerId;
            }

            var spec = new BaseSpecification<MonitoredEntity>(m => m.UserId == userId);
            spec.AddOrderByDescending(m => m.LastUpdate);
            var latestMonitored = _unitOfWork.Repository<MonitoredEntity>().GetEntityWithSpec(spec);

            if (latestMonitored == null)
                return NotFound("No monitored entity found.");

            var cameraSpec = new BaseSpecification<Camera>(cam => cam.MonitoredEntityId == latestMonitored.Id);
            var cams = _unitOfWork.Repository<Camera>().GetAllWithSpec(cameraSpec)
                .Where(cam => cam.Enabled)
                .Select(cam =>
                {
                    var pwd = _protector.Unprotect(cam.PasswordEnc);
                    var rtspUrl = _urlBuilder.Build(cam, pwd);
                    return new { cam.Id, url = rtspUrl };
                })
                .ToList();

            if (!cams.Any())
                return NotFound("No active RTSP streams available.");

            return Ok(cams);
        }

        #endregion

        #endregion

        

}
}
