using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepository;
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CommentsController(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository)
    {
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment(
        [FromBody] CreateCommentDto request)
    {
        // both throw if the id is unknown, which becomes a 404
        await postRepository.GetSingleAsync(request.PostId);
        await userRepository.GetSingleAsync(request.UserId);

        Comment comment = new Comment
        {
            Body = request.Body,
            PostId = request.PostId,
            UserId = request.UserId
        };
        Comment created = await commentRepository.AddAsync(comment);
        return Created(
            $"/comments/{created.Id}", ToDto(created, UsernamesById()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateComment(
        [FromRoute] int id,
        [FromBody] UpdateCommentDto request)
    {
        Comment comment = await commentRepository.GetSingleAsync(id);
        comment.Body = request.Body;
        await commentRepository.UpdateAsync(comment);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommentDto>> GetComment([FromRoute] int id)
    {
        Comment comment = await commentRepository.GetSingleAsync(id);
        return Ok(ToDto(comment, UsernamesById()));
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? username,
        [FromQuery] int? postId)
    {
        Dictionary<int, string> usernames = UsernamesById();
        IQueryable<Comment> comments = commentRepository.GetManyAsync();
        if (userId is not null)
        {
            comments = comments.Where(c => c.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            comments = comments.Where(c =>
                usernames.ContainsKey(c.UserId) &&
                usernames[c.UserId].Equals(
                    username, StringComparison.OrdinalIgnoreCase));
        }

        if (postId is not null)
        {
            comments = comments.Where(c => c.PostId == postId);
        }

        List<CommentDto> dtos = comments
            .OrderBy(c => c.Id)
            .Select(c => ToDto(c, usernames))
            .ToList();
        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComment([FromRoute] int id)
    {
        await commentRepository.DeleteAsync(id);
        return NoContent();
    }

    private Dictionary<int, string> UsernamesById()
    {
        return userRepository.GetManyAsync()
            .ToDictionary(u => u.Id, u => u.Username);
    }

    private static CommentDto ToDto(
        Comment comment, Dictionary<int, string> usernames)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            PostId = comment.PostId,
            UserId = comment.UserId,
            AuthorName = usernames.GetValueOrDefault(comment.UserId, "unknown")
        };
    }
}
