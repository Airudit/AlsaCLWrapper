namespace AlsaCommandLineWrapper;

public class AlsaOutEvent : IDisposable
{

    private readonly AutoResetEvent callbackEvent;

    public event EventHandler<AlsaCLOutEventArgs> DataAvailable;

    public event EventHandler<AlsaCLStoppedEventArgs> PlaybackStopped;

    private Task<Stream> buffer;

    private volatile AlsaPlaybackState playbackState;

    public AlsaPlaybackState PlaybackState => playbackState;

    public AlsaOutEvent()
    {
        callbackEvent = new AutoResetEvent(false);
        playbackState = AlsaPlaybackState.Stopped;
    }

    public async Task Play(Stream stream, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now.ToString("T")} device.Play");
        if (playbackState == AlsaPlaybackState.Stopped)
        {
            playbackState = AlsaPlaybackState.Playing;
            await AlsaCommandLineWrapperAudio.Play(stream, cancellationToken);
            callbackEvent.Set();
            //ThreadPool and callback is still a thing to understand
        }
        else
        {
            if (playbackState != AlsaPlaybackState.Paused)
            {
                return;
            }
            //            Resume();
            callbackEvent.Set();
        }
    }

    public void Stop()
    {
        if (playbackState == AlsaPlaybackState.Stopped)
        {
            return;
        }

        playbackState = AlsaPlaybackState.Stopped;
        PlaybackStopped?.Invoke(this, new AlsaCLStoppedEventArgs());
        callbackEvent.Set();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (playbackState != AlsaPlaybackState.Stopped)
        {
            Stop();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize((object)this);
    }
}

public class AlsaCLOutEventArgs : EventArgs
{
    private byte[] buffer;
    private int bytes;

    public AlsaCLOutEventArgs(byte[] buffer, int bytes)
    {
        buffer = buffer;
        bytes = bytes;
    }

    public byte[] Buffer => buffer;

    public int BytesRecorded => bytes;
}

public enum AlsaPlaybackState
{
    Stopped,
    Playing,
    Paused,
}
