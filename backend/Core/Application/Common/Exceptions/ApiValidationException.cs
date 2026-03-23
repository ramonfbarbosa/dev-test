using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Exceptions;

public class ApiValidationException : Exception
{
    public ApiValidationException()
        : base("One or more validation failures have occurred.")
    {
        Failures = new Dictionary<string, string>();
    }

    public ApiValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        var propertyNames = failures
            .Select(e => e.PropertyName)
            .Distinct();
        foreach (var propertyName in propertyNames)
        {
            var propertyFailures = failures
                .Where(e => e.PropertyName == propertyName)
                .Select(e => e.ErrorMessage)
                .ToArray();
            foreach (var propertyFailure in propertyFailures)
            {
                Failures.Add(propertyName, propertyFailure);
            }
        }
    }

    public IDictionary<string, string> Failures { get; }
}
