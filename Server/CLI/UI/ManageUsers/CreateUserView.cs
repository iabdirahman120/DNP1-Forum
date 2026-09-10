using Entities;
using RepositoryContracts;

namespace CLI.UI.ManageUsers;

public class CreateUserView
{
    private readonly IUserRepository userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task ShowAsync()
    {
        Console.WriteLine("--- Create user ---");
        string username = ConsoleInput.ReadRequired("Username: ");

        bool taken = userRepository.GetManyAsync()
            .Any(u => u.Username.ToLower() == username.ToLower());
        if (taken)
        {
            Console.WriteLine($"Username '{username}' is already taken.");
            return;
        }

        string password = ConsoleInput.ReadRequired("Password: ");

        User created = await userRepository.AddAsync(new User
        {
            Username = username,
            Password = password
        });
        Console.WriteLine($"User '{created.Username}' created with id {created.Id}.");
    }
}
