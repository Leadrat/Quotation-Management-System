using System;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Payments.Commands.Handlers
{
    public class DeletePaymentGatewayConfigCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ILogger<DeletePaymentGatewayConfigCommandHandler> _logger;

        public DeletePaymentGatewayConfigCommandHandler(
            IAppDbContext db,
            ILogger<DeletePaymentGatewayConfigCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> Handle(DeletePaymentGatewayConfigCommand command)
        {
            var config = await _db.PaymentGatewayConfigs
                .FirstOrDefaultAsync(c => c.ConfigId == command.ConfigId);

            if (config == null)
                throw new InvalidOperationException("Payment gateway config not found");

            _db.PaymentGatewayConfigs.Remove(config);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Payment gateway config deleted: {ConfigId}", config.ConfigId);

            return true;
        }
    }
}

