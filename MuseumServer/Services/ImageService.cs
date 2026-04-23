using MuseumServer.Services.Base;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MuseumServer.Services
{
    public class ImageService : FileServiceBase
    {
        private readonly string _rootPath;

        public ImageService(IWebHostEnvironment env)
        {
            _rootPath = env.WebRootPath;
        }

        public async Task<(string fileName, string thumbnailName)> SaveImageWithThumbnailAsync(
            IFormFile file,
            string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var imagesPath = Path.Combine(_rootPath, folder, "images");
            var thumbsPath = Path.Combine(_rootPath, folder, "thumbnails");

            Directory.CreateDirectory(imagesPath);
            Directory.CreateDirectory(thumbsPath);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            var fileName = GenerateFileName(ext);
            fileName = EnsureUniqueFileName(imagesPath, fileName);

            var imagePath = Path.Combine(imagesPath, fileName);
            var thumbPath = Path.Combine(thumbsPath, fileName);

            // оригинал
            await SaveToDiskAsync(imagePath, file);

            // thumbnail
            using (var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(200, 200),
                    Mode = ResizeMode.Max
                }));

                await image.SaveAsync(thumbPath);
            }

            return (fileName, fileName);
        }
    }
}