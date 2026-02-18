using System;
using System.Collections.Generic;
using System.Linq;

namespace Arctos
{
    public static class ExceptionExtensions
    {
        public static string GetAllMessages(this Exception ex, string separator = " --> ")
        {
            var messages = new List<string>();
            Exception? currentException = ex;

            while (currentException != null)
            {
                if (!string.IsNullOrWhiteSpace(currentException.Message) &&
                    !messages.Contains(currentException.Message))
                {
                    messages.Add(currentException.Message.Trim());
                }

                if (currentException.InnerException != null)
                {
                    currentException = currentException.InnerException;
                }
                else if (currentException is AggregateException aggregateException &&
                         aggregateException.InnerExceptions.Any())
                {
                    foreach (var inner in aggregateException.InnerExceptions)
                    {
                        var innerMessages = inner.GetAllMessages(separator);
                        if (!string.IsNullOrWhiteSpace(innerMessages))
                            messages.Add(innerMessages);
                    }

                    currentException = null;
                }
                else
                {
                    currentException = null;
                }
            }

            return string.Join(separator, messages);
        }
    }
}
