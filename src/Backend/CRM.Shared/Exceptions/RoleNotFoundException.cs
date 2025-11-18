using System;

namespace CRM.Shared.Exceptions;

public class RoleNotFoundException : Exception
{
    public RoleNotFoundException(string message = "Role not found") : base(message) { }
}
