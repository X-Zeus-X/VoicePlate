using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using VoicePlate.Models;

namespace VoicePlate;

public partial class HistoryPage : ContentPage
{
    private List<JournalEntry> allEntries = new();

    public HistoryPage()
    {
        InitializeComponent();
        LoadEntries();
        LoadDateFilters();
    }

    private async void LoadEntries()
    {
        string filePath = Path.Combine(FileSystem.AppDataDirectory, "journal_entries.json");

        if (File.Exists(filePath))
        {
            string json = await File.ReadAllTextAsync(filePath);
            allEntries = JsonSerializer.Deserialize<List<JournalEntry>>(json) ?? new();
            EntriesView.ItemsSource = allEntries.OrderByDescending(e => e.Timestamp);
        }
    }

    private void LoadDateFilters()
    {
        DateFilterPicker.Items.Add("All");
        DateFilterPicker.Items.Add("Today");
        DateFilterPicker.Items.Add("This Week");
        DateFilterPicker.Items.Add("This Month");
        DateFilterPicker.Items.Add("This Year");
        DateFilterPicker.SelectedIndex = 0;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnDateFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        string query = SearchBar.Text?.ToLower() ?? "";
        string dateFilter = DateFilterPicker.SelectedItem?.ToString() ?? "All";

        var filtered = allEntries.Where(entry =>
            (string.IsNullOrEmpty(query) || entry.Title.ToLower().Contains(query)) &&
            DateMatches(entry.Timestamp, dateFilter)
        );

        EntriesView.ItemsSource = filtered.OrderByDescending(e => e.Timestamp).ToList();
    }

    private bool DateMatches(DateTime date, string filter)
    {
        var now = DateTime.Now;

        return filter switch
        {
            "Today" => date.Date == now.Date,
            "This Week" => date >= now.AddDays(-7),
            "This Month" => date.Month == now.Month && date.Year == now.Year,
            "This Year" => date.Year == now.Year,
            _ => true,
        };
    }

    private void OnEntrySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is JournalEntry selectedEntry)
        {
            Navigation.PushAsync(new EntryDetailPage(selectedEntry));
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}
