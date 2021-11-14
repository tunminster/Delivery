using System.Collections.Generic;
using System.IO;
using Delivery.Azure.Library.WebApi.Files;
using FluentValidation;

namespace Delivery.Domain.Validators
{
    public class FileValidator : AbstractValidator<DisposableFormFile>
    {
        public FileValidator(List<string> fileTypes)
        {
            RuleFor(x => GetFileType(x.FileName)).NotNull().Must(fileTypes.Contains).WithMessage(x =>
                $"{x.FileName} file extension is not allowed.");
        }

        private static string GetFileType(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            return extension;
        }
    }
}