# C# .Net Pipes

For 12 years working as a Full Stack software developer, I have never used Pipes in the commercial projects. Nevertheless, I have used Pipes in my Pet projects for Console applications.
Pipes are used to communication between Processes and standalone applications. Also, Pipes can be used for communication between threads. Moreover, Pipes can be used for communication via network.
There are two types of Pipes: Anonymous pipes and Named Pipes. Anonymous pipes require less overhead than named pipes but offer limited services. Anonymous pipes are useful for communication between threads, or between parent and child processes where the pipe handles can be easily passed to the child process when it is created.

| Feature | Anonymous Pipes | Named Pipes | Comments |
|----------------------------|-------------------------------------------------|---------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------|
| **Direction of Communication** | One-way only (parent to child) | One-way or Duplex (bidirectional) | One-way pipes are simpler; duplex allows for more complex interactions. |
| **Server Instance** | Single only (created by parent) | Single or Multiple (can be created by any process with sufficient privileges)   | Named pipes support multiple server instances, enabling more flexible server-client architectures. |
| **Client Instance** | Single only (child process inherits handles) | Single or Multiple (can connect to any available server instance) | Named pipes allow multiple clients to connect to a single or multiple server instances. |
| **Interprocess Communication (IPC)** | Yes (between parent and child processes) | Yes (between any processes on the same machine) | Both facilitate IPC, but named pipes offer broader scope. |
| **Thread Communication** | Yes (within a process, if handles are shared) | Yes (within a process, or between processes) | Handles need to be properly passed for thread communication. |
| **Internetwork Communication** | No | Yes (with appropriate network configuration, via SMB) | Named pipes, with network configuration, can be used for communication across a network, typically using the Server Message Block (SMB) protocol. |
| **Pipe Handles** | Required (inherited by child process) | No (accessed by name) | Anonymous pipes rely on file handles; named pipes are accessed using a path-like name. |
| **Impersonation Support** | No | Yes (server can impersonate the client) | Impersonation allows the server to act on behalf of the client, useful for access control. |
| **Transmission Mode** | Byte stream | Byte stream or Message (discrete packets) | Named pipes support message mode, which preserves message boundaries. |
| **Security** | Inherited from parent process | <span style="background-color:brown;">Configurable Security Descriptor (SD)</span> | Named pipes allow for finer-grained security control. |
| **Persistence** | Ephemeral (destroyed when last handle closes) | Persistent (can exist beyond the lifetime of the creating process) | Named pipes can be designed to persist, while anonymous pipes are inherently temporary. |
| **Naming Convention** | None (handles only) | <span style="background-color:brown;">`\\.\pipe\<pipename>` (local) or `\\<servername>\pipe\<pipename>` (network) </span>| Named pipes are identified by their names, allowing for easy discovery and connection. |

### Security Considerations

Named pipes can be secured using Windows security features. When creating a named pipe, you can specify a <span style="background-color:brown;">security descriptor</span> that controls who can connect to the pipe, who can read from or write to the pipe, etc.

## Anonymous pipes

C# provides the `System.IO.Pipes` namespace for working with pipes. The `AnonymousPipeServerStream` and `AnonymousPipeClientStream` classes facilitate creating and managing anonymous pipes.
`AnonymousPipeServerStream` has several constructors:

```csharp
public AnonymousPipeServerStream() : this(PipeDirection.Out, HandleInheritability.None, 0)
```

```csharp
public AnonymousPipeServerStream(PipeDirection direction) : this(direction, HandleInheritability.None, 0)
```

```csharp
public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability) : this(direction, inheritability, 0)
```

```csharp
// Create an AnonymousPipeServerStream from two existing pipe handles.
public AnonymousPipeServerStream(PipeDirection direction, SafePipeHandle serverSafePipeHandle, SafePipeHandle clientSafePipeHandle) : base(direction, 0)
```

```csharp
// bufferSize is used as a suggestion; specify 0 to let OS decide
// This constructor instantiates the PipeSecurity using just the inheritability flag
public AnonymousPipeServerStream(PipeDirection direction, HandleInheritability inheritability, int bufferSize)
 : base(direction, bufferSize)
```

`PipeDirection` enum has three options: `In`, `Out` and `InOut`. `InOut` is not supported by Anonymous Pipes and throws exception.
`HandleInheritability` - specifies whether the underlying handle is inheritable by child processes.

`AnonymousPipeClientStream` constructors:

```csharp
public AnonymousPipeClientStream(string pipeHandleAsString) : this(PipeDirection.In, pipeHandleAsString)
```

```csharp
public AnonymousPipeClientStream(PipeDirection direction, string pipeHandleAsString) : base(direction, 0)
```

```csharp
public AnonymousPipeClientStream(PipeDirection direction, SafePipeHandle safePipeHandle) : base(direction, 0)
```

There are two ways how to pass Pipe Handle: as string and as `SafePipeHandle` object. After passing Handle to Client – Handle needs to be disposed:

```csharp
pipeServerStream.DisposeLocalCopyOfClientHandle();
```

Starting .NET 8, the Client Handle owned by a server that was created for out-of-proc communication is disposed by `AnonymousPipeServerStream.Dispose()` if it's not exposed by using the `AnonymousPipeServerStream.ClientSafePipeHandle` property. (You create a server for out-of-proc communication by passing `HandleInheritability.Inheritable` to the `AnonymousPipeServerStream(PipeDirection, HandleInheritability)` constructor.)

## Anonymous pipes – Interprocess communication (IPC)

Let's illustrate how to use anonymous pipes to enable communication between a parent and child process.

<details>
  <summary>Server Application</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/AnonymousPipesInterprocess/ServerApp/Program.cs

</details>

<details>
  <summary>Client Application</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/AnonymousPipesInterprocess/ClientApp/Program.cs

</details>

### Explanation

1. Parent Process (Server):
   * Creates an AnonymousPipeServerStream with PipeDirection.Out (write-only).
   * Retrieves the client handle using GetClientHandleAsString().
   * Starts the child process, passing the client handle as a command-line argument.
   * Dispose the client handle after running child process.
   * Writes data to the pipe using a StreamWriter.
   * Waits for the child process to exit.

2. Child Process (Client):
   * Receives the client handle from the command-line arguments.
   * Creates an AnonymousPipeClientStream with PipeDirection.In (read-only).
   * Reads data from the pipe using a StreamReader.
   * Writes the recieved data to the console.

### Console output

<details>
  <summary>Windows</summary>

```text
31.47.381944 | [SERVER] Started.
31.47.415210 | [SERVER] Running on Windows.
31.47.430859 | [SERVER] Current TransmissionMode: Byte
31.47.469036 | [SERVER] Enter text ('exit' to quit):
31.47.526476 | [CLIENT] Started.
31.47.558210 | [CLIENT] Handle: 904
31.47.567850 | [CLIENT] Current TransmissionMode: Byte
31.47.568273 | [CLIENT] Wait for sync...
hello from server
32.01.598822 | [CLIENT] Echo: hello from server
32.01.626956 | [SERVER] Enter text ('exit' to quit):
user message
32.09.478972 | [CLIENT] Echo: user message
32.09.479289 | [SERVER] Enter text ('exit' to quit):
exit
32.11.346058 | [CLIENT] Echo: exit
32.11.358982 | [CLIENT] Quit.
32.11.392888 | [SERVER] Quit.
```

</details>

<details>
  <summary>Linux</summary>

```text
27.23.780827 | [SERVER] Started.
27.23.883141 | [SERVER] Running on Linux.
27.23.896880 | [SERVER] Current TransmissionMode: Byte
27.23.940629 | [CLIENT] Started.
27.23.992946 | [CLIENT] Handle: 62
27.23.999070 | [CLIENT] Current TransmissionMode: Byte
27.24.001885 | [CLIENT] Wait for sync...
27.23.999800 | [SERVER] Enter text ('exit' to quit):
hello world
27.37.251346 | [CLIENT] Echo: hello world
27.37.257623 | [SERVER] Enter text ('exit' to quit):
message from user
27.45.853176 | [SERVER] Enter text ('exit' to quit):
27.45.855195 | [CLIENT] Echo: message from user
exit
27.48.022561 | [CLIENT] Echo: exit
27.48.028410 | [CLIENT] Quit.
27.48.042115 | [SERVER] Quit.
```

</details>

## Anonymous Pipes – Multi Threading Communication

Quite similar but with some changes we can use Anonymous Pipe for communication between threads.

<details>
  <summary>Code example</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/AnonymousPipesMultiThreading/Program.cs

</details>

### Explanation

1. Main Thread (Server):
   * Creates an AnonymousPipeServerStream with PipeDirection.Out (write-only).
   * In this case I decided to use SafePipeHandle. It is still possible to use handle as string though.
   * Starts the child thread, passing the client handle as an object.
   * No Dispose the client handle after running child process.
   * Writes data to the pipe using a StreamWriter.
   * Waits for the child thread to complete.
2. Child Thread (Client):
   * Receives the client handle object.
   * Creates an AnonymousPipeClientStream with PipeDirection.In (read-only).
   * Reads data from the pipe using a StreamReader.
   * Writes the recieved data to the console.

### Console output

<details>
  <summary>Windows</summary>

```text
04.11.954792 | 1 | [SERVER] Application started.
04.11.989560 | 1 | [SERVER] Write data to Client.
04.11.989734 | 1 | [SERVER] Writing data to Client complete.
04.11.989836 | 1 | [SERVER] IsConnected: True.
04.11.990201 | 11 | [CLIENT] Thread started.
04.12.013954 | 11 | [CLIENT] Current TransmissionMode: Byte
04.12.014303 | 11 | [CLIENT] Reading data.
04.12.014657 | 11 | [CLIENT] Received: I am the one true server!
04.12.031185 | 11 | [CLIENT] Reading data.
04.12.031316 | 11 | [CLIENT] Received: Hello from Server.
04.12.031390 | 11 | [CLIENT] Reading data.
04.12.031446 | 11 | [CLIENT] Received: Message 23491284
04.12.031514 | 11 | [CLIENT] Reading data.
04.12.031569 | 11 | [CLIENT] Received: EXIT
04.12.031625 | 11 | [CLIENT] Pipe stream exit.
04.12.031808 | 11 | [CLIENT] Thread exit.
04.12.037822 | 1 | [SERVER] Application finished.
```

</details>

<details>
  <summary>Linux</summary>

```text
05.55.008780 | 1 | [SERVER] Application started.
05.55.107474 | 1 | [SERVER] Write data to Client.
05.55.107570 | 1 | [SERVER] Writing data to Client complete.
05.55.107599 | 1 | [SERVER] IsConnected: True.
05.55.108473 | 10 | [CLIENT] Thread started.
05.55.116022 | 10 | [CLIENT] Current TransmissionMode: Byte
05.55.116188 | 10 | [CLIENT] Reading data.
05.55.166753 | 10 | [CLIENT] Received: I am the one true server!
05.55.192495 | 10 | [CLIENT] Reading data.
05.55.196868 | 10 | [CLIENT] Received: Hello from Server.
05.55.198572 | 10 | [CLIENT] Reading data.
05.55.198620 | 10 | [CLIENT] Received: Message 23491284
05.55.198635 | 10 | [CLIENT] Reading data.
05.55.198647 | 10 | [CLIENT] Received: EXIT
05.55.198656 | 10 | [CLIENT] Pipe stream exit.
05.55.198715 | 10 | [CLIENT] Thread exit.
05.55.200630 | 1 | [SERVER] Application finished.
```

</details>

* Key Considerations:
  * Directionality: Anonymous pipes are unidirectional. If bidirectional communication is required, you'll need to create two pipes.
  * Inheritance: The HandleInheritability.Inheritable option ensures that the child process can access the pipe handle.
  * Error Handling: In a production environment, robust error handling is crucial.
  * Synchronization: For more complex communication scenarios, you might need to implement synchronization mechanisms to prevent race conditions.
  * Security: Anonymous pipes are less secure than named pipes, as they rely on handle inheritance.
  * Lifetime: Anonymous pipes are temporary and are destroyed when all associated handles are closed.
* Practical Applications:
  * Parent-Child Communication: As demonstrated in the example, they're ideal for passing data between a parent and its child process.
  * Simple Task Delegation: A parent process can delegate tasks to a child process and receive results via an anonymous pipe.
  * Logging: A process can send log messages to another process for centralized logging.
  * Data Streaming: Passing data streams between closely related processes.
* Limitations:
  * Local machine only.
  * Unidirectional.
  * Limited security.

### Anonymous pipes – Conclusion

Anonymous pipes provide a simple and efficient way to implement basic IPC in C#. While they have limitations, they are well-suited for scenarios involving closely related processes that require unidirectional data transfer. For more complex communication needs, consider using named pipes or other IPC mechanisms.

## Named pipes

The .NET Framework 3.5 introduced support for named pipes. Named pipes provide a more powerful and flexible mechanism for inter-process communication (IPC) than anonymous pipes. While anonymous pipes are limited to communication between related processes (parent-child) on the same machine, named pipes allow communication between processes on the same machine or across a network.

#### Key characteristics of Named Pipes

* **Duplex Communication:** Named pipes support both one-way and duplex (bidirectional) communication.
* **Multiple Instances:** A named pipe server can support multiple client connections.
* **Network Communication:** Named pipes can be used for communication between processes on different machines over a network (though this has security implications and requires careful configuration).
* **Security:** Named pipes offer robust security features. You can control access to named pipes using Windows security descriptors, allowing you to specify which users or groups can connect, read, or write.
* **Transmission Modes:** Named pipes support both byte and message transmission modes. Message mode preserves message boundaries, which can be crucial in some applications.

The `System.IO.Pipes` namespace provides the `NamedPipeServerStream` and `NamedPipeClientStream` classes for working with named pipes.
`NamedPipeServerStream` constructors:

```csharp
public NamedPipeServerStream(string pipeName)
    : this(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, HandleInheritability.None)
```

```csharp
public NamedPipeServerStream(string pipeName, PipeDirection direction)
    : this(pipeName, direction, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, HandleInheritability.None)
```

```csharp
public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances)
    : this(pipeName, direction, maxNumberOfServerInstances, PipeTransmissionMode.Byte, PipeOptions.None, 0, 0, HandleInheritability.None)
```

```csharp
public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode)
    : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, PipeOptions.None, 0, 0, HandleInheritability.None)
```

```csharp
public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options)
    : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, 0, 0, HandleInheritability.None)
```

```csharp
public NamedPipeServerStream(string pipeName, PipeDirection direction, int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options, int inBufferSize, int outBufferSize)
    : this(pipeName, direction, maxNumberOfServerInstances, transmissionMode, options, inBufferSize, outBufferSize, HandleInheritability.None)
```

`PipeDirection`: Specifies the direction of data flow for the pipe. Options include: `In`, `Out` or `InOut` (duplex).
`maxNumberOfServerInstances`: Determines the maximum number of concurrent server instances that can be created for a named pipe.
`PipeTransmissionMode`: Defines how data is transmitted through the pipe (`Byte` or `Message`).
`PipeOptions`: A set of flags from the `PipeOptions` enumeration that configure the pipe's behavior:

* `None`: Specifies no special options.
* `Asynchronous`: Enables asynchronous read and write operations on the pipe, allowing for non-blocking I/O.
* `WriteThrough`: Ensures that write operations are flushed directly to the pipe buffer, bypassing any intermediate buffering.
* `FirstPipeInstance`: Indicates that the server stream is the first instance of the named pipe.

`NamedPipeClientStream` constructors:

```csharp
public NamedPipeClientStream(string pipeName)
    : this(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
```

```csharp
public NamedPipeClientStream(string serverName, string pipeName)
    : this(serverName, pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
```

```csharp
public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction)
    : this(serverName, pipeName, direction, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.None)
```

```csharp
public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options)
    : this(serverName, pipeName, direction, options, TokenImpersonationLevel.None, HandleInheritability.None)
```

```csharp
public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel)
    : this(serverName, pipeName, direction, options, impersonationLevel, HandleInheritability.None)
```

```csharp
public NamedPipeClientStream(string serverName, string pipeName, PipeDirection direction, PipeOptions options, TokenImpersonationLevel impersonationLevel, HandleInheritability inheritability)
    : base(direction, 0)
```

```csharp
// Create a NamedPipeClientStream from an existing server pipe handle.
public NamedPipeClientStream(PipeDirection direction, bool isAsync, bool isConnected, SafePipeHandle safePipeHandle)
    : base(direction, 0)
```

`serverName`: Specifies the name of the server to which the client wants to connect.

* It can be the name of a remote server on the network or a local server name.
* To connect to a named pipe on the same machine, you can use ".", "localhost", or the machine's NetBIOS name.

`PipeDirection`: Specifies the direction of data flow for the pipe, exactly as with `NamedPipeServerStream`.
`TokenImpersonationLevel`: Specifies the level of impersonation the server is allowed to use when acting on behalf of the client.

* This is a crucial security setting for named pipes.
* Impersonation allows the server to assume the client's security context, which is necessary for accessing resources on the server on behalf of the client.
* The `TokenImpersonationLevel` enum defines the available levels (e.g., `None`, `Anonymous`, `Identification`, `Impersonation`, `Delegation`), each granting different levels of privilege to the server.

`HandleInheritability`: This parameter is relevant in scenarios where the client process might create child processes.

* It specifies whether the client's handle to the named pipe can be inherited by those child processes.
* If set to `HandleInheritability.Inheritable`, child processes can also use the pipe for communication.

`SafePipeHandle`: Represents a wrapper around the native operating system handle to the pipe.

* It provides a type-safe way to manage pipe handles, ensuring that they are properly closed and preventing resource leaks.
* Using `SafePipeHandle` is generally recommended over working directly with raw handles.

## Named Pipes - Interprocess communication (IPC)

The code below provides a practical example of interprocess communication with Named Pipes, showcasing how a server and client application can exchange data.

<details>
  <summary>Server Application</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/NamedPipesInterprocess/ServerApp/Program.cs

</details>

<details>
  <summary>Client Application</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/NamedPipesInterprocess/ClientApp/Program.cs

</details>

### Explanation

1. Server Application:
   * **Initialization**: The server application defines a constant `PipeName` to identify the named pipe and checks the operating system to determine the correct path to the client application's executable.
   * **Pipe Creation**: It creates a `NamedPipeServerStream` using the defined `PipeName` and specifies `PipeDirection.InOut` for bidirectional communication.
   * **Client Process Start**: It starts the client process using `Process.Start()`.  Note that, unlike the Anonymous Pipes example, the Named Pipe server doesn't pass a handle to the client process. Named Pipes are located by name.
   * **Connection and Communication**: The server waits for a client to connect using `pipeServerStream.WaitForConnectionAsync()`. It sends a "SYNC" message to the client to synchronize communication. The server then enters a loop to read input from the console (`Console.ReadLine()`) and send it to the client via the pipe. This loop continues until the user enters "EXIT". After the loop, the server reads a final message from the client.
   * **Cleanup**: The server waits for the client process to exit and then closes the pipe.

2. Client Application:
   * **Initialization**: The client application defines the same `PipeName` as the server and the `ServerName` to connect to (in this case, "." for the local machine).
   * **Pipe Connection**: It creates a `NamedPipeClientStream` using the `ServerName` and `PipeName`, and attempts to connect to the server using `pipeClientStream.ConnectAsync()`.  A timeout is specified for the connection attempt.
   * **Communication**: It creates a `StringPipe` to handle string-based communication. The client waits for the "SYNC" message from the server. It then enters a loop to read messages from the server and display them on the console. This loop continues until the server sends "EXIT". Finally, the client sends a "Bye-bye." message to the server.
   * **Cleanup**: The client closes the pipe.

**Key differences from the Anonymous Pipes example**:

* Named Pipes use a name for connection instead of handle passing.
* Named Pipes support bidirectional communication by default.
* The server and client explicitly connect to each other.

### Console output

<details>
  <summary>Windows</summary>

```text
15.48.102070 | [SERVER] Started.
15.48.128004 | [SERVER] Running on Windows.
15.48.190682 | [CLIENT] Started.
15.48.221664 | [CLIENT] Connected to server.
15.48.226765 | [CLIENT] Current TransmissionMode: Byte
15.48.227133 | [CLIENT] Wait for sync...
15.48.232859 | [SERVER] Client connected.
15.48.234812 | [CLIENT] Synced with server.
15.48.234911 | [CLIENT] Reading data.
15.48.236236 | [SERVER] Current TransmissionMode: Byte
15.48.236356 | [SERVER] Enter text ('exit' to quit):
User message 1
15.56.649212 | [CLIENT] Received: User message 1
15.56.661262 | [CLIENT] Reading data.
15.56.661651 | [SERVER] Enter text ('exit' to quit):
User message 2
16.02.026339 | [CLIENT] Received: User message 2
16.02.026461 | [CLIENT] Reading data.
16.02.026542 | [SERVER] Enter text ('exit' to quit):
exit
16.04.798981 | [SERVER] Reading data.
16.04.799015 | [CLIENT] Received: exit
16.04.799244 | [CLIENT] Write data to Server.
16.04.803951 | [CLIENT] Writing data to Server complete.
16.04.804052 | [CLIENT] Quit.
16.04.805728 | [SERVER] Message from Client: Bye-bye.
16.04.826100 | [SERVER] Pipe stream exit.
16.04.826767 | [SERVER] Quit.
```

</details>

<details>
  <summary>Linux</summary>

```text
21.00.455524 | [SERVER] Started.
21.00.511331 | [SERVER] Running on Linux.
21.00.615988 | [CLIENT] Started.
21.00.751091 | [CLIENT] Connected to server.
21.00.757069 | [CLIENT] Current TransmissionMode: Byte
21.00.759179 | [CLIENT] Wait for sync...
21.00.761362 | [SERVER] Client connected.
21.00.773210 | [SERVER] Current TransmissionMode: Byte
21.00.773262 | [SERVER] Enter text ('exit' to quit):
21.00.779608 | [CLIENT] Synced with server.
21.00.779648 | [CLIENT] Reading data.
User message 1
21.14.048379 | [CLIENT] Received: User message 1
21.14.061543 | [SERVER] Enter text ('exit' to quit):
21.14.066439 | [CLIENT] Reading data.
User message 2
21.21.106639 | [SERVER] Enter text ('exit' to quit):
21.21.107444 | [CLIENT] Received: User message 2
21.21.107555 | [CLIENT] Reading data.
exit
21.22.801497 | [SERVER] Reading data.
21.22.802082 | [CLIENT] Received: exit
21.22.803311 | [CLIENT] Write data to Server.
21.22.804555 | [CLIENT] Writing data to Server complete.
21.22.804587 | [CLIENT] Quit.
21.22.810020 | [SERVER] Message from Client: Bye-bye.
21.22.822148 | [SERVER] Pipe stream exit.
21.22.827009 | [SERVER] Quit.
```

</details>

## Named Pipes - Multi Threading Communication

The example below shows how to use Named Pipes for Multi Threading Communication.

<details>
  <summary>Code Example</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/NamedPipesMultiThreading/Program.cs

</details>

### Explanation - Named Pipes for Multi Threading Communication

1. Server Thread (ServerThread method):
   * **Pipe Creation**: It creates a `NamedPipeServerStream` with the specified `PipeName` and `PipeDirection.InOut` for bidirectional communication.
   * **Connection Wait**: The server thread then calls `pipeServerStream.WaitForConnection()` to block and wait until a client connects to the named pipe.
   * **Connection Information**: Once a client connects, it displays a message confirming the connection and logs the pipe's `TransmissionMode`.
   * **Data Transfer**: It creates a `StringPipe object` to handle sending and receiving strings over the pipe. The server thread then writes a series of messages to the client using `stringPipe.WriteString()`. It reads a message from the client using `stringPipe.ReadString()` and displays the received text.
   * **Connection Status and Cleanup**: The server thread checks and displays the `IsConnected` status of the pipe. It indicates that the pipe stream is exiting.

2. Client Thread (ClientThread method):
   * **Pipe Creation and Connection**: It creates a `NamedPipeClientStream` to connect to the server. It uses "." to specify the local machine as the server, the defined `PipeName`, `PipeDirection.InOut` for bidirectional communication, and `PipeOptions.Asynchronous`. The `PipeOptions.Asynchronous` option suggests that the client might be performing asynchronous operations on the pipe.
It attempts to connect to the server using `pipeClientStream.Connect(timeout)` with a timeout of 30 seconds. It displays a message confirming the connection and logs the pipe's `TransmissionMode`.
   * **Data Transfer**: It creates a `StringPipe` object for string-based duplex communication. The client thread enters a loop to read messages from the server using `stringPipe.ReadString()` and displays the received text.  The loop continues until it receives the "Exit" message (case-insensitive). After receiving "Exit", the client sends a "Hello from Client." message to the server using `stringPipe.WriteString()`.
   * **Communication End**: The client thread indicates that it has finished writing data. It indicates that the pipe stream is exiting.

### Console output - Named Pipes for Multi Threading Communication

<details>
  <summary>Windows</summary>

```text
43.06.824613 | 1 | [MAIN] Application started.
43.06.888222 | 12 | [CLIENT] Thread started.
43.06.888498 | 11 | [SERVER] Thread started.
43.06.896171 | 11 | [SERVER] Client connected.
43.06.896162 | 12 | [CLIENT] Connected to server.
43.06.904225 | 12 | [CLIENT] Current TransmissionMode: Byte
43.06.904225 | 11 | [SERVER] Current TransmissionMode: Byte
43.06.904797 | 12 | [CLIENT] Reading data.
43.06.904797 | 11 | [SERVER] Write data to Client.
43.06.924001 | 12 | [CLIENT] Received: I am the one true server!
43.06.948071 | 12 | [CLIENT] Reading data.
43.06.948703 | 12 | [CLIENT] Received: Hello from Server.
43.06.948816 | 12 | [CLIENT] Reading data.
43.06.949275 | 12 | [CLIENT] Received: Message 23491284
43.06.949369 | 12 | [CLIENT] Reading data.
43.06.949733 | 11 | [SERVER] Writing data to Client complete.
43.06.949834 | 12 | [CLIENT] Received: Exit
43.06.949998 | 12 | [CLIENT] Write data to Server.
43.06.950410 | 11 | [SERVER] Received: Hello from Client..
43.06.950505 | 12 | [CLIENT] Writing data to Server complete.
43.06.950637 | 11 | [SERVER] IsConnected: True.
43.06.950646 | 12 | [CLIENT] Pipe stream exit.
43.06.950719 | 11 | [SERVER] Pipe stream exit.
43.06.951051 | 11 | [SERVER] Thread exit.
43.06.951089 | 12 | [CLIENT] Thread exit.
43.06.961112 | 1 | [MAIN] Application finished.
```

</details>

<details>
  <summary>Linux</summary>

```text
24.09.940854 | 1 | [MAIN] Application started.
24.10.032569 | 8 | [SERVER] Thread started.
24.10.033275 | 9 | [CLIENT] Thread started.
24.10.066387 | 9 | [CLIENT] Connected to server.
24.10.066631 | 8 | [SERVER] Client connected.
24.10.078173 | 9 | [CLIENT] Current TransmissionMode: Byte
24.10.078534 | 8 | [SERVER] Current TransmissionMode: Byte
24.10.078705 | 9 | [CLIENT] Reading data.
24.10.078828 | 8 | [SERVER] Write data to Client.
24.10.079393 | 8 | [SERVER] Writing data to Client complete.
24.10.079606 | 9 | [CLIENT] Received: I am the one true server!
24.10.092345 | 9 | [CLIENT] Reading data.
24.10.093040 | 9 | [CLIENT] Received: Hello from Server.
24.10.093094 | 9 | [CLIENT] Reading data.
24.10.093110 | 9 | [CLIENT] Received: Message 23491284
24.10.093119 | 9 | [CLIENT] Reading data.
24.10.093131 | 9 | [CLIENT] Received: Exit
24.10.093140 | 9 | [CLIENT] Write data to Server.
24.10.093161 | 9 | [CLIENT] Writing data to Server complete.
24.10.093170 | 9 | [CLIENT] Pipe stream exit.
24.10.093279 | 8 | [SERVER] Received: Hello from Client..
24.10.093328 | 8 | [SERVER] IsConnected: True.
24.10.093339 | 8 | [SERVER] Pipe stream exit.
24.10.093341 | 9 | [CLIENT] Thread exit.
24.10.093973 | 8 | [SERVER] Thread exit.
24.10.094237 | 1 | [MAIN] Application finished.
```

</details>

## Named Pipes - Internetwork communication

Using Named Pipes for internetwork communication introduces specific considerations.

* First, it primarily functions on Windows operating systems.
* Underlyingly, pipe communication relies on the Server Message Block (SMB) protocol, necessitating that port 445 be open in the firewall.
* Furthermore, establishing a pipe connection typically requires using the same username and password on both the client and server machines.
* Notably, setting up pipe communication with different user credentials is only achievable within the .NET Framework.
* For the server name, you can utilize either the computer's name or its IP address.

The following code example illustrates a scenario involving the same user account on both machines.

<details>
  <summary>Server Application (same user - .Net 8)</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/NamedPipesNetwork/ServerApp/Program.cs

</details>

<details>
  <summary>Client Application</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/NamedPipesNetwork/ClientApp/Program.cs

</details>

<details>
  <summary>Server Application (any user - .Net Framework)</summary>

https://github.com/LA777/Articles/blob/da69b207dfd3d1fbd69ce95aef0aed1e7d686887/dotnet-pipes/src/NamedPipesNetwork/ServerAppAnyUser/Program.cs

</details>

### Explanation

1. Server Application - same user (.Net 8):
   * **Pipe Creation**: It creates a `NamedPipeServerStream` using the defined `PipeName` and `PipeDirection.InOut` to enable bidirectional communication.
   * **Client Connection**: The server waits for a client to connect using `pipeServerStream.WaitForConnectionAsync()`, which blocks until a connection is established.
   * **Communication**: A `StringPipe` object (a custom class for handling string-based communication) is created. The server sends a "SYNC" message to the client to indicate it's ready for communication. It logs "Client connected." and the pipe's `TransmissionMode`. The server enters a loop that reads input from the console and sends it to the client via `stringPipe.WriteStringAsync()`. This continues until the user enters "EXIT" (case-insensitive). After the loop, the server reads a message from the client using `stringPipe.ReadStringAsync()` and logs the received message.

2. Client Application:
   * **Pipe Connection**: It creates a `NamedPipeClientStream` using the `ServerName` and `PipeName`, specifying `PipeDirection.InOut` for bidirectional communication. It attempts to connect to the server using `pipeClientStream.ConnectAsync()`. It logs "Connected to server." and the pipe's `TransmissionMode`.
   * **Communication**: A `StringPipe` object is created for duplex string-based communication. The client enters a loop to wait for and read the "SYNC" message from the server. After receiving "SYNC," it enters another loop to read messages from the server using `stringPipe.ReadStringAsync()` and logs the received text. This loop continues until the server sends "EXIT" (case-insensitive). The client then sends a "Bye-bye." message to the server.

3. Server Application - any user (.Net Framework):
   * **Namespaces**:
     * `System.Security.AccessControl`: Enables manipulation of access control lists (ACLs) for securable objects.
     * `System.Security.Principal`: Provides classes for representing user and group identities.
   * **Pipe Security Setup**: A `PipeSecurity` object is created to manage access control for the named pipe.
     * **Allow Current User**: It retrieves the current user's identity (`Environment.UserDomainName + "\\" + Environment.UserName`).
     * A `PipeAccessRule` is created to grant the current user `PipeAccessRights.FullControl` and added to the `pipeSecurity`.
   * **Allow Administrators**: A `SecurityIdentifier` is created for the built-in Administrators group (`WellKnownSidType.BuiltinAdministratorsSid`). A `PipeAccessRule` grants `FullControl` to the Administrators group.
   * **Allow Everyone (Less Secure)**: A `SecurityIdentifier` is created for the "Everyone" group (`WellKnownSidType.WorldSid`). A `PipeAccessRule` grants `FullControl` to everyone. **Note**: The code comments explicitly mention this is "less secure" but used for simplicity in the example. In a production environment, granting full control to everyone is generally discouraged.

### Named Pipe Server Creation and Communication

A `NamedPipeServerStream` is created within a using statement (for automatic resource disposal).
The constructor takes several parameters:

* `PipeName`: The name of the pipe.
* `PipeDirection.InOut`: For bidirectional communication.
* `numThreads`: The maximum number of server instances.
* `PipeTransmissionMode.Byte`: Data transmission mode.
* `PipeOptions.Asynchronous`: Enables asynchronous operations.
* 1024, 1024: Input and output buffer sizes.
* `pipeSecurity`: The `PipeSecurity` object we configured earlier, which sets the access permissions.
* The server waits for a client to connect using `pipeServerStream.WaitForConnectionAsync()`.
* It logs a message indicating the client has connected, including the thread ID.
* A `StringPipe` object is created (again, presumably a custom class for string handling).
* The server sends a "SYNC" message to the client.
* It enters a loop to read input from the console and send it to the client, similar to the previous examples.  The loop continues until the user enters "EXIT" (case-insensitive).
* It reads a message from the client.

### Key Differences and Improvements

* **Security**: This code explicitly sets security permissions on the named pipe, which is the most important distinction. This allows connections from users other than the one running the server.
* **.NET Framework**: This code is specifically designed for the .NET Framework, where setting pipe security for arbitrary users is supported. .NET Core and .NET (versions 5+) have differences in how security contexts are handled with named pipes, particularly for cross-machine scenarios.
* `PipeSecurity`: The `PipeSecurity` class is used to define access rules. You can grant or deny various `PipeAccessRights` (e.g., `Read`, `Write`, `CreateNewInstance`, `FullControl`) to specific users or groups.
* **Security Identifiers**: The `SecurityIdentifier` class is used to represent Windows security principals (users, groups, etc.). `WellKnownSidType` provides constants for common SIDs (e.g., `Administrators`, `Everyone`).

### Important Security Note

While this code demonstrates how to allow connections from "Everyone" for simplicity, it's crucial to understand the security implications. In real-world applications, you should carefully define the minimum necessary permissions and grant them only to the specific users or groups that need to access the pipe. Granting full control to "Everyone" can expose your application to security risks.

**Key Points**:

* This example demonstrates basic client-server communication using Named Pipes.
* Both applications use a custom `StringPipe` class to simplify string sending and receiving over the pipe.
* The client connects to the server by name (`ServerName`).
* The "SYNC" message is used for initial synchronization between the client and server.
* The communication is bidirectional (`PipeDirection.InOut`).
* Error handling is included to catch potential `IOExceptions`.

### Console output

<details>
  <summary>Server application</summary>

```text
46.56.978875 | [SERVER] Started.
46.56.999660 | [SERVER] Waiting Client to connect...
47.01.135768 | [SERVER] Client connected.
47.01.139156 | [SERVER] Current TransmissionMode: Byte
47.01.139299 | [SERVER] Enter text ('exit' to quit):
User message 1
47.13.527281 | [SERVER] Enter text ('exit' to quit):
User message 2
47.19.764928 | [SERVER] Enter text ('exit' to quit):
exit
47.25.145989 | [SERVER] Reading data.
47.25.153849 | [SERVER] Message from Client: Bye-bye.
47.25.153986 | [SERVER] Pipe stream exit.
47.25.154556 | [SERVER] Quit.
```

</details>

<details>
  <summary>Client application</summary>

```text
47.00.585797 | [CLIENT] Started.
47.00.626249 | [CLIENT] Connected to server.
47.00.632320 | [CLIENT] Current TransmissionMode: Byte
47.00.633004 | [CLIENT] Wait for sync...
47.00.652175 | [CLIENT] Synced with server.
47.00.652309 | [CLIENT] Reading data.
47.13.031024 | [CLIENT] Received: User message 1
47.13.043087 | [CLIENT] Reading data.
47.19.281566 | [CLIENT] Received: User message 2
47.19.281751 | [CLIENT] Reading data.
47.24.662994 | [CLIENT] Received: exit
47.24.663193 | [CLIENT] Write data to Server.
47.24.671069 | [CLIENT] Writing data to Server complete.
47.24.672542 | [CLIENT] Quit.
```

</details>

### Named pipes – Conclusion

Named Pipes offer a powerful and flexible mechanism for interprocess communication in .NET. They provide a way for processes to communicate, whether they are on the same machine or, with careful configuration, across a network.

We have explored several facets of Named Pipes:

* **Basic IPC**: Demonstrating the fundamental client-server interaction with synchronization.

* **Multithreading**: Showing how Named Pipes can be used in multithreaded applications to enable concurrent communication.

* **Internetwork Communication**: Addressing the complexities of using Named Pipes across a network, particularly the importance of security considerations.

Key factors to keep in mind when working with Named Pipes include:

* **Platform Dependence**: While Named Pipes are available on both Windows and Linux, their behavior and security implementations can differ.

* **Security**: Especially in networked scenarios, security is paramount. Proper configuration of `PipeSecurity` is essential to restrict access and protect data.

* **Resource Management**: Like any I/O operation, proper resource management (e.g., using `using` statements to ensure disposal of `NamedPipeServerStream` and `NamedPipeClientStream`) is crucial to prevent leaks.

By understanding these concepts and considerations, developers can effectively leverage Named Pipes to build robust and efficient interprocess communication solutions in .NET.
