using System.ComponentModel.DataAnnotations;

namespace AssetServer.Validation;

public class ValidServerPathAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is not string path)
            return false;

        // Making paths required can be driven by [Required] instead
        // If we reject these then it shows 2 validation errors
        if (string.IsNullOrWhiteSpace(path))
            return true;

        return Directory.Exists(path);
    }
}
