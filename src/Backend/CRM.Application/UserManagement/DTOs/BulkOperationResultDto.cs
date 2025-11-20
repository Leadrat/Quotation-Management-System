using System;
using System.Collections.Generic;

namespace CRM.Application.UserManagement.DTOs;

public class BulkOperationResultDto
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkOperationItemResultDto> Results { get; set; } = new();
}

public class BulkOperationItemResultDto
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

