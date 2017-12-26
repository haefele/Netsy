using System;
using System.Diagnostics;
using System.Reflection;

namespace Netsy
{
    public static class Guard
    {
        [DebuggerStepThrough]
        public static void NotNull(object argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        [DebuggerStepThrough]
        public static void NotNullOrWhiteSpace(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentNullException(argumentName);
        }

        [DebuggerStepThrough]
        public static void NotInvalidEnum(object argument, string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);

            if (argument.GetType().GetTypeInfo().IsEnum == false)
                throw new InvalidOperationException("The NotInvalidEnum only works with enum values.");

            if (Enum.IsDefined(argument.GetType(), argument) == false)
                throw new ArgumentException("Unknown enum value.", argumentName);
        }
    }
}