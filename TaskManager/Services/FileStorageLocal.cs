using TaskManager.Models.Dtos;

namespace TaskManager.Services
{
    public class FileStorageLocal : IFileStorage
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccesor;
        public FileStorageLocal(IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccesor = httpContextAccessor;
        }
        public Task Delete(string path, string container)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Task.CompletedTask;
            }

            var fileName = Path.GetFileName(path); 
            var fileDirectory = Path.Combine(_webHostEnvironment.WebRootPath, container, fileName);

            if (File.Exists(fileDirectory))
            {
                File.Delete(fileDirectory);
            }

            return Task.CompletedTask;
        }

        public async Task<FileStorageResult[]> Store(string container, IEnumerable<IFormFile> files)
        {
            var tasks = files.Select(async file =>
            {
                var originalFileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, container);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string path = Path.Combine(folder, fileName);
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    var content = ms.ToArray();
                    await File.WriteAllBytesAsync(path, content);
                }

                var url = $"{_httpContextAccesor.HttpContext!.Request.Scheme}://" +
                $"{_httpContextAccesor.HttpContext.Request.Host}";
                var fileUrl = Path.Combine(url, container, fileName).Replace("\\", "/");

                return new FileStorageResult
                {
                    URL = fileUrl,
                    Title = originalFileName
                };
            });

            var results = await Task.WhenAll(tasks);
            return results;
        }
    }
}
