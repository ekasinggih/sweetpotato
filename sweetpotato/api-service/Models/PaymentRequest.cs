﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api_service.Models
{
    public class PaymentRequest
    {
        public int Id { get; set; }
        public int RequesterUserId { get; set; }
        public string RequesterCashTag { get; set; }
        public int PayerUserId { get; set; }
        public string PayerCashTag { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNo { get; set; }
        public DateTime ExpiredTime { get; set; }
        public bool IsSettled { get; set; }
    }
}