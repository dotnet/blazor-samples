using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.CodeAnalysis;

namespace BlazorSample;

internal sealed class LazyBrowserFileStream : Stream
{
    private readonly IBrowserFile file;
    private readonly int maxAllowedSize;
    private Stream? underlyingStream;
    private bool isDisposed;

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => file.Size;

    public override long Position
    {
        get => underlyingStream?.Position ?? 0;
        set => throw new NotSupportedException();
    }

    public LazyBrowserFileStream(IBrowserFile file, int maxAllowedSize)
    {
        this.file = file;
        this.maxAllowedSize = maxAllowedSize;
    }

    public override void Flush()
    {
        underlyingStream?.Flush();
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, 
        CancellationToken cancellationToken)
    {
        EnsureStreamIsOpen();

        return underlyingStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, 
        CancellationToken cancellationToken = default)
    {
        EnsureStreamIsOpen();
        return underlyingStream.ReadAsync(buffer, cancellationToken);
    }

    [MemberNotNull(nameof(underlyingStream))]
    private void EnsureStreamIsOpen()
    {
        underlyingStream ??= file.OpenReadStream(maxAllowedSize);
    }

    protected override void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        underlyingStream?.Dispose();
        isDisposed = true;

        base.Dispose(disposing);
    }

    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override void SetLength(long value)
        => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
}
