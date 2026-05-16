namespace ECommerce.Application.Common.Exceptions;

using System;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}