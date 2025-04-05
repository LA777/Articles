using Common;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace ServerApp;

internal static class Program
{
    private const string PipeName = "pipe72346";

    private static async Task Main()
    {
        // IMPORTANT: Build ClientApp before running ServerApp
        WriteLine("Started.");
        string clientAppPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            const string clientAppRelativePath = @"..\..\..\..\ClientApp\bin\Debug\net8.0\ClientApp.exe";
            clientAppPath = Path.GetFullPath(clientAppRelativePath);
            WriteLine("Running on Windows.");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            const string clientAppRelativePathLinux = "../../../../ClientApp/bin/Debug/net8.0/ClientApp";
            clientAppPath = Path.GetFullPath(clientAppRelativePathLinux);
            WriteLine("Running on Linux.");
        }
        else
        {
            throw new NotSupportedException("OS is not supported.");
        }

        if (!File.Exists(clientAppPath))
        {
            throw new FileNotFoundException(clientAppPath);
        }

        try
        {
            await using var pipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut);

            var startInfo = new ProcessStartInfo
            {
                FileName = clientAppPath,
                UseShellExecute = false
            };
            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();

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

            await process.WaitForExitAsync();
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
