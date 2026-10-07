using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public PostsController(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ICommentRepository commentRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost(
        [FromBody] CreatePostDto request)
    {
        // throws if the author does not exist, which becomes a 404
        await userRepository.GetSingleAsync(request.UserId);

        Post post = new Post
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId
        };
        Post created = await postRepository.AddAsync(post);
        return Created($"/posts/{created.Id}", ToDto(created, UsernamesById()));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdatePost(
        [FromRoute] int id,
        [FromBody] UpdatePostDto request)
    {
        Post post = await postRepository.GetSingleAsync(id);
        post.Title = request.Title;
        post.Body = request.Body;
        await postRepository.UpdateAsync(post);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDto>> GetPost(
        [FromRoute] int id,
        [FromQuery] bool includeComments = false)
    {
        Post post = await postRepository.GetSingleAsync(id);
        Dictionary<int, string> usernames = UsernamesById();
        PostDto dto = ToDto(post, usernames);
        if (includeComments)
        {
            dto.Comments = CommentsForPost(id, usernames);
        }

        return Ok(dto);
    }

    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> GetPosts(
        [FromQuery] string? title,
        [FromQuery] int? userId,
        [FromQuery] string? username)
    {
        Dictionary<int, string> usernames = UsernamesById();
        IQueryable<Post> posts = postRepository.GetManyAsync();
        if (!string.IsNullOrWhiteSpace(title))
        {
            posts = posts.Where(p => p.Title.Contains(
                title, StringComparison.OrdinalIgnoreCase));
        }

        if (userId is not null)
        {
            posts = posts.Where(p => p.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(username))
        {
            posts = posts.Where(p =>
                usernames.ContainsKey(p.UserId) &&
                usernames[p.UserId].Equals(
                    username, StringComparison.OrdinalIgnoreCase));
        }

        List<PostDto> dtos = posts
            .OrderBy(p => p.Id)
            .Select(p => ToDto(p, usernames))
            .ToList();
        return Ok(dtos);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int id)
    {
        await postRepository.DeleteAsync(id);

        // comments cannot exist without their post
        List<int> commentIds = commentRepository.GetManyAsync()
            .Where(c => c.PostId == id)
            .Select(c => c.Id)
            .ToList();
        foreach (int commentId in commentIds)
        {
            await commentRepository.DeleteAsync(commentId);
        }

        return NoContent();
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForPost(
        [FromRoute] int id)
    {
        await postRepository.GetSingleAsync(id);
        return Ok(CommentsForPost(id, UsernamesById()));
    }

    private List<CommentDto> CommentsForPost(
        int postId, Dictionary<int, string> usernames)
    {
        return commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Id)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                PostId = c.PostId,
                UserId = c.UserId,
                AuthorName = usernames.GetValueOrDefault(c.UserId, "unknown")
            })
            .ToList();
    }

    private Dictionary<int, string> UsernamesById()
    {
        return userRepository.GetManyAsync()
            .ToDictionary(u => u.Id, u => u.Username);
    }

    private static PostDto ToDto(Post post, Dictionary<int, string> usernames)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            AuthorName = usernames.GetValueOrDefault(post.UserId, "unknown")
        };
    }
}
