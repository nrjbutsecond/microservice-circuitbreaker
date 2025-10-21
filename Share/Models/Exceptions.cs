using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.common;
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string name, object key)
            : base($"Entity '{name}' with key '{key}' was not found.") { }
    }

    public class ValidationException : Exception
    {
        public List<string> Errors { get; }

        public ValidationException(string message) : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(List<string> errors) : base("Validation failed")
        {
            Errors = errors;
        }
    }


    public class ServiceUnavailableException : Exception
    {
        public string ServiceName { get; }

        public ServiceUnavailableException(string serviceName, string message)
            : base(message)
        {
            ServiceName = serviceName;
        }
    }
