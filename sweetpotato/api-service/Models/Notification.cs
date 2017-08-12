using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api_service.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string CashTag { get; set; }
        public string Token { get; set; }
    }
}
