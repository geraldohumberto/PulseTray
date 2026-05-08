using System.Media;

namespace PulseTray.Notifications;

public sealed class AlertSoundPlayer
{
    public void PlayAlert()
    {
        SystemSounds.Exclamation.Play();
    }
}
