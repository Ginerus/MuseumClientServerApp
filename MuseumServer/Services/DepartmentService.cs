using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.DTOs;
using MuseumServer.Models;

namespace MuseumServer.Services
{
    public class DepartmentService
    {
        private readonly MuseumContext _context;

        public DepartmentService(MuseumContext context)
        {
            _context = context;
        }

        public async Task<int> GetCountAsync()
            => await _context.Departments.CountAsync();

        public async Task<List<DepartmentInfo>> GetAllAsync()
        {
            return await _context.Departments
                .Select(d => new DepartmentInfo
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name
                })
                .ToListAsync();
        }

        public async Task<DepartmentContentResponse?> GetContentAsync(int departmentId)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);

            if (department == null)
                return null;

            return new DepartmentContentResponse
            {
                Department = new DepartmentResponse
                {
                    DepartmentId = department.DepartmentId,
                    Name = department.Name,
                    Description = department.Description
                },

                Exhibits = await _context.Exhibits
                    .Where(e => e.DepartmentId == departmentId)
                    .Select(e => new ExhibitResponse
                    {
                        ExhibitId = e.ExhibitId,
                        Name = e.Name,
                    })
                    .ToListAsync(),

                MediaFiles = await _context.MediaFiles
                    .Where(m => m.DepartmentId == departmentId)
                    .Select(m => new MediaFileResponse
                    {
                        MediaFileId = m.MediaFileId,
                        MediaType = m.MediaType,
                    })
                    .ToListAsync(),

                Documents = await _context.Documents
                    .Where(d => d.DepartmentId == departmentId)
                    .Select(d => new DocumentResponse
                    {
                        DocumentId = d.DocumentId,
                        Title = d.Title,
                        FileType = d.FileType,
                    })
                    .ToListAsync()
            };
        }
        public async Task<Department> CreateAsync(CreateDepartmentRequest request, string? imageName)
        {
            var dept = new Department
            {
                Name = request.Name,
                Description = request.Description,
                ImagePath = imageName
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            return dept;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var dept = await _context.Departments.FindAsync(id);

            if (dept == null)
                return false;

            _context.Departments.Remove(dept);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(int id, UpdateDepartmentRequest request)
        {
            var dept = await _context.Departments.FindAsync(id);

            if (dept == null)
                return false;

            if (!string.IsNullOrWhiteSpace(request.Name))
                dept.Name = request.Name;

            if (request.Description != null)
                dept.Description = request.Description;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}