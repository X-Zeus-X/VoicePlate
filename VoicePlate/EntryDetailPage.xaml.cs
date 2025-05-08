using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using VoicePlate.Models;

namespace VoicePlate;

public partial class EntryDetailPage : ContentPage
{
    private readonly JournalEntry entry;
    private readonly IAudioPlayer player;

    public EntryDetailPage(JournalEntry selectedEntry)
    {
        InitializeComponent();
        entry = selectedEntry;
        BindingContext = entry;

        if (!string.IsNullOrEmpty(entry.PhotoPath) && File.Exists(entry.PhotoPath))
        {
            PhotoImage.Source = ImageSource.FromFile(entry.PhotoPath);
            PhotoFrame.IsVisible = true;
        }

        if (!string.IsNullOrEmpty(entry.AudioPath) && File.Exists(entry.AudioPath))
        {
            var stream = File.OpenRead(entry.AudioPath);
            player = AudioManager.Current.CreatePlayer(stream);
        }
    }

    private void OnPlayAudioClicked(object sender, EventArgs e)
    {
        try
        {
            if (player != null)
            {
                player.Play();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            DisplayAlert("Error", "Unable to play audio.", "OK");
        }
    }

    private async void OnLocationTapped(object sender, EventArgs e)
    {
        try
        {
            if (entry.Latitude.HasValue && entry.Longitude.HasValue)
            {
                var uri = $"https://www.google.com/maps/search/?api=1&query={entry.Latitude},{entry.Longitude}";
                await Launcher.Default.OpenAsync(uri);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not open location.", "OK");
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm", "Delete this journal entry?", "Yes", "No");
        if (!confirm) return;

        string filePath = Path.Combine(FileSystem.AppDataDirectory, "journal_entries.json");
        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            var entries = JsonSerializer.Deserialize<List<JournalEntry>>(json) ?? new();

            var updated = entries.Where(e => e.Id != entry.Id).ToList();
            string updatedJson = JsonSerializer.Serialize(updated, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, updatedJson);
        }

        await Navigation.PopAsync();
    }
}
