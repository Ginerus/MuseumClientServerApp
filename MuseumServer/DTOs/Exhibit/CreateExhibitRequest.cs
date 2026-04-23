public class CreateExhibitRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Materials { get; set; }
    public bool IsPermanent { get; set; } = true;
    public int DepartmentId { get; set; }

    public IFormFile Image { get; set; } = null!;  // Изображение
}