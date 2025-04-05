using Microsoft.Win32.SafeHandles;
using System.IO.Pipes;

namespace AnonymousPipesMultiThreading;

internal static class Program
{
    private static void Main()
    {
        WriteLine("[SERVER] Application started.");
        using var pipeServerStream = new AnonymousPipeServerStream(PipeDirection.Out);
        var handle = pipeServerStream.ClientSafePipeHandle;
        var clientThread = new Thread(() => ClientThread(handle));
        clientThread.Start();

        try
        {
            using var streamWriter = new StreamWriter(pipeServerStream);
            WriteLine("[SERVER] Write data to Client.");
            streamWriter.WriteLine("I am the one true server!");
            streamWriter.WriteLine("Hello from Server.");
            streamWriter.WriteLine("Message 23491284");
            streamWriter.WriteLine("EXIT");
            WriteLine("[SERVER] Writing data to Client complete.");
            WriteLine($"[SERVER] IsConnected: {pipeServerStream.IsConnected}.");
        }
        catch (IOException exception)
        {
            WriteLine($"[SERVER] Error: {exception.Message}");
        }

        clientThread.Join();
        WriteLine("[SERVER] Application finished.");
    }

    private static void ClientThread(SafePipeHandle handle)
    {
        WriteLine("[CLIENT] Thread started.");
        try
        {
            using var pipeClientStream = new AnonymousPipeClientStream(PipeDirection.In, handle);
            WriteLine($"[CLIENT] Current TransmissionMode: {pipeClientStream.TransmissionMode}");
            using var streamReader = new StreamReader(pipeClientStream);
            string? text;
            do
            {
                WriteLine("[CLIENT] Reading data.");
                text = streamReader.ReadLine();
                WriteLine($"[CLIENT] Received: {text}");
            }
            while (text is not null && !text.Equals("EXIT", StringComparison.CurrentCultureIgnoreCase));
            WriteLine("[CLIENT] Pipe stream exit.");
        }
        catch (IOException exception)
        {
            WriteLine($"[CLIENT] Error: {exception.Message}");
        }
        WriteLine("[CLIENT] Thread exit.");
    }

    private static void WriteLine(string text)
    {
        Console.WriteLine($"{DateTime.Now:mm.ss.ffffff} | {Environment.CurrentManagedThreadId} | {text}");
    }
}
