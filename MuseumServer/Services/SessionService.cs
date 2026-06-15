using MuseumServer.Data;
using MuseumServer.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace MuseumServer.Services
{
    public class SessionService
    {
        private readonly IDbContextFactory<MuseumContext> _dbFactory;
        private readonly string _adminPasswordHash;
        private readonly TimeSpan _sessionLifetime = TimeSpan.FromHours(12);

        public SessionService(IDbContextFactory<MuseumContext> dbFactory)
        {
            _dbFactory = dbFactory;

            var path = Path.Combine(AppContext.BaseDirectory, "Config", "password.txt");

            if (!File.Exists(path))
                throw new FileNotFoundException("Файл с паролем администратора не найден", path);

            _adminPasswordHash = File.ReadAllText(path).Trim();
        }

        public string CreateSession(string userType)
        {
            var token = Guid.NewGuid().ToString();
            using var db = _dbFactory.CreateDbContext();
            db.Sessions.Add(new Session
            {
                Token = token,
                UserType = userType,
                CreatedAt = DateTime.UtcNow,
                LastAccess = DateTime.UtcNow
            });
            db.SaveChanges();
            return token;
        }

        public bool ValidateSession(string token)
        {
            using var db = _dbFactory.CreateDbContext();

            var session = db.Sessions.FirstOrDefault(s => s.Token == token);
            if (session == null)
                return false;

            // Проверка времени
            if (DateTime.UtcNow - session.LastAccess > _sessionLifetime)
                return false;

            // продлеваем сессию
            session.LastAccess = DateTime.UtcNow;
            db.SaveChanges();

            return true;
        }

        public bool ValidateAdminPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(_adminPasswordHash))
            {
                return false;
            }

            //Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(password));

            try
            {
                var result = BCrypt.Net.BCrypt.Verify(password, _adminPasswordHash);
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Session? GetSession(string token)
        {
            using var db = _dbFactory.CreateDbContext();
            var session = db.Sessions.FirstOrDefault(s => s.Token == token);
            if (session != null)
            {
                session.LastAccess = DateTime.UtcNow; // обновляем последнее обращение
                db.SaveChanges();
            }
            return session;
        }

        public void CleanupOldSessions()
        {
            using var db = _dbFactory.CreateDbContext();
            var cutoff = DateTime.UtcNow - _sessionLifetime;
            var oldSessions = db.Sessions.Where(s => s.LastAccess < cutoff).ToList();
            if (oldSessions.Count > 0)
            {
                db.Sessions.RemoveRange(oldSessions);
                db.SaveChanges();
                Console.WriteLine($"Удалено сессий: {oldSessions.Count}");
            }
        }
    }
}