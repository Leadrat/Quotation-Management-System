using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Application.Common.Persistence;
using CRM.Application.Refunds.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Refunds.Commands.Handlers
{
    public class BulkProcessRefundsCommandHandler
    {
        private readonly IAppDbContext _db;
        private readonly ProcessRefundCommandHandler _processRefundHandler;
        private readonly ILogger<BulkProcessRefundsCommandHandler> _logger;

        public BulkProcessRefundsCommandHandler(
            IAppDbContext db,
            ProcessRefundCommandHandler processRefundHandler,
            ILogger<BulkProcessRefundsCommandHandler> logger)
        {
            _db = db;
            _processRefundHandler = processRefundHandler;
            _logger = logger;
        }

        public async Task<BulkProcessRefundsResult> Handle(BulkProcessRefundsCommand command)
        {
            var refundIds = command.Request.RefundIds;
            var results = new List<BulkRefundResult>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var refundId in refundIds)
            {
                try
                {
                    var processCommand = new ProcessRefundCommand
                    {
                        RefundId = refundId
                    };

                    var refundDto = await _processRefundHandler.Handle(processCommand);
                    results.Add(new BulkRefundResult
                    {
                        RefundId = refundId,
                        Success = true,
                        Message = "Refund processed successfully"
                    });
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing refund {RefundId} in bulk operation", refundId);
                    results.Add(new BulkRefundResult
                    {
                        RefundId = refundId,
                        Success = false,
                        Message = ex.Message
                    });
                    failureCount++;
                }
            }

            return new BulkProcessRefundsResult
            {
                TotalProcessed = refundIds.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results
            };
        }
    }

    public class BulkProcessRefundsResult
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BulkRefundResult> Results { get; set; } = new();
    }

    public class BulkRefundResult
    {
        public Guid RefundId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

