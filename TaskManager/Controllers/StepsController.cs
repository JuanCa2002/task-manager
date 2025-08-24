using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models.Dtos;
using TaskManager.Models.Entities;
using TaskManager.Services;

namespace TaskManager.Controllers
{
    [Route("api/steps")]
    public class StepsController: ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserService _userService;
        public StepsController(ApplicationDbContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        [HttpPost("{taskId:int}")]
        public async Task<ActionResult<StepEntity>> Save([FromBody] StepCreateDTO step, int taskId)
        {
            var userId = _userService.GetUserId();

            var task = await _dbContext.Tasks.FirstOrDefaultAsync(task => task.Id == taskId);

            if (task is null)
            {
                return NotFound();
            }

            if (task.UserOwnerId != userId)
            {
                return Forbid();
            }

            var existsSteps = await _dbContext.Steps.AnyAsync(s => s.TaskId == taskId);

            var maxOrder = 0;

            if (existsSteps) 
            {
                maxOrder = await _dbContext.Steps
                    .Where(s => s.TaskId == taskId)
                    .Select(s => s.Order)
                    .MaxAsync();            
            }

            var newStep = new StepEntity()
            {
                Description = step.Description,
                Done = step.Done,
                Order = maxOrder + 1,
                TaskId = taskId
            };

            _dbContext.Steps.Add(newStep);
            await _dbContext.SaveChangesAsync();

            return Ok(newStep);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StepEntity>> Update(Guid id, [FromBody] StepCreateDTO step)
        {
            var userId = _userService.GetUserId();

            var foundStep = await _dbContext.Steps
                .Include(currentStep => currentStep.Task)
                .FirstOrDefaultAsync(currentStep => currentStep.Id == id);

            if (foundStep is null)
            {
                return NotFound();
            }

            if (foundStep.Task.UserOwnerId != userId)
            {
                return Forbid();
            }

            foundStep.Description = step.Description;
            foundStep.Done = step.Done;

            await _dbContext.SaveChangesAsync();

            return Ok(foundStep);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var userId = _userService.GetUserId();

            var step = await _dbContext.Steps
                .Include(currentStep => currentStep.Task)
                .FirstOrDefaultAsync(currentStep => currentStep.Id == id);

            if(step is null)
            {
                return NotFound();
            }

            if(step.Task.UserOwnerId != userId)
            {
                return Forbid();
            }

            _dbContext.Steps.Remove(step);  
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("sort/{taskId:int}")]
        public async Task<ActionResult> Sort([FromBody] Guid[] ids, int taskId)
        {
            var userId = _userService.GetUserId();

            var task = await _dbContext.Tasks
                .Include(task => task.Steps)
                .FirstOrDefaultAsync(task => task.Id == taskId && task.UserOwnerId == userId);

            if(task is null)
            {
                return NotFound();
            }

            var steps = task.Steps;

            var stepIds = steps.Select(step => step.Id).ToList();

            var idsDoNotBelogToUser = ids.Except(stepIds).ToList();

            if (idsDoNotBelogToUser.Count > 0)
            {
                return Forbid();
            }

            var stepDictionary = steps.ToDictionary(step => step.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var step = stepDictionary[id];
                step.Order = i + 1;
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }



    }
}
