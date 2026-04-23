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

        public async Task SaveAsThumbnailAsync(
            IFormFile file,
            string fullPath,
            int width,
            int height)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using var image = await Image.LoadAsync(file.OpenReadStream());

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));

            await image.SaveAsync(fullPath);
        }
    }
}