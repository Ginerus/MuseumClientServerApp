using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.Models;

namespace MuseumServer.Services
{
    public class MuseumInfoService
    {
        private readonly MuseumContext _context;

        public MuseumInfoService(MuseumContext context)
        {
            _context = context;
        }

        public async Task<string> GetDescriptionAsync()
        {
            var info = await _context.MuseumInfo.FirstOrDefaultAsync();
            return info?.Description ?? string.Empty;
        }

        public async Task<string> UpdateDescriptionAsync(string description)
        {
            var info = await _context.MuseumInfo.FirstOrDefaultAsync();

            if (info == null)
            {
                // на случай, если строку в БД забыли создать вручную
                info = new MuseumInfo { Description = description };
                _context.MuseumInfo.Add(info);
            }
            else
            {
                info.Description = description;
            }

            await _context.SaveChangesAsync();

            return info.Description ?? string.Empty;
        }

        public async Task<bool> ChangeAdminPasswordAsync( string oldPassword, string newPassword)
        {
            var info = await _context.MuseumInfo.FirstOrDefaultAsync();

            if (info == null)
                return false;

            if (!BCrypt.Net.BCrypt.Verify(
                oldPassword,
                info.AdminPasswordHash))
            {
                return false;
            }

            info.AdminPasswordHash =
                BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}