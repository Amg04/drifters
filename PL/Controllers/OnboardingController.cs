using BLLProject.Interfaces;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PL.DTOs;
using System.Security.Claims;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnboardingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public OnboardingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region SiteSelection

        [Authorize]
        [HttpGet("SiteSelection")]
        public IActionResult SiteSelection()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var spec = new BaseSpecification<MonitoredEntity>(m => m.UserId == userId);
            spec.Includes.Add(t => t.Cameras);
            var yourSites = _unitOfWork.Repository<MonitoredEntity>()
                .GetAllWithSpec(spec)
                .Take(3)
                .Select(m => new YourSitesDto
                {
                    Name = m.EntityName,
                    NumberOfCameraes = m.Cameras.Count
                }).ToList();
           
            SiteSelectionDto dto = new SiteSelectionDto()
            {
                EntityTypes = Enum.GetNames(typeof(EntityTypes)).ToList(),
                EntityName = string.Empty, 
                YourSites = yourSites
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpPost("SiteSelection")]
        public IActionResult SiteSelectionPost([FromBody] SiteSelectionRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var spec = new BaseSpecification<MonitoredEntity>
                (m => m.EntityName == request.EntityName && m.UserId == userId);
            var monitoredFromDb = _unitOfWork.Repository<MonitoredEntity>().GetEntityWithSpec(spec);

            if (monitoredFromDb == null) 
            {
                if (string.IsNullOrEmpty(request.EntityType))
                {
                    ModelState.AddModelError(nameof(request.EntityType), "Site Type must have value");
                }
                if (string.IsNullOrEmpty(request.EntityName))
                {
                    ModelState.AddModelError(nameof(request.EntityName), "Name must have value");
                }

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                MonitoredEntity obj = new MonitoredEntity()
                {
                    EntityName = request.EntityName,
                    UserId = userId,
                    LastUpdate = DateTime.UtcNow
                };

                if (Enum.TryParse<EntityTypes>(request.EntityType, true, out var entityType))
                    obj.EntityType = entityType;
                else
                    return BadRequest("Invalid EntityType value");

                _unitOfWork.Repository<MonitoredEntity>().Add(obj);
                _unitOfWork.Complete();
            }
            else
            {
                monitoredFromDb.LastUpdate = DateTime.UtcNow;
            }
            return Ok(new { Message = "Data received successfully" });
        }

        #endregion

    }
}
