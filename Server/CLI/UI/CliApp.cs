using CLI.UI.ManageComments;
using CLI.UI.ManagePosts;
using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    private readonly CreateUserView createUserView;
    private readonly CreatePostView createPostView;
    private readonly CreateCommentView createCommentView;
    private readonly ListPostsView listPostsView;
    private readonly SinglePostView singlePostView;

    public CliApp(IUserRepository userRepository, IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        createUserView = new CreateUserView(userRepository);
        createPostView = new CreatePostView(postRepository, userRepository);
        createCommentView = new CreateCommentView(commentRepository, postRepository, userRepository);
        listPostsView = new ListPostsView(postRepository);
        singlePostView = new SinglePostView(postRepository, commentRepository, userRepository);
    }

    public async Task StartAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Forum ===");
            Console.WriteLine("1. Create user");
            Console.WriteLine("2. Create post");
            Console.WriteLine("3. Add comment to post");
            Console.WriteLine("4. View posts overview");
            Console.WriteLine("5. View single post");
            Console.WriteLine("0. Exit");

            string choice = ConsoleInput.ReadRequired("Choose: ");
            try
            {
                switch (choice)
                {
                    case "1":
                        await createUserView.ShowAsync();
                        break;
                    case "2":
                        await createPostView.ShowAsync();
                        break;
                    case "3":
                        await createCommentView.ShowAsync();
                        break;
                    case "4":
                        listPostsView.Show();
                        break;
                    case "5":
                        await singlePostView.ShowAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Unknown choice.");
                        break;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
