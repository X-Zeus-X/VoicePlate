using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using VoicePlate.Models;

namespace VoicePlate;

public partial class MainPage : ContentPage
{
    private bool isRecording = false;
    private string audioPath;
    private FileResult capturedPhoto;
    private readonly IAudioPlayer player;

    public MainPage()
    {
        InitializeComponent();
        player = AudioManager.Current.CreatePlayer(Stream.Null);
    }

    private async void OnRecordClicked(object sender, EventArgs e)
    {
#if ANDROID
        try
        {
            if (!isRecording)
            {
                var folder = FileSystem.AppDataDirectory;
                audioPath = Path.Combine(folder, $"entry_{DateTime.Now.Ticks}.3gp");

                var context = Android.App.Application.Context;
                var intent = new Android.Content.Intent("START_RECORDING");
                intent.PutExtra("audioPath", audioPath);
                context.SendBroadcast(intent);

                isRecording = true;
                await DisplayAlert("Recording", "Recording started...", "OK");
            }
            else
            {
                var context = Android.App.Application.Context;
                var intent = new Android.Content.Intent("STOP_RECORDING");
                context.SendBroadcast(intent);

                isRecording = false;
                await DisplayAlert("Saved", "Recording stopped.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            await DisplayAlert("Error", "Recording failed.", "OK");
        }
#else
        await DisplayAlert("Unavailable", "Audio recording is not supported on this platform.", "OK");
#endif
    }

    private void OnPlayClicked(object sender, EventArgs e)
    {
#if ANDROID
        try
        {
            if (!string.IsNullOrEmpty(audioPath) && File.Exists(audioPath))
            {
                var stream = File.OpenRead(audioPath);
                player?.Stop();

                var newPlayer = AudioManager.Current.CreatePlayer(stream);
                newPlayer.Play();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            DisplayAlert("Error", "Playback failed.", "OK");
        }
#else
        DisplayAlert("Unavailable", "Audio playback is not supported on this platform.", "OK");
#endif
    }

    private async void OnCameraClicked(object sender, EventArgs e)
    {
#if ANDROID
        try
        {
            capturedPhoto = await MediaPicker.CapturePhotoAsync();
            if (capturedPhoto != null)
            {
                await DisplayAlert("Photo Captured", "Image saved successfully.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Camera Error", ex.Message, "OK");
        }
#else
        await DisplayAlert("Unavailable", "Camera is not supported on this platform.", "OK");
#endif
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var newEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            Title = EntrySummary.Text,
            Timestamp = DateTime.Now,
            AudioPath = audioPath,
            PhotoPath = capturedPhoto?.FullPath
        };

#if ANDROID
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();
            if (location != null)
            {
                newEntry.Latitude = location.Latitude;
                newEntry.Longitude = location.Longitude;

                var placemarks = await Geocoding.GetPlacemarksAsync(location);
                var placemark = placemarks?.FirstOrDefault();
                newEntry.LocationName = placemark != null
                    ? $"{placemark.Locality}, {placemark.CountryName}"
                    : "Unknown Location";
            }

            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100));
            await Flashlight.Default.TurnOnAsync();
            await Task.Delay(200);
            await Flashlight.Default.TurnOffAsync();
        }
        catch
        {
            // Skip errors silently
        }
#endif

        string filePath = Path.Combine(FileSystem.AppDataDirectory, "journal_entries.json");
        List<JournalEntry> entries = new();

        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            entries = JsonSerializer.Deserialize<List<JournalEntry>>(json) ?? new();
        }

        entries.Add(newEntry);
        string updatedJson = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, updatedJson);

        await DisplayAlert("Saved", "Journal entry saved successfully.", "OK");
        EntrySummary.Text = string.Empty;
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HistoryPage());
    }
}
