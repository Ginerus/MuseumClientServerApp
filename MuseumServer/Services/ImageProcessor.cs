using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MuseumServer.Services
{
    public class ImageProcessor
    {
        private readonly string _rootPath;

        public ImageProcessor(IWebHostEnvironment env)
        {
            _rootPath = env.WebRootPath;
        }

        public async Task CreateThumbnailAsync(
            string folder,
            string fileName,
            int width,
            int height)
        {
            var fullFolder = Path.Combine(_rootPath, folder);
            var fullPath = Path.Combine(fullFolder, fileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Image not found");

            var thumbFolder = Path.Combine(fullFolder, "thumbnails");
            Directory.CreateDirectory(thumbFolder);

            var thumbPath = Path.Combine(thumbFolder, fileName);

            using var image = await Image.LoadAsync(fullPath);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));

            await image.SaveAsync(thumbPath);
        }
    }
}