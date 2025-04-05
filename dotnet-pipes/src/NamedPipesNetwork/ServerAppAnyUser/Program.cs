using Common;
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace ServerAppAnyUser
{
    internal static class Program
    {
        private const string PipeName = "pipe038371";
        private static int numThreads = 1;

        private static async Task Main()
        {
            WriteLine("Started.");

            PipeSecurity pipeSecurity = new PipeSecurity();

            // Allow the current user full access.
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            pipeSecurity.AddAccessRule(new PipeAccessRule(user, PipeAccessRights.FullControl, AccessControlType.Allow));

            // Allow built-in administrators full control.
            SecurityIdentifier administratorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            pipeSecurity.AddAccessRule(new PipeAccessRule(administratorsSid, PipeAccessRights.FullControl, AccessControlType.Allow));

            // Allow everyone read/write (less secure, but a simple example)
            SecurityIdentifier everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            pipeSecurity.AddAccessRule(new PipeAccessRule(everyoneSid, PipeAccessRights.FullControl, AccessControlType.Allow));

            try
            {
                using (var pipeServerStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
                numThreads, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, pipeSecurity))
                {
                    int threadId = Thread.CurrentThread.ManagedThreadId;

                    WriteLine("Waiting Client to connect...");
                    await pipeServerStream.WaitForConnectionAsync();

                    WriteLine($"Client connected on thread[{threadId}].");

                    var stringPipe = new StringPipe(pipeServerStream);

                    await stringPipe.WriteStringAsync("SYNC");
                    WriteLine("Client connected.");
                    WriteLine($"Current TransmissionMode: {pipeServerStream.TransmissionMode}");
                    string userInput;

                    do
                    {
                        WriteLine("Enter text ('exit' to quit): ");
                        userInput = Console.ReadLine();
                        await stringPipe.WriteStringAsync(userInput);
                    }
                    while (userInput != null && !userInput.Equals("EXIT", StringComparison.CurrentCultureIgnoreCase));

                    WriteLine("Reading data.");
                    var clientMessage = await stringPipe.ReadStringAsync();
                    WriteLine($"Message from Client: {clientMessage}");

                    WriteLine("Pipe stream exit.");
                }
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
}
