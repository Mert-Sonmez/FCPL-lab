using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SharpQuest.Contracts;

namespace Capstone.Core;
public static class CourseStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private sealed class CourseRow
    {
        public string Kind { get; set; } = "";
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int Credits { get; set; }
        public Assessment AssessmentType { get; set; }
        public TimeSlot? Slot { get; set; }
        public Semester? Semester { get; set; }
        public List<string> Prerequisites { get; set; } = new();
        public string? Room { get; set; }
        public int? Capacity { get; set; }
    }
    public static async Task SaveAsync(IItemStore store, string path)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var rows = store.All().Select(ToRow).ToList();

        string tempPath = path + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, rows, Options);
        }

        File.Move(tempPath, path, overwrite: true);
    }
    public static async Task<CourseStore> LoadAsync(string path)
    {   //It read back every course and all fields
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path))
        {
            return new CourseStore();
        }

    //missing file returns empty store, broken file sends InvalidDataException
        List<CourseRow>? rows;
        try
        {
            await using var stream = File.OpenRead(path);
            rows = await JsonSerializer.DeserializeAsync<List<CourseRow>>(stream, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The file contains invalid JSON.", ex);
        }

        if (rows is null)
        {
            throw new InvalidDataException("The file JSON has the wrong shape.");
        }

        var store = new CourseStore();
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Id) || string.IsNullOrWhiteSpace(row.Kind))
            {
                throw new InvalidDataException("A row is missing required fields.");
            }

            store.Add(FromRow(row));
        }

        return store;
    }
    private static CourseRow ToRow(IItem item)
    {
        if (item is MandatoryCourse m)
        {
            return new CourseRow
            {
                Kind = nameof(MandatoryCourse),
                Id = m.Id,
                DisplayName = m.DisplayName,
                Credits = m.Credits,
                AssessmentType = m.AssessmentType,
                Slot = m.Slot,
                Semester = m.Semester,
                Prerequisites = m.Prerequisites.ToList(),
                Room = m.Room
            };
        }

        if (item is ElectiveCourse e)
        {
            return new CourseRow
            {
                Kind = nameof(ElectiveCourse),
                Id = e.Id,
                DisplayName = e.DisplayName,
                Credits = e.Credits,
                AssessmentType = e.AssessmentType,
                Slot = e.Slot,
                Semester = e.Semester,
                Prerequisites = e.Prerequisites.ToList(),
                Capacity = e.Capacity
            };
        }

        throw new InvalidDataException($"Unknown course type: {item.GetType().Name}");
    }
    private static Course FromRow(CourseRow row)
    {
        var slot = row.Slot ?? new TimeSlot(DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(12, 0), null);
        var semester = row.Semester ?? new Semester(1, 1);

        if (row.Kind == nameof(MandatoryCourse))
        {
            return new MandatoryCourse
            {
                Id = row.Id,
                DisplayName = row.DisplayName,
                Credits = row.Credits,
                AssessmentType = row.AssessmentType,
                Slot = slot,
                Semester = semester,
                Prerequisites = row.Prerequisites?.ToList() ?? new List<string>(),
                Room = row.Room ?? "301CP"
            };
        }

        if (row.Kind == nameof(ElectiveCourse))
        {
            return new ElectiveCourse
            {
                Id = row.Id,
                DisplayName = row.DisplayName,
                Credits = row.Credits,
                AssessmentType = row.AssessmentType,
                Slot = slot,
                Semester = semester,
                Prerequisites = row.Prerequisites?.ToList() ?? new List<string>(),
                Capacity = row.Capacity ?? 30
            };
        }

        throw new InvalidDataException($"Unknown course kind in file: {row.Kind}");
    }
}