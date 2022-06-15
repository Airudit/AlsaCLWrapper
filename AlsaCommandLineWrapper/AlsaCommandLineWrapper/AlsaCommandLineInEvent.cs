namespace AlsaCommandLineWrapper;

public sealed class AlsaCommandLineInEvent : IDisposable
{
    private readonly AutoResetEvent callbackEvent;
    
    public event EventHandler<AlsaCLInEventArgs> DataAvailable;

    public event EventHandler<AlsaCLStoppedEventArgs> RecordingStopped;

    private volatile CaptureState captureState;

    public AlsaCommandLineInEvent()
    {
        this.callbackEvent = new AutoResetEvent(false);
        this.captureState = CaptureState.Stopped;
    }
    public int BufferMilliseconds { get; set; }

    public int NumberOfBuffers { get; set; }

    public int DeviceNumber { get; set; }

    public async Task StartRecording(Stream stream, CancellationToken cancellationToken)
    {
        if (this.captureState != CaptureState.Stopped)
        {
            throw new InvalidOperationException("Already recording");
        }

        await AlsaCommandLineWrapperAudio.Record(stream, cancellationToken);
        this.captureState = CaptureState.Starting;
    }

    public void StopRecording()
    {
        if (this.captureState == CaptureState.Stopped)
        {
            return;
        }

        this.captureState = CaptureState.Stopped;
        this.RecordingStopped?.Invoke(this, new AlsaCLStoppedEventArgs());
        this.callbackEvent.Set();
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (this.captureState != CaptureState.Stopped)
        {
            this.StopRecording();
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize((object) this);
    }
}

public enum CaptureState
{
    Stopped,
    Starting,
    Capturing,
    Stopping,
}

public class AlsaCLStoppedEventArgs : EventArgs
{
    private readonly Exception exception;

    public AlsaCLStoppedEventArgs(Exception exception = null) => this.exception = exception;

    public Exception Exception => this.exception;
}

//Might be useless
public class AlsaCLInEventArgs : EventArgs
{
    private byte[] buffer;
    private int bytes;

    public AlsaCLInEventArgs(byte[] buffer, int bytes)
    {
        this.buffer = buffer;
        this.bytes = bytes;
    }

}
