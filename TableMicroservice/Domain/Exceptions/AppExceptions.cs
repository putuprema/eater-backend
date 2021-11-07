﻿using System.Net;

namespace Domain.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message) : base((int)HttpStatusCode.BadRequest, message)
        {
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base((int)HttpStatusCode.NotFound, message)
        {
        }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message) : base((int)HttpStatusCode.Forbidden, message)
        {
        }
    }
}