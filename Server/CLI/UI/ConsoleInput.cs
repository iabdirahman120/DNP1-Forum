namespace CLI.UI;

public static class ConsoleInput
{
    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                return input;
            }

            Console.WriteLine("Input cannot be empty.");
        }
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            string input = ReadRequired(prompt);
            if (int.TryParse(input, out int number))
            {
                return number;
            }

            Console.WriteLine("Please enter a whole number.");
        }
    }
}
