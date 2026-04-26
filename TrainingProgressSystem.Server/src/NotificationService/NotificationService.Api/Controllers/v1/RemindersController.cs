using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Interfaces.v1;
using Shared.Kernal.Models;

namespace NotificationService.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class RemindersController(IRemindersService remindersService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var result =  remindersService.GetAllRemindersAsync();
        return result.ToActionResult();
    }
}
