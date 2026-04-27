using MuseumServer.Data;
using MuseumServer.Models;
using Microsoft.EntityFrameworkCore;
using MuseumServer.DTOs;

namespace MuseumServer.Services
{
    public class ExhibitService
    {
        private readonly MuseumContext _context;

        public ExhibitService(MuseumContext context)
        {
            _context = context;
        }

        public async Task<List<ExhibitWithDepartmentResponse>> GetAllExhibitsAsync()
        {
            return await _context.Exhibits
                .Select(e => new ExhibitWithDepartmentResponse
                {
                    ExhibitId = e.ExhibitId,
                    Name = e.Name,

                    Department = e.Department != null
                        ? new DepartmentInfo
                        {
                            DepartmentId = e.Department.DepartmentId,
                            Name = e.Department.Name
                        }
                        : null
                })
                .ToListAsync();
        }

        public async Task<ExhibitFullResponse?> GetExhibitAsync(int id)
        {
            return await _context.Exhibits
                .Where(e => e.ExhibitId == id)
                .Select(e => new ExhibitFullResponse
                {
                    ExhibitId = e.ExhibitId,
                    Name = e.Name,
                    Description = e.Description,
                    Materials = e.Materials,
                    IsPermanent = e.IsPermanent,

                    Department = e.Department != null
                        ? new DepartmentInfo
                        {
                            DepartmentId = e.Department.DepartmentId,
                            Name = e.Department.Name
                        }
                        : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Exhibit> CreateExhibitAsync(Exhibit exhibit)
        {
            _context.Exhibits.Add(exhibit);
            await _context.SaveChangesAsync();
            return exhibit;
        }

        public async Task<bool> UpdateExhibitAsync(Exhibit exhibit)
        {
            var existing = await _context.Exhibits.FindAsync(exhibit.ExhibitId);
            if (existing == null) return false;

            existing.Name = exhibit.Name;
            existing.Description = exhibit.Description;
            existing.Materials = exhibit.Materials;
            existing.IsPermanent = exhibit.IsPermanent;
            existing.ImagePath = exhibit.ImagePath;
            existing.DepartmentId = exhibit.DepartmentId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteExhibitAsync(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return false;

            _context.Exhibits.Remove(exhibit);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetExhibitCountAsync() =>
            await _context.Exhibits.CountAsync();

        public async Task<Exhibit?> GetExhibitEntityAsync(int id)
        {
            return await _context.Exhibits
                .FirstOrDefaultAsync(e => e.ExhibitId == id);
        }
    }
}