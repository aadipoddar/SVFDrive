using Plugin.Maui.Audio;
using SVFDrive.Shared.Services;

namespace SVFDrive.Services;

public class SoundService : ISoundService
{
    public async Task PlaySound(string soundFileName) =>
        AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(soundFileName)).Play();
}
