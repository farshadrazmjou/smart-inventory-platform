namespace ProductService.Application.Common;

public class ValidationErrorResponse
{
    public bool Success { get; set; } = false;

    public string Message { get; set; }
        = "Validation failed";

    public List<string> Errors { get; set; } = [];
}