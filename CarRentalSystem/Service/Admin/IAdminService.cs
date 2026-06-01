using CarRentalSystem.DTO.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Admin;
public interface IAdminService
{
     Task<ActionResult<AdminStats>> GetAdminStatsAsync();
}