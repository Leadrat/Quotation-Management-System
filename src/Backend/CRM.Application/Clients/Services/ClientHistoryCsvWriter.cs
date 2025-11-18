using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRM.Application.Clients.Dtos;

namespace CRM.Application.Clients.Services
{
    public class ClientHistoryCsvWriter
    {
        private const int MaxRows = 5000;

        public async Task WriteToStreamAsync(Stream stream, IEnumerable<ClientHistoryEntryDto> entries, int maxRows = MaxRows)
        {
            var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
            var count = 0;

            // Write header
            await writer.WriteLineAsync("HistoryId,ClientId,ActionType,ActorUserName,ChangedFields,Reason,CreatedAt,BeforeSnapshot,AfterSnapshot");

            foreach (var entry in entries.Take(maxRows))
            {
                if (count >= maxRows)
                {
                    break;
                }

                var row = new[]
                {
                    EscapeCsv(entry.HistoryId.ToString()),
                    EscapeCsv(entry.ClientId.ToString()),
                    EscapeCsv(entry.ActionType ?? ""),
                    EscapeCsv(entry.ActorDisplayName ?? ""),
                    EscapeCsv(string.Join(";", entry.ChangedFields ?? Array.Empty<string>())),
                    EscapeCsv(entry.Reason ?? ""),
                    EscapeCsv(entry.CreatedAt.ToString("o")),
                    EscapeCsv(entry.BeforeSnapshot?.ToString() ?? ""),
                    EscapeCsv(entry.AfterSnapshot?.ToString() ?? "")
                };

                await writer.WriteLineAsync(string.Join(",", row));
                count++;
            }

            await writer.FlushAsync();
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}

