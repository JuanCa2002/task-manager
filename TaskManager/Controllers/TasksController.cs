using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models.Dtos;
using TaskManager.Models.Entities;
using TaskManager.Services;

namespace TaskManager.Controllers
{
    [Route("api/tasks")]
    public class TasksController: ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;  
        public TasksController(ApplicationDbContext dbContext, IUserService userService,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskDTO>>> Get()
        {
            var userId = _userService.GetUserId();
            return await _dbContext.Tasks
                .Where(task => task.UserOwnerId == userId)
                .OrderBy(task => task.Order)
                .ProjectTo<TaskDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskEntity?>> GetById(int id)
        {
            var userId = _userService.GetUserId();
            var foundTask = await _dbContext.Tasks
                .Include(task => task.Steps.OrderBy(step => step.Order))
                .Include(task => task.AttachedFiles.OrderBy(file => file.Order))
                .FirstOrDefaultAsync(task => task.Id == id && task.UserOwnerId == userId);

            if( foundTask is null)
            {
                return NotFound();
            }

            return foundTask;
        }

        [HttpPost]
        public async Task<ActionResult<TaskEntity>> Post([FromBody] string title)
        {
            var userId = _userService.GetUserId();

            var existsTasks = await _dbContext.Tasks.AnyAsync(task => task.UserOwnerId == userId);

            var maxOrder = 0;

            if(existsTasks)
            {
                maxOrder = await _dbContext.Tasks
                    .Where(task => task.UserOwnerId == userId)
                    .Select(task => task.Order).MaxAsync();
            }

            var task = new TaskEntity
            {
                Title = title,
                UserOwnerId = userId,
                Order = maxOrder + 1,
                CreationDate = DateTime.UtcNow
            };

            _dbContext.Add(task);
            await _dbContext.SaveChangesAsync();

            return task;
        }

        [HttpPost("sort")]
        public async Task<ActionResult> Sort([FromBody] int[] ids)
        {
            var userId = _userService.GetUserId();

            var tasks = await _dbContext.Tasks
                .Where(task => task.UserOwnerId == userId)
                .ToListAsync();

            var taskIds = tasks.Select(task => task.Id).ToList();

            var idsDoNotBelogToUser = ids.Except(taskIds).ToList();

            if (idsDoNotBelogToUser.Count > 0) 
            {
                return Forbid();
            }

            var taskDictionary = tasks.ToDictionary(task => task.Id);

            for(int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var task = taskDictionary[id];
                task.Order = i + 1;
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateTask([FromBody] TaskUpdateDTO updateTask, int id)
        {
            var userId = _userService.GetUserId();

            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.UserOwnerId == userId);

            if(task is null)
            {
                return NotFound();
            }

            task.Title = updateTask.Title;
            task.Description = updateTask.Description;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = _userService.GetUserId();

            var task = await _dbContext.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.UserOwnerId == userId);

            if (task is null)
            {
                return NotFound();
            }

            _dbContext.Remove(task);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }


    }
}
