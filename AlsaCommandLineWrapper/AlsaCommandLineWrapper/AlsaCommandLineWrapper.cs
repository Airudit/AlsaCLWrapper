namespace AlsaCommandLineWrapper;
using System.Text;
using System.Text.RegularExpressions;
using CliWrap;

public static class AlsaCommandLineWrapperAudio
{
    public static async Task Play(
        Stream audioStream,
        CancellationToken token = default
    )
    {
        try
        {
            var command =
                await Cli
                    .Wrap("aplay")
                    .WithArguments(new[]
                        {
                            "--device=sysdefault",
                            "--rate=16000",
                            "--channels=1",
                            "-t",
                            "raw",
                            "--format=s16_le",
                        })
                    .WithStandardInputPipe(PipeSource.FromStream(audioStream))
                    .ExecuteAsync(token);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public static async Task Record(Stream sb, CancellationToken token = default)
    {
        MemoryStream memoryStream = new MemoryStream();
        Console.WriteLine($"{DateTime.Now.ToString("T")} Function Record in AlsaWrapper.cs");
        try
        {
            await Cli
                .Wrap("arecord")
                .WithArguments(new[]
                {
                    "-",
                    "--rate=16000",
                    "--channels=1",
                    "-t",
                    "raw",
                    "--format=s16_le",
                })
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync(token);
        }
        catch (Exception)
        {
            // ignored
        }

        sb.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
    }

    public static async Task<List<AlsaInCapabilities>> GetOutCapabilities()
    {
        List<AlsaInCapabilities> alsaInCapabilitiesList = new List<AlsaInCapabilities>();
        var sb = new StringBuilder();
        var command =
            await Cli
                .Wrap("aplay")
                .WithArguments("--list-devices")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(sb))
                .ExecuteAsync();
        int i = 0;
        while ((i = sb.ToString().IndexOf("card", i, StringComparison.Ordinal)) != -1)
        {
            string line = sb.ToString().Substring(i);
            AlsaInCapabilities alsaInCapabilities = new AlsaInCapabilities()
            {
                DeviceName = AlsaInCapabilities.GetDeviceName(line),
                DeviceId = AlsaInCapabilities.GetDeviceId(line),
                DeviceCardName = AlsaInCapabilities.GetDeviceCardName(line),
                DeviceCardDescription = AlsaInCapabilities.GetDeviceCardDescription(line),
                DeviceCardId = AlsaInCapabilities.GetDeviceCardId(line),
            };
            i += "card".Length;
            alsaInCapabilitiesList.Add(alsaInCapabilities);
        }

        return alsaInCapabilitiesList;
    }

    private static async Task<List<AlsaInCapabilities>> GetInCapabilities()
    {
        List<AlsaInCapabilities> alsaInCapabilitiesList = new List<AlsaInCapabilities>();
        var sb = new StringBuilder();
        var command =
            await Cli
                .Wrap("arecord")
                .WithArguments("--list-devices")
                .WithStandardOutputPipe(PipeTarget.ToStringBuilder(sb))
                .ExecuteAsync();
        int i = 0;
        while ((i = sb.ToString().IndexOf("card", i, StringComparison.Ordinal)) != -1)
        {
            string line = sb.ToString().Substring(i);
            AlsaInCapabilities alsaInCapabilities = new AlsaInCapabilities()
            {
                DeviceName = AlsaInCapabilities.GetDeviceName(line),
                DeviceId = AlsaInCapabilities.GetDeviceId(line),
                DeviceCardName = AlsaInCapabilities.GetDeviceCardName(line),
                DeviceCardDescription = AlsaInCapabilities.GetDeviceCardDescription(line),
                DeviceCardId = AlsaInCapabilities.GetDeviceCardId(line),
            };
            i += "card".Length;
            alsaInCapabilitiesList.Add(alsaInCapabilities);
        }

        return alsaInCapabilitiesList;
    }

    private static List<string>[] GetProductsName(string listOfDevice)
    {
        List<string>[] result = new List<string>[2];
        result[0] = new List<string>();
        result[1] = new List<string>();
        int selector = 0;
        string pattern = @"\[[^\]]*\]";
        foreach (Match match in Regex.Matches(listOfDevice, pattern, RegexOptions.IgnoreCase))
        {
            if (selector++ % 2 == 1)
            {
                result[0].Add(match.Groups[0].Value);
            }
            else
            {
                result[1].Add(match.Groups[0].Value);
            }
        }
        return result;
    }

    public record PlaybackResult(string Output)
    {
    }
}

public struct AlsaInCapabilities
{
    public string DeviceName { get;  set; }

    public int DeviceId { get;  set; }

    public string DeviceCardName { get;  set; }

    public string DeviceCardDescription { get;  set; }

    public int DeviceCardId { get;  set; }

    public static string GetDeviceCardName(string line)
    {
        string deviceName = line.Substring(line.IndexOf(':') + 2).Split(' ')[0];
        return deviceName;
    }

    public static string GetDeviceCardDescription(string line)
    {
        string deviceName = line.Substring(line.IndexOf('[') + 1).Split(']')[0];
        return deviceName;
    }

    public static string GetDeviceName(string line)
    {
        string dontLookAtMe = line.Substring(line.IndexOf(':') + 1);
        string deviceName = dontLookAtMe.Substring(dontLookAtMe.IndexOf(':') + 2).Split('[')[0].Trim();
        return deviceName;
    }

    public static int GetDeviceId(string line)
    {
        string dontLookAtMe = line.Substring(line.IndexOf("device "));
        string getStringId = Regex.Match(dontLookAtMe, @"\d+").Value;
        return Int16.Parse(getStringId);
    }

    public static int GetDeviceCardId(string line)
    {
        string getStringId = Regex.Match(line, @"\d+").Value;
        return Int16.Parse(getStringId);
    }

}
