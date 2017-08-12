using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace api_service.Models
{
    public class SweetPotatoContext : DbContext
    {
        public SweetPotatoContext(DbContextOptions<SweetPotatoContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentRequest> PaymentRequests { get; set; }

        public DbSet<Notification> Notifications { get; set; }
    }
}
