using TaskManager.Models.Dtos;

namespace TaskManager.Services
{
    public interface IFileStorage
    {
        Task Delete(string path, string container);
        Task<FileStorageResult[]> Store(string container, IEnumerable<IFormFile> files);
    }
}
