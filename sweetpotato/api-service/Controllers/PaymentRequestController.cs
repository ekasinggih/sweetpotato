using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using api_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_service.Controllers
{
    [Route("api/[controller]")]
    public class PaymentRequestController : Controller
    {
        private readonly SweetPotatoContext _context;

        public PaymentRequestController(SweetPotatoContext context)
        {
            _context = context;
        }

        // GET api/PaymentRequest
        [HttpGet("{id}", Name = "GetPaymentRequest")]
        public async Task<IActionResult> GetById(long id)
        {
            var paymentRequest = await _context.PaymentRequests.FirstOrDefaultAsync(t => t.Id == id);
            if (paymentRequest == null)
            {
                return NotFound();
            }
            return new ObjectResult(paymentRequest);
        }

        // POST api/PaymentRequest
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]PaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            // check is reference no is supplied, if not auto generate by system
            if (!string.IsNullOrWhiteSpace(request.ReferenceNo))
            {
                request.ReferenceNo = GenerateReferenceNo();
            }

            // set expired time
            request.ExpiredTime = DateTime.UtcNow.AddMinutes(5);

            _context.PaymentRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtRoute("GetPaymentRequest", new { id = request.Id }, request);
        }

        private string GenerateReferenceNo()
        {
            return string.Concat(DateTime.UtcNow.ToString("yyyyMMdd_hhmmss_ffff", CultureInfo.InvariantCulture), "-", Guid.NewGuid());
        }
    }
}
