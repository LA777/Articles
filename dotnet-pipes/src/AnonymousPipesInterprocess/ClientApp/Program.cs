using System.IO.Pipes;

namespace ClientApp;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        WriteLine("Started.");
        if (args.Length > 0)
        {
            var handle = args[0];
            WriteLine($"Handle: {handle}");

            await using var pipeClientStream = new AnonymousPipeClientStream(PipeDirection.In, handle);
            WriteLine($"Current TransmissionMode: {pipeClientStream.TransmissionMode}");
            using StreamReader streamReader = new(pipeClientStream);
            string? text;
            do
            {
                WriteLine("Wait for sync...");
                text = await streamReader.ReadLineAsync();
            }
            while (text is not null && !text.StartsWith("SYNC"));

            do
            {
                text = await streamReader.ReadLineAsync();
                WriteLine($"Echo: {text}");
            }
            while (text is not null && !text.Equals("EXIT", StringComparison.CurrentCultureIgnoreCase));
        }
        WriteLine("Quit.");
    }

    private static void WriteLine(string text)
    {
        Console.WriteLine($"{DateTime.Now:mm.ss.ffffff} | [CLIENT] {text}");
    }
}
