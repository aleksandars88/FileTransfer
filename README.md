# FileTransfer

A .NET 10 console application for transferring large files by splitting them into smaller chunks and transferring those chunks through a pluggable transport layer.

The solution is designed to keep the file transfer logic independent from the underlying transport mechanism. It currently supports an in-memory transport and RabbitMQ.

## Overview

The application consists of two main components:

* **Producer** – reads a source file, splits it into chunks, calculates an MD5 checksum for each chunk and a SHA-256 checksum for the complete file, and sends the chunks through the configured transport.
* **Consumer** – receives the chunks, validates their checksums, stores them temporarily, and reconstructs the original file once all chunks have been received.

The application does not load the entire file into memory. Files are processed incrementally using streams and chunks.

## File Chunking

The producer reads the source file using `FileStream` and splits it into smaller chunks.

Each chunk contains metadata required by the consumer:

```csharp
public class FileChunk
{
    public Guid FileId { get; set; }

    public string FileName { get; set; }

    public int ChunkIndex { get; set; }

    public int TotalChunks { get; set; }

    public long FileSize { get; set; }

    public byte[] Data { get; set; }

    public string ChunkChecksum { get; set; }

    public string FileChecksum { get; set; }
}
```

### Chunk metadata

* `FileId` uniquely identifies the file.
* `FileName` contains the original file name.
* `ChunkIndex` identifies the position of the chunk within the file.
* `TotalChunks` specifies how many chunks belong to the file.
* `FileSize` contains the original file size.
* `Data` contains the actual chunk data.
* `ChunkChecksum` contains the MD5 checksum calculated for the chunk.
* `FileChecksum` contains the SHA-256 checksum calculated for the complete file.

The consumer does not rely on chunks arriving in order. `FileId` and `ChunkIndex` are used to correctly identify and assemble the chunks.

## Checksum Validation

Checksum validation is performed on the consumer side before a received chunk is stored.

The producer calculates an MD5 checksum from the chunk data and when the consumer receives a chunk, it independently calculates the MD5 checksum from the received data and compares it with the `ChunkChecksum` provided in the message.

If the destination checksum does not match the source checksum, the chunk must be re-submitted.

The producer also calculates a SHA-256 checksum for the complete source file. This value is stored in `FileChecksum`.

After the destination file is reconstructed, its SHA-256 checksum is calculated and compared with the `FileChecksum`.

## Consumer Flow

The consumer follows this sequence:

1. Receive a chunk from the transport.
2. Validate the chunk metadata.
3. Calculate the MD5 checksum from the received data.
4. Compare the calculated checksum with the `ChunkChecksum`.
5. Re-submit the chunk if the checksum does not match.
6. Store the chunk if checksum validation succeeds.
7. Determine whether all chunks for the file have been received.
8. Reconstruct the file in the correct chunk order.
9. Calculate the SHA-256 checksum of the reconstructed file.
10. Compare the destination checksum with the `FileChecksum`.

## Transport Abstraction

The transfer logic is separated from the transport implementation through `IChunkTransport`.

```csharp
public interface IChunkTransport
{
    ValueTask SendChunk(
        FileChunk chunk,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<FileChunk> ReceiveChunk(
        CancellationToken cancellationToken = default);
}
```

This allows the producer and consumer to work with different transport implementations without changing the file-transfer logic.

### In-Memory Transport

The in-memory implementation uses `Channel<FileChunk>`.

It is useful for:

* unit tests
* integration tests
* local development
* verifying the producer/consumer workflow without external infrastructure

### RabbitMQ Transport

RabbitMQ provides an external message broker implementation of the same abstraction.

The application uses the RabbitMQ client to publish and consume `FileChunk` messages.

RabbitMQ can be started locally using Docker.

Example:

```bash
docker run -d \
  --hostname rabbitmq \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:management
```

RabbitMQ Management UI is available on:

```text
http://localhost:15672
```

## Storage

The consumer stores received chunks temporarily before the complete file can be assembled.

The storage abstraction keeps the consumer independent from the physical storage implementation.

The current implementation uses the local file system.

The destination directory is configured through application configuration.

## File Assembly

Once all chunks belonging to a file have been received, the consumer reconstructs the original file.

Chunks are ordered using `ChunkIndex`:

```text
Chunk 0
Chunk 1
Chunk 2
Chunk 3
...
Chunk N
```

They are then written sequentially to the destination file.

The consumer therefore does not depend on the order in which chunks are received from the transport.

## Configuration

Application settings are separated into file-transfer and transport configuration.

Example:

```json
{
  "FileTransfer": {
    "ChunkSize": 1048576,
    "SourceDirectory": "...",
    "DestinationDirectory": "..."
  },

  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "QueueName": "file-transfer"
  }
}
```

The exact values can be changed depending on the environment.

## Testing

The solution includes unit and integration tests.

### Unit Tests

Unit tests cover individual components and their behavior in isolation, for each service or component there is specific test suite.

Mocks are used where external dependencies need to be isolated.

### Integration Test

The integration test verifies the complete file-transfer workflow using the in-memory transport.

## Technologies

* .NET 10
* C#
* `System.IO`
* `System.Threading.Channels`
* RabbitMQ
* Docker
* xUnit
* Moq
* FluentAssertions

## Running the Application

### Prerequisites

* .NET 10 SDK
* Docker Desktop (required when using RabbitMQ)

### Clone the repository

```bash
git clone https://github.com/aleksandars88/FileTransfer.git

cd FileTransfer
```

### Build

```bash
dotnet build
```

### Run tests

```bash
dotnet test
```

### Run with RabbitMQ

Start RabbitMQ:

```bash
docker compose up -d
```

Then configure the producer and consumer to use the RabbitMQ transport.

## Example Transfer

Copy file into SourceDirectory configuration defined in the producer appsettings.json:

e.g.

```text
C:\FileTransfer\Source\large-file.jpg
```

The resulting file is written to the configured destination directory in the consumer appsettings.json.
