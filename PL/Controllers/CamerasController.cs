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
    [ApiController]
    [Route("api/[controller]")]
    public class CamerasController : ControllerBase
    {
        private readonly IDataProtector _protector;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRtspUrlBuilder _urlBuilder;
        private readonly UserManager<AppUser> _userManager;

        public CamerasController(IUnitOfWork unitOfWork,
            IDataProtectionProvider dp, 
            IRtspUrlBuilder urlBuilder,
            UserManager<AppUser> userManager)
        {
           
            _protector = dp.CreateProtector("cam-secrets");
            _unitOfWork = unitOfWork;
            _urlBuilder = urlBuilder;
            _userManager = userManager;
        }


        #region Create

        [Authorize(Roles = SD.ManagerRole)]
        [HttpPost]
        public IActionResult Create([FromBody] CreateCameraDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cam = new Camera
            {
                Name = dto.Name,
                Host = dto.Host,
                Port = dto.Port == 0 ? 554 : dto.Port,
                Username = dto.Username,
                PasswordEnc = _protector.Protect(dto.PasswordEnc),
                RtspPath = dto.RtspPath,
                Enabled = dto.Enabled,
                UserId = userId
            };

            _unitOfWork.Repository<Camera>().Add(cam);
            _unitOfWork.Complete();
            return CreatedAtAction(nameof(GetById), new { id = cam.Id }, new { cam.Id });
        }

        #endregion

        #region GetById

        // Details
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var cam = _unitOfWork.Repository<Camera>().Get(id);

            if (cam == null)
                return NotFound();

            return Ok(new
            {
                cam.Id,
                cam.Name,
                cam.Status,
                cam.HlsPublicUrl,
                cam.Enabled,
            });
        }

        #endregion

        #region For Flutter

        #region GetHls 

        [Authorize]
        [HttpGet("{id}/hls")]
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

        #region GetAllHls

        [Authorize]
        [HttpGet("hls")]
        public async Task<IActionResult> GetAllHlsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();


            var spec = new BaseSpecification<Camera>
              (cam => cam.UserId == userId && cam.Enabled && !string.IsNullOrEmpty(cam.HlsPublicUrl));
            if (User.IsInRole(SD.ObserverRole))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user?.ManagerId == null)
                    return NotFound("Observer has no manager assigned.");
                spec = new BaseSpecification<Camera>
                (cam => cam.UserId == user.ManagerId && cam.Enabled && !string.IsNullOrEmpty(cam.HlsPublicUrl));
            }
           
            var cams = _unitOfWork.Repository<Camera>().GetAllWithSpec(spec)
                .Select(cam => new { cam.Id, url = cam.HlsPublicUrl })
                .ToList();

            if (!cams.Any())
                return NotFound("No active streams available.");

            return Ok(cams);
        }

        #endregion

        #endregion

        #region  For AI

        #region GetRtsp

        [HttpGet("{id}/rtsp")]
        public IActionResult GetRtsp(int id)
        {
            var cam = _unitOfWork.Repository<Camera>().Get(id);

            if (cam == null)
                return NotFound();

            var pwd = _protector.Unprotect(cam.PasswordEnc);
            var rtsp = _urlBuilder.Build(cam,pwd);

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

            var spec = new BaseSpecification<Camera>(cam => cam.UserId == userId);
            if (User.IsInRole(SD.ObserverRole))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user?.ManagerId == null)
                    return NotFound("Observer has no manager assigned.");
                spec = new BaseSpecification<Camera>(cam => cam.UserId == user.ManagerId );
            }

            var cams = _unitOfWork.Repository<Camera>().GetAllWithSpec(spec)
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
