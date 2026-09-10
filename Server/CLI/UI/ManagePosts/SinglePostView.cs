using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class SinglePostView
{
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;
    private readonly IUserRepository userRepository;

    public SinglePostView(IPostRepository postRepository, ICommentRepository commentRepository,
        IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        int postId = ConsoleInput.ReadInt("Post id: ");
        Post post = await postRepository.GetSingleAsync(postId);
        User author = await userRepository.GetSingleAsync(post.UserId);

        Console.WriteLine();
        Console.WriteLine($"{post.Title} (by {author.Username})");
        Console.WriteLine(post.Body);

        List<Comment> comments = commentRepository.GetManyAsync()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Id)
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"Comments ({comments.Count}):");
        foreach (Comment comment in comments)
        {
            User commenter = await userRepository.GetSingleAsync(comment.UserId);
            Console.WriteLine($"  {commenter.Username}: {comment.Body}");
        }
    }
}
