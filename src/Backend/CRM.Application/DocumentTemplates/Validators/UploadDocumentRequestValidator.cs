using CRM.Application.DocumentTemplates.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace CRM.Application.DocumentTemplates.Validators
{
    public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };
        private const long MaxFileSizeBytes = 52428800; // 50MB

        public UploadDocumentRequestValidator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required")
                .Must(BeValidFileType)
                .WithMessage($"File must be one of: {string.Join(", ", AllowedExtensions)}")
                .Must(BeWithinSizeLimit)
                .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024 / 1024}MB");

            RuleFor(x => x.TemplateType)
                .IsInEnum()
                .WithMessage("Template type must be 'Quotation' or 'ProformaInvoice'");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Template name is required")
                .MaximumLength(100)
                .WithMessage("Template name must not exceed 100 characters");
        }

        private bool BeValidFileType(IFormFile? file)
        {
            if (file == null) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        private bool BeWithinSizeLimit(IFormFile? file)
        {
            if (file == null) return false;
            return file.Length <= MaxFileSizeBytes;
        }
    }
}

