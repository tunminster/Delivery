using System.Collections.Generic;
using System.Collections.ObjectModel;
using Delivery.Azure.Library.WebApi.Files;
using Delivery.Domain.Validators;
using FluentValidation;

namespace Delivery.Driver.Domain.Validators.DriverProfile
{
    public class DriverProfileFileUploadValidator : AbstractValidator<ReadOnlyCollection<DisposableFormFile>>
    {
        public DriverProfileFileUploadValidator(List<string> fileTypes, long maximumFileSize)
        {
            RuleFor(x => x.Count).NotEqual(toCompare: 0).WithMessage("A file attached is required.");

            RuleForEach(x => x).SetValidator(new FileValidator(fileTypes));

            RuleFor(x => x).SetValidator(new FileSizeValidator(maximumFileSize));
        }
    }
}