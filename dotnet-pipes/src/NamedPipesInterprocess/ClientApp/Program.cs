﻿using Common;
using System.IO.Pipes;

namespace ClientApp;

internal static class Program
{
    private const string PipeName = "pipe72346";
    private const string ServerName = ".";

    private static async Task Main()
    {
        WriteLine("Started.");
        var cancellationToken = CancellationToken.None;

        try
        {
            await using var pipeClientStream = new NamedPipeClientStream(ServerName, PipeName, PipeDirection.InOut);
            var timeout = TimeSpan.FromSeconds(30);
            await pipeClientStream.ConnectAsync(timeout, cancellationToken);
            WriteLine("Connected to server.");
            WriteLine($"Current TransmissionMode: {pipeClientStream.TransmissionMode}");
            var stringPipe = new StringPipe(pipeClientStream);
            string? text;
            do
            {
                WriteLine("Wait for sync...");
                text = await stringPipe.ReadStringAsync();
            }
            while (!text.StartsWith("SYNC"));
            WriteLine("Synced with server.");
            do
            {
                WriteLine("Reading data.");
                text = await stringPipe.ReadStringAsync();
                WriteLine($"Received: {text}");
            }
            while (!text.Equals("EXIT", StringComparison.CurrentCultureIgnoreCase));

            WriteLine("Write data to Server.");
            await stringPipe.WriteStringAsync("Bye-bye.");
            WriteLine("Writing data to Server complete.");
        }
        catch (Exception exception)
        {
            WriteLine(exception.Message);
        }

        WriteLine("Quit.");
    }

    private static void WriteLine(string text)
    {
        Console.WriteLine($"{DateTime.Now:mm.ss.ffffff} | [CLIENT] {text}");
    }
}
