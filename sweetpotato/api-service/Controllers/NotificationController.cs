using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_service.Controllers
{
[Route("api/[controller]")]
public class NotificationController : ControllerBase
    {
        private readonly SweetPotatoContext _context;

        public NotificationController(SweetPotatoContext context)
        {
            _context = context;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]Notification request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            _context.Notifications.Add(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
