using BLLProject.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Mvc;
using PL.DTOs;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Index

        [HttpGet]
        public IActionResult Index()
        {
            var allCameras = _unitOfWork.Repository<Camera>().GetAll();

            DashboardDto dashboardDto = new DashboardDto()
            {
                TotalAlerts = _unitOfWork.Repository<CameraDetection>().GetAll().Count(),
                ActiveCameras = allCameras.Count(c => c.Enabled),
                CriticalEvents = allCameras.Sum(c => c.CriticalEvent),
                CameraStatus = allCameras.Select(c => new CameraStatusDto
                {
                    Id = c.Id,
                    UserName = c.Username,
                    Enabled = c.Enabled
                }).ToList()
            };

            return Ok(dashboardDto);
        }

        #endregion

    }
}
