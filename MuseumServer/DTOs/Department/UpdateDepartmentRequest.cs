namespace MuseumServer.DTOs
{
    // Для изменения существующего отдела
    public class UpdateDepartmentRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}
