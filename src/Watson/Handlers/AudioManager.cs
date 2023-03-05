using NAudio.CoreAudioApi;

namespace Watson.Handlers;

public class AudioManager
{
    private readonly MMDeviceEnumerator _enumerator;
    public AudioManager()
    {
        _enumerator = new MMDeviceEnumerator();
    }

    public bool SetVolume(string volumeString)
    {
        if (!int.TryParse(volumeString, out int volume))
        {
            Console.WriteLine("Invalid volume - cannot be parsed to an integer");
            return false;
        }

        Console.WriteLine($"Adjusting volume to {volume}");

        // Get the default audio endpoint device
        var device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        Console.WriteLine($"Device name: {device.FriendlyName}");
        Console.WriteLine($"Device state: {device.State}");
        Console.WriteLine($"Device is muted: {device.AudioEndpointVolume.Mute}");

        // Set the volume to a new value (between 0 and 1)
        float newVolume = volume == 0 ? 0 : volume / 100;

        if (volume == 0)
        {
            device.AudioEndpointVolume.Mute = true;
        }
        else
        {
            device.AudioEndpointVolume.Mute = false;
            device.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume;
        }

        return true;
    }

    public void Mute()
    {
        SetVolume("0");
    }

}