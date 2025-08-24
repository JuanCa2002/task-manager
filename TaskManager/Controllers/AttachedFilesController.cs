using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Messages.Constants;
using TaskManager.Models.Entities;
using TaskManager.Services;

namespace TaskManager.Controllers
{
    [Route("api/files")]
    public class AttachedFilesController: ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IFileStorage _fileStorage;
        private readonly IUserService _userService;
        public AttachedFilesController(ApplicationDbContext dbContext,
            IFileStorage fileStorage, IUserService userService)
        {
            _dbContext = dbContext;
            _fileStorage = fileStorage;
            _userService = userService;
        }

        [HttpPost("{taskId:int}")]
        public async Task<ActionResult<IEnumerable<AttachedFileEntitty>>> Create(int taskId,
            [FromForm] IEnumerable<IFormFile> files)
        {
            var userId = _userService.GetUserId();

            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == taskId);

            if(task is null)
            {
                return NotFound();
            }

            if(task.UserOwnerId != userId)
            {
                return Forbid();
            }

            var existsAttachedFiles = await _dbContext.AttachedFiles
                .AnyAsync(file => file.TaskId == taskId);

            int maxOrder = 0;

            if(existsAttachedFiles)
            {
                maxOrder = await _dbContext.AttachedFiles
                    .Where(file => file.TaskId == taskId)
                    .Select(file => file.Order)
                    .MaxAsync();
            }

            var result = await _fileStorage.Store(AttachedFilesConstants.Container, files);

            var attachedFiles = result.Select((file, index) => new AttachedFileEntitty()
            {
                Url = file.URL,
                Title = file.Title,
                TaskId = taskId,
                CreationDate = DateTime.UtcNow,
                Order = maxOrder + (index + 1)
            }).ToList();

            _dbContext.AttachedFiles.AddRange(attachedFiles);

            await _dbContext.SaveChangesAsync();

            return Ok(attachedFiles);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] string title)
        {
            var userId = _userService.GetUserId();

            var attachedFile = await _dbContext.AttachedFiles
                .Include(file => file.Task)
                .FirstOrDefaultAsync(file => file.Id == id);

            if(attachedFile is null)
            {
                return NotFound();
            }

            if(attachedFile.Task.UserOwnerId != userId)
            {
                return Forbid();
            }

            attachedFile.Title = title;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var userId = _userService.GetUserId();

            var attachedFile = await _dbContext.AttachedFiles
                .Include(file => file.Task)
                .FirstOrDefaultAsync(file => file.Id == id);

            if(attachedFile is null)
            {
                return NotFound();
            }

            if(attachedFile.Task.UserOwnerId != userId)
            {
                return Forbid();
            }

            _dbContext.AttachedFiles.Remove(attachedFile);

            await _dbContext.SaveChangesAsync();
            await _fileStorage.Delete(attachedFile.Url, AttachedFilesConstants.Container);
            return Ok();
        }

        [HttpPut("sort/{taskId:int}")]
        public async Task<ActionResult> Sort([FromBody] Guid[] ids, int taskId)
        {
            var userId = _userService.GetUserId();

            var task = await _dbContext.Tasks
                .Include(task => task.AttachedFiles)
                .FirstOrDefaultAsync(task => task.Id == taskId && task.UserOwnerId == userId);

            if (task is null)
            {
                return NotFound();
            }

            var attachedFiles = task.AttachedFiles;

            var attachedFileIds = attachedFiles.Select(file => file.Id).ToList();

            var idsDoNotBelogToUser = ids.Except(attachedFileIds).ToList();

            if (idsDoNotBelogToUser.Count > 0)
            {
                return Forbid();
            }

            var attachedDictionary = attachedFiles.ToDictionary(file => file.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var attachedFile = attachedDictionary[id];
                attachedFile.Order = i + 1;
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }


    }
}
