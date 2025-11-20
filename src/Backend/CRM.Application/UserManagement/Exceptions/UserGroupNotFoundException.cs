using System;

namespace CRM.Application.UserManagement.Exceptions;

public class UserGroupNotFoundException : Exception
{
    public UserGroupNotFoundException(Guid groupId) : base($"User group not found: {groupId}") { }
}

