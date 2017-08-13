using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using api_service.Models;
using Microsoft.EntityFrameworkCore;

namespace api_service.Controllers
{
    [Route("api/[controller]"), DisableCors]
    public class CustomerController : ControllerBase
    {
        private readonly SweetPotatoContext _context;

        public CustomerController(SweetPotatoContext context)
        {
            _context = context;
        }

        [HttpGet("{cashTag}")]
        public async Task<IActionResult> Register(string cashTag)
        {
            if (string.IsNullOrWhiteSpace(cashTag))
            {
                return BadRequest();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(t => t.CashTag.Equals(cashTag, StringComparison.CurrentCultureIgnoreCase));

            if (customer == null)
            {
                return NotFound();
            }

            return new ObjectResult(customer);
        }
    }
}
