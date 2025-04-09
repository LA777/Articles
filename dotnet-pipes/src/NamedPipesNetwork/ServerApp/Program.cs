using Common;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ServerApp;

internal static class Program
{
    private const string PipeName = "pipe038371";

    [SupportedOSPlatform("windows")]
    private static async Task Main()
    {
        WriteLine("Started.");
        WriteLine(Environment.MachineName);

        var pipeSecurity = new PipeSecurity();

        var currentUserSid = WindowsIdentity.GetCurrent().User;
        if (currentUserSid != null)
        {
            pipeSecurity.AddAccessRule(new PipeAccessRule(currentUserSid, PipeAccessRights.FullControl, AccessControlType.Allow));
        }

        var administratorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        pipeSecurity.AddAccessRule(new PipeAccessRule(administratorsSid, PipeAccessRights.FullControl, AccessControlType.Allow));

        var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        pipeSecurity.AddAccessRule(new PipeAccessRule(everyoneSid, PipeAccessRights.ReadWrite, AccessControlType.Allow));

        try
        {
            await using var pipeServerStream = NamedPipeServerStreamAcl.Create(PipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, pipeSecurity);

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
            WriteLine($"ERROR: {exception.Message}");
        }

        WriteLine("Quit.");
    }

    private static void WriteLine(string text)
    {
        Console.WriteLine($"{DateTime.Now:mm.ss.ffffff} | [SERVER] {text}");
    }
}
