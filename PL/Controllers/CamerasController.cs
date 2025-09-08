using BLLProject.Interfaces;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using PL.DTOs;
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
        public CamerasController(IUnitOfWork unitOfWork, IDataProtectionProvider dp)
        {
            _protector = dp.CreateProtector("cam-secrets");
            _unitOfWork = unitOfWork;
        }

        #region Create

        [Authorize(Roles = SD.ManagerRole)]
        [HttpPost]
        public IActionResult Create([FromBody] CreateCameraDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var monitoredFromDb = _unitOfWork.Repository<MonitoredEntity>()
                .GetEntityWithSpec(new BaseSpecification<MonitoredEntity>(m => m.UserId == userId));

            var cam = new Camera()
            {
                Host = dto.Host,
                Port = dto.Port == 0 ? 554 : dto.Port,
                Username = dto.Username,
                PasswordEnc = _protector.Protect(dto.PasswordEnc),
                RtspPath = dto.RtspPath,
                Enabled = dto.Enabled,
                CameraLocation = dto.CameraLocation,
                MonitoredEntityId = monitoredFromDb.Id
            };

            _unitOfWork.Repository<Camera>().Add(cam);
            _unitOfWork.Complete();

            // return from GetById => id only
            //return CreatedAtAction(nameof(GetById), new { id = cam.Id }, new { cam.Id });
            return CreatedAtAction(nameof(GetById), new { id = cam.Id }, 
                new { cam.Id, cam.Username });
        }

        #endregion

        #region GetById => Details

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
                cam.Username,
                cam.Status,
                cam.HlsPublicUrl,
                cam.Enabled,
            });
        }

        #endregion
    }
}
