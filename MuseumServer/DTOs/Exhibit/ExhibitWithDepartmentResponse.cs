namespace MuseumServer.DTOs
{
    public class ExhibitWithDepartmentResponse
    {
        public int ExhibitId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DepartmentInfo? Department { get; set; }
    }
}
