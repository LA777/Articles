using Common;
using System.IO.Pipes;

namespace ServerApp;

internal static class Program
{
    private const string PipeName = "pipe038371";

    private static async Task Main()
    {
        WriteLine("Started.");

        try
        {
            await using var pipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
            WriteLine("Waiting Client to connect...");
            await pipeServerStream.WaitForConnectionAsync();

            var stringPipe = new StringPipe(pipeServerStream);
            await stringPipe.WriteStringAsync("SYNC");
            WriteLine("Client connected.");
            WriteLine($"Current TransmissionMode: {pipeServerStream.TransmissionMode}");
            string? userInput;

            do
            {
                WriteLine("Enter text ('exit' to quit): ");
                userInput = Console.ReadLine();
                await stringPipe.WriteStringAsync(userInput);
            }
            while (userInput is not null && !userInput.Equals("EXIT", StringComparison.CurrentCultureIgnoreCase));

            WriteLine("Reading data.");
            var clientMessage = await stringPipe.ReadStringAsync();
            WriteLine($"Message from Client: {clientMessage}");

            WriteLine("Pipe stream exit.");
        }
        catch (IOException exception)
        {
            WriteLine($"Error: {exception.Message}");
        }

        WriteLine("Quit.");
    }

    private static void WriteLine(string text)
    {
        Console.WriteLine($"{DateTime.Now:mm.ss.ffffff} | [SERVER] {text}");
    }
}
