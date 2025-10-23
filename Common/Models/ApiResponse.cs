using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CircuitBreakerInfo
    {
        public string State { get; set; } = "Closed";
        public int FailureCount { get; set; }
        public int SuccessCount { get; set; }
        public double SuccessRate { get; set; }
        public DateTime? LastFailureTime { get; set; }
        public DateTime? NextAttemptTime { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        // Circuit Breaker Info - useful for demo purposes
        public Dictionary<string, CircuitBreakerInfo>? CircuitBreakerStatus { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}