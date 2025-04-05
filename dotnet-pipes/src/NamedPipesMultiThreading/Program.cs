using System.IO.Pipes;
using System.Text;

namespace NamedPipesMultiThreading;

internal static class Program
{
    private const string PipeName = "pipe239489";

    private static void Main()
    {
        WriteLine("[MAIN] Application started.");

        var serverThread = new Thread(ServerThread);
        var clientThread = new Thread(ClientThread);

        serverThread.Start();
        clientThread.Start();

        clientThread.Join();
        serverThread.Join();

        WriteLine("[MAIN] Application finished.");
    }

    private static void ServerThread()
    {
        WriteLine("[SERVER] Thread started.");
        try
        {
            using var pipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
            pipeServerStream.WaitForConnection();
            WriteLine("[SERVER] Client connected.");
            WriteLine($"[SERVER] Current TransmissionMode: {pipeServerStream.TransmissionMode}");
            var stringPipe = new StringPipe(pipeServerStream);

            WriteLine("[SERVER] Write data to Client.");
            stringPipe.WriteString("I am the one true server!");
            stringPipe.WriteString("Hello from Server.");
            stringPipe.WriteString("Message 23491284");
            stringPipe.WriteString("Exit");
            WriteLine("[SERVER] Writing data to Client complete.");

            var text = stringPipe.ReadString();
            WriteLine($"[SERVER] Received: {text}.");

            WriteLine($"[SERVER] IsConnected: {pipeServerStream.IsConnected}.");
            WriteLine("[SERVER] Pipe stream exit.");
        }
        catch (IOException exception)
        {
            WriteLine($"[SERVER] Error: {exception.Message}");
        }
        WriteLine("[SERVER] Thread exit.");
    }

    private static void ClientThread()
    {
        WriteLine("[CLIENT] Thread started.");
        try
        {
            using var pipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            var timeout = TimeSpan.FromSeconds(30);
            pipeClientStream.Connect(timeout);
            WriteLine("[CLIENT] Connected to server.");
            WriteLine($"[CLIENT] Current TransmissionMode: {pipeClientStream.TransmissionMode}");

            var stringPipe = new StringPipe(pipeClientStream);
            string? text;
            do
            {
                WriteLine("[CLIENT] Reading data.");
                text = stringPipe.ReadString();
                WriteLine($"[CLIENT] Received: {text}");
            }
            while (text is not null && !text.Equals("EXIT", StringComparison.CurrentCultureIgnoreCase));

            WriteLine("[CLIENT] Write data to Server.");
            stringPipe.WriteString("Hello from Client.");
            WriteLine("[CLIENT] Writing data to Server complete.");

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
