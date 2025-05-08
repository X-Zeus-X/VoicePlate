using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Media;
using Android.Widget;
using Java.IO;

namespace VoicePlate;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
    public static MediaRecorder Recorder;
    public static string FilePath;

    public static void StartRecording()
    {
        try
        {
            FilePath = System.IO.Path.Combine(Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath, "voiceentry.3gp");

            Recorder = new MediaRecorder();
            Recorder.SetAudioSource(AudioSource.Mic);
            Recorder.SetOutputFormat(OutputFormat.ThreeGpp);
            Recorder.SetAudioEncoder(AudioEncoder.AmrNb);
            Recorder.SetOutputFile(FilePath);
            Recorder.Prepare();
            Recorder.Start();

            Toast.MakeText(Android.App.Application.Context, "Recording started", ToastLength.Short)?.Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(Android.App.Application.Context, "Error: " + ex.Message, ToastLength.Long)?.Show();
        }
    }

    public static void StopRecording()
    {
        try
        {
            Recorder.Stop();
            Recorder.Release();
            Recorder = null;

            Toast.MakeText(Android.App.Application.Context, "Recording saved", ToastLength.Short)?.Show();
        }
        catch (Exception ex)
        {
            Toast.MakeText(Android.App.Application.Context, "Error stopping recording", ToastLength.Long)?.Show();
        }
    }
}
