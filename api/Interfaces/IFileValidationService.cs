using api.Dtos.Track;

namespace api.Interfaces;

public interface IFileValidationService
{
    FileValidationResult ValidateFile(IFormFile file);
}