using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CRM.Application.Clients.Services
{
    public class ClientHistoryDiffBuilder
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private static readonly HashSet<string> MaskedFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "passwordhash",
            "secret",
            "token",
            "apiKey",
            "accessToken"
        };

        public ClientHistoryDiffResult Build(IDictionary<string, object?>? before, IDictionary<string, object?>? after)
        {
            var safeBefore = Mask(before);
            var safeAfter = Mask(after);

            var changed = DetermineChangedFields(safeBefore, safeAfter);

            return new ClientHistoryDiffResult
            {
                BeforeSnapshotJson = SerializeSnapshot(safeBefore),
                AfterSnapshotJson = SerializeSnapshot(safeAfter),
                ChangedFields = changed
            };
        }

        private static Dictionary<string, object?>? Mask(IDictionary<string, object?>? input)
        {
            if (input == null) return null;

            var clone = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in input)
            {
                clone[kvp.Key] = MaskedFields.Contains(kvp.Key)
                    ? "***"
                    : kvp.Value;
            }
            return clone;
        }

        private static string? SerializeSnapshot(Dictionary<string, object?>? snapshot)
        {
            if (snapshot == null || snapshot.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(snapshot, SerializerOptions);
        }

        private static List<string> DetermineChangedFields(Dictionary<string, object?>? before, Dictionary<string, object?>? after)
        {
            var changes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (before != null)
            {
                foreach (var kvp in before)
                {
                    changes.Add(kvp.Key);
                }
            }

            if (after != null)
            {
                foreach (var kvp in after)
                {
                    changes.Add(kvp.Key);
                }
            }

            if (before != null && after != null)
            {
                foreach (var key in before.Keys.Intersect(after.Keys, StringComparer.OrdinalIgnoreCase))
                {
                    var beforeValue = before[key]?.ToString() ?? string.Empty;
                    var afterValue = after[key]?.ToString() ?? string.Empty;
                    if (string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
                    {
                        changes.Remove(key);
                    }
                }
            }

            return changes.ToList();
        }
    }

    public class ClientHistoryDiffResult
    {
        public string? BeforeSnapshotJson { get; set; }
        public string? AfterSnapshotJson { get; set; }
        public IReadOnlyList<string> ChangedFields { get; set; } = Array.Empty<string>();
    }
}

