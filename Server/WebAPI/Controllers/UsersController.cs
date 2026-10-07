using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepository;

    public UsersController(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser(
        [FromBody] CreateUserDto request)
    {
        if (IsUsernameTaken(request.Username, null))
        {
            return Conflict($"Username '{request.Username}' is already taken");
        }

        User user = new User
        {
            Username = request.Username,
            Password = request.Password
        };
        User created = await userRepository.AddAsync(user);
        return Created($"/users/{created.Id}", ToDto(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateUser(
        [FromRoute] int id,
        [FromBody] UpdateUserDto request)
    {
        if (IsUsernameTaken(request.Username, id))
        {
            return Conflict($"Username '{request.Username}' is already taken");
        }

        User user = new User
        {
            Id = id,
            Username = request.Username,
            Password = request.Password
        };
        await userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser([FromRoute] int id)
    {
        User user = await userRepository.GetSingleAsync(id);
        return Ok(ToDto(user));
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers(
        [FromQuery] string? username)
    {
        IQueryable<User> users = userRepository.GetManyAsync();
        if (!string.IsNullOrWhiteSpace(username))
        {
            users = users.Where(u => u.Username.Contains(
                username, StringComparison.OrdinalIgnoreCase));
        }

        List<UserDto> dtos = users
            .OrderBy(u => u.Id)
            .Select(u => ToDto(u))
            .ToList();
        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser([FromRoute] int id)
    {
        await userRepository.DeleteAsync(id);
        return NoContent();
    }

    private bool IsUsernameTaken(string username, int? ownId)
    {
        return userRepository.GetManyAsync().Any(u =>
            u.Id != ownId &&
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username
        };
    }
}
