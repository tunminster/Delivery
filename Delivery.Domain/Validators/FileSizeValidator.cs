using System.Collections.ObjectModel;
using System.Linq;
using Delivery.Azure.Library.WebApi.Files;
using FluentValidation;

namespace Delivery.Domain.Validators
{
    public class FileSizeValidator : AbstractValidator<ReadOnlyCollection<DisposableFormFile>>
    {
        public FileSizeValidator(long validTotalFileSize)
        {
            RuleFor(x => IsValidFileSize(x, validTotalFileSize)).Must(x => x).WithMessage(
                $"{GetMegabytes(validTotalFileSize)} exceeds the allowance total file size.");
        }

        private static bool IsValidFileSize(ReadOnlyCollection<DisposableFormFile> files, long maxFilesSize)
        {
            var totalFilesSize = files.Sum(x => x.Length);
            var result = totalFilesSize < maxFilesSize;
            return result;
        }

        private static string GetMegabytes(long fileSize)
        {
            var strMegaBytes = (fileSize / 1024 / 1000).ToString();
            return strMegaBytes;
        }
    }
}