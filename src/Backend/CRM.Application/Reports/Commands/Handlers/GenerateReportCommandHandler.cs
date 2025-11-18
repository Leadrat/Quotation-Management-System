using System.Threading.Tasks;
using CRM.Application.Reports.Dtos;
using CRM.Application.Reports.Queries;
using CRM.Application.Reports.Queries.Handlers;

namespace CRM.Application.Reports.Commands.Handlers
{
    public class GenerateReportCommandHandler
    {
        private readonly GenerateCustomReportQueryHandler _queryHandler;

        public GenerateReportCommandHandler(GenerateCustomReportQueryHandler queryHandler)
        {
            _queryHandler = queryHandler;
        }

        public async Task<ReportData> Handle(GenerateReportCommand command)
        {
            var query = new GenerateCustomReportQuery
            {
                ReportType = command.Request.ReportType,
                Filters = command.Request.Filters,
                GroupBy = command.Request.GroupBy,
                SortBy = command.Request.SortBy,
                Limit = command.Request.Limit
            };

            return await _queryHandler.Handle(query);
        }
    }
}

