using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CreatePostView(IPostRepository postRepository, IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine("--- Create post ---");
        int userId = ConsoleInput.ReadInt("User id: ");
        await userRepository.GetSingleAsync(userId); // throws if the user does not exist

        string title = ConsoleInput.ReadRequired("Title: ");
        string body = ConsoleInput.ReadRequired("Body: ");

        Post created = await postRepository.AddAsync(new Post
        {
            Title = title,
            Body = body,
            UserId = userId
        });
        Console.WriteLine($"Post '{created.Title}' created with id {created.Id}.");
    }
}
