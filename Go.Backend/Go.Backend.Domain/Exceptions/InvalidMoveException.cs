using System;

namespace Go.Backend.Domain.Exceptions
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message) : base(message)
        {
        }
    }
}