using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using api_service.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace api_service.Controllers
{
    [Route("api/[controller]"), DisableCors]
    public class PaymentRequestController : Controller
    {
        private readonly SweetPotatoContext _context;

        public PaymentRequestController(SweetPotatoContext context)
        {
            _context = context;
        }

        // GET api/PaymentRequest/{id}
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

        // GET api/PaymentRequest/{cashTag}
        [HttpGet("{cashTag}", Name = "GetPaymentRequestByCashTag")]
        public async Task<IActionResult> GetByCashTag(string cashTag)
        {
            var paymentRequest = await _context.PaymentRequests
                .Where(t => t.PayerCashTag.Equals(cashTag, StringComparison.CurrentCultureIgnoreCase))
                .ToListAsync();

            return new ObjectResult(paymentRequest);
        }

        // GET api/PaymentRequest/{cashTag}
        [HttpGet("settle/{cashTag}")]
        public async Task<IActionResult> GetSettleByCashTag(string cashTag)
        {
            var paymentRequest = await _context.PaymentRequests
                .Where(t => t.PayerCashTag.Equals(cashTag, StringComparison.CurrentCultureIgnoreCase) && t.IsSettled)
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            return new ObjectResult(paymentRequest);
        }

        [HttpGet("outstanding/{cashTag}")]
        public async Task<IActionResult> GetOutstandingByCashTag(string cashTag)
        {
            var paymentRequest = await _context.PaymentRequests
                .Where(t => t.PayerCashTag.Equals(cashTag, StringComparison.CurrentCultureIgnoreCase) && 
                t.ExpiredTime > DateTime.UtcNow && !t.IsSettled)
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            return new ObjectResult(paymentRequest);
        }

        // GET api/PaymentRequest/{cashTag}
        [HttpGet("History/{cashTag}")]
        public async Task<IActionResult> GetHistoryByCashTag(string cashTag)
        {
            var paymentRequest = await _context.PaymentRequests
                .Where(t => t.RequesterCashTag.Equals(cashTag, StringComparison.CurrentCultureIgnoreCase))
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            return new ObjectResult(paymentRequest);
        }

        // POST api/PaymentRequest
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentRequest request)
        {
            Console.WriteLine("api call POST /PaymentRequest");
            if (request == null)
            {
                return BadRequest();
            }

            // check is reference no is supplied, if not auto generate by system
            if (string.IsNullOrWhiteSpace(request.ReferenceNo))
            {
                request.ReferenceNo = GenerateReferenceNo();
            }

            // get PayerUserId
            var payer = await _context.Customers.FirstOrDefaultAsync(
                    t => t.CashTag.Equals(request.PayerCashTag, StringComparison.CurrentCultureIgnoreCase));
            if (payer == null)
            {
                return BadRequest("payer not found");
            }
            request.PayerUserId = payer.Id;
            request.PayerName = payer.Name;

            // get RequesterUserId
            var requester = await _context.Customers.FirstOrDefaultAsync(
                    t => t.CashTag.Equals(request.RequesterCashTag, StringComparison.CurrentCultureIgnoreCase));
            if (requester == null)
            {
                return BadRequest("requester not found");
            }
            request.RequesterUserId = requester.Id;
            request.RequesterName = requester.Name;

            // set transaction time
            request.TransactionTime = DateTime.UtcNow;

            // set expired time
            request.ExpiredTime = DateTime.UtcNow.AddMinutes(5);

            _context.PaymentRequests.Add(request);
            await _context.SaveChangesAsync();

            // send notification to device
            var deviceNotification = await _context.Notifications.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
            if (deviceNotification != null)
            {
                try
                {
                    await SendNotification(request.Id, deviceNotification.Token, "Tagihan Pembayaran",
                                $"Tagihan pembayaran sebesar {request.Amount} dari {request.PayerCashTag}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error sending notification : {ex.Message}");
                }
            }

            return CreatedAtRoute("GetPaymentRequest", new { id = request.Id }, request);
        }

        // POST api/PaymentRequest/Pay/{id}
        [HttpPost("Pay/{id}")]
        public async Task<IActionResult> Pay(long id)
        {
            Console.WriteLine("api call POST /PaymentRequest/Pay/{id}");
            // get payment request
            var paymentRequest = await _context.PaymentRequests.Where(t => t.Id == id).FirstOrDefaultAsync();

            if (paymentRequest == null)
            {
                return NotFound();
            }

            // check is settled
            if (paymentRequest.IsSettled)
            {
                return BadRequest("transaction settled");
            }

            // check is expired
            if (paymentRequest.ExpiredTime < DateTime.UtcNow)
            {
                return BadRequest("transaction expired");
            }

            // set transaction as settled
            paymentRequest.IsSettled = true;
            paymentRequest.SettledTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // send notification to device
            var deviceNotification = await _context.Notifications.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
            if (deviceNotification != null)
            {
                try
                {
                    await SendNotification(paymentRequest.Id, deviceNotification.Token, "Pembayaran Tagihan",
                                $"Tagihan pembayaran sebesar {paymentRequest.Amount} telah di bayarkan");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error sending notification : {ex.Message}");
                }
            }

            return CreatedAtRoute("GetPaymentRequest", new { id = paymentRequest.Id }, paymentRequest);
        }

        private string GenerateReferenceNo()
        {
            return string.Concat(DateTime.UtcNow.ToString("yyyyMMdd_hhmmss_ffff", CultureInfo.InvariantCulture), "-", Guid.NewGuid());
        }

        private const string NOTIFICATION_URL = "https://fcm.googleapis.com/fcm/send";
        private const string SERVER_API_KEY = "AAAAKBdOFwQ:APA91bGX4vUxuDZAfLJ1aZsDXjvzWLWLrwne5LfZrs6Gn0ZZ2jOK1DhrzPSg6CaIe6LDJOBnD60kGnURmqCTKf127o10nmf54LfVwPcB10UWsGSqnfExDbJ4_0ysHGl4AnY74gZtXV0H";
        private const string SENDER_ID = "172189685508";

        private async Task SendNotification(int id, string token, string title, string message)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={SERVER_API_KEY}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Sender", $"id={SENDER_ID}");

            var body = new
            {
                notification = new
                {
                    body = message,
                    title,
                    sound = "default",
                    priority = "high"
                },
                data = new
                {
                    id
                },
                to = token
            };
            var jsonBody = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            Console.WriteLine(jsonBody);
            var response = await client.PostAsync(NOTIFICATION_URL, jsonBody);
            var responseMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine(response);
            Console.WriteLine(responseMessage);
        }
    }
}
;