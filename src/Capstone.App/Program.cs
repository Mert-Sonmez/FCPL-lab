using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Capstone.Core;

string filePath = "curriculum.json";
string url = "http://localhost:5088/";

var store = await CourseStore.LoadAsync(filePath);
if (store.Count == 0)
{
    store.Add(new MandatoryCourse
    {
        Id = "CS101",
        DisplayName = "Programming Fundamentals",
        Credits = 6,
        AssessmentType = Assessment.Exam,
        Slot = new TimeSlot(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(10, 0), "A101"),
        Semester = new Semester(1, 1),
        Room = "A101"
    });
    store.Add(new MandatoryCourse
    {
        Id = "MATH101",
        DisplayName = "Calculus",
        Credits = 5,
        AssessmentType = Assessment.Exam,
        Slot = new TimeSlot(DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(12, 0), "A102"),
        Semester = new Semester(1, 1),
        Room = "A102"
    });
    store.Add(new MandatoryCourse
    {
        Id = "CS201",
        DisplayName = "Data Structures",
        Credits = 6,
        AssessmentType = Assessment.Exam,
        Slot = new TimeSlot(DayOfWeek.Wednesday, new TimeOnly(10, 0), new TimeOnly(12, 0), "B204"),
        Semester = new Semester(2, 1),
        Prerequisites = new List<string> { "CS101" },
        Room = "B204"
    });
    store.Add(new ElectiveCourse
    {
        Id = "CS305",
        DisplayName = "AI Fundamentals",
        Credits = 4,
        AssessmentType = Assessment.Project,
        Slot = new TimeSlot(DayOfWeek.Wednesday, new TimeOnly(11, 0), new TimeOnly(13, 0), null),
        Semester = new Semester(3, 1),
        Prerequisites = new List<string> { "CS201" },
        Capacity = 40
    });
    store.Add(new ElectiveCourse
    {
        Id = "CS310",
        DisplayName = "Web Engineering",
        Credits = 4,
        AssessmentType = Assessment.Colloquium,
        Slot = new TimeSlot(DayOfWeek.Friday, new TimeOnly(14, 0), new TimeOnly(16, 0), "C301"),
        Semester = new Semester(3, 1),
        Prerequisites = new List<string> { "CS101" },
        Capacity = 35
    });

    await store.SaveAsync(filePath);
}

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

using var listener = new HttpListener();
listener.Prefixes.Add(url);
listener.Start();

Console.WriteLine($"University Course Planner is running at {url}");
Console.WriteLine("Press Ctrl+C to stop.");

// Auto-open in browser
try
{
    if (OperatingSystem.IsMacOS()) Process.Start("open", url);
    else if (OperatingSystem.IsWindows()) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    else Process.Start("xdg-open", url);
}
catch { /* Ignore if browser cannot auto-open */ }

while (true)
{
    var ctx = await listener.GetContextAsync();
    var req = ctx.Request;
    var res = ctx.Response;
    string path = req.Url?.AbsolutePath ?? "/";

    try
    {
        if (path == "/")
        {
            await SendText(res, GetHtmlPage(), "text/html; charset=utf-8");
        }
        else if (path == "/api/state")
        {
            string passedParam = req.QueryString["passed"] ?? "CS101";
            var passed = passedParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var all = store.All().Cast<Course>().ToList();
            var clashes = new List<string>();
            for (int i = 0; i < all.Count; i++)
                for (int j = i + 1; j < all.Count; j++)
                    if (all[i].Slot.Overlaps(all[j].Slot))
                        clashes.Add($"{all[i].Id} ↔ {all[j].Id} ({all[i].Slot.Day})");

            var payload = new
            {
                count = store.Count,
                totalCredits = store.GetTotalCredits(),
                y1t1Credits = store.GetCreditsForSemester(1, 1),
                clashes,
                available = store.GetAvailableCourses(passed).Select(c => $"{c.Id} - {c.DisplayName}").ToList(),
                courses = all.Select(c => new
                {
                    kind = c is MandatoryCourse ? "Mandatory" : "Elective",
                    c.Id,
                    c.DisplayName,
                    c.Credits,
                    assessment = c.AssessmentType.ToString(),
                    semester = $"Y{c.Semester.Year} T{c.Semester.Term}",
                    slot = $"{c.Slot.Day} {c.Slot.Start:HH:mm}-{c.Slot.End:HH:mm}",
                    room = c.Slot.Room ?? "TBA",
                    extra = c is MandatoryCourse m ? $"Room {m.Room}" : $"Cap {((ElectiveCourse)c).Capacity}",
                    prerequisites = c.Prerequisites
                })
            };

            await SendText(res, JsonSerializer.Serialize(payload, jsonOptions), "application/json");
        }
        else if (path == "/api/save" && req.HttpMethod == "POST")
        {
            await store.SaveAsync(filePath);
            await SendText(res, JsonSerializer.Serialize(new { message = $"Saved {store.Count} courses to {filePath} (atomic .tmp -> File.Move)." }, jsonOptions), "application/json");
        }
        else if (path == "/api/load" && req.HttpMethod == "POST")
        {
            store = await CourseStore.LoadAsync(filePath);
            await SendText(res, JsonSerializer.Serialize(new { message = $"Loaded {store.Count} courses from {filePath} with all kinds, slots & prerequisites." }, jsonOptions), "application/json");
        }
        else if (path == "/api/add" && req.HttpMethod == "POST")
        {
            using var reader = new StreamReader(req.InputStream);
            var body = await reader.ReadToEndAsync();
            var doc = JsonDocument.Parse(body).RootElement;

            string kind = doc.GetProperty("kind").GetString() ?? "Mandatory";
            string id = (doc.GetProperty("id").GetString() ?? "").Trim();
            string name = (doc.GetProperty("name").GetString() ?? "").Trim();
            int credits = doc.GetProperty("credits").GetInt32();
            string prereqText = doc.GetProperty("prereqs").GetString() ?? "";
            var prereqs = prereqText.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            Course newCourse = kind == "Elective"
                ? new ElectiveCourse
                {
                    Id = id,
                    DisplayName = name,
                    Credits = credits,
                    AssessmentType = Assessment.Project,
                    Slot = new TimeSlot(DayOfWeek.Thursday, new TimeOnly(12, 0), new TimeOnly(14, 0), null),
                    Semester = new Semester(3, 2),
                    Prerequisites = prereqs,
                    Capacity = 30
                }
                : new MandatoryCourse
                {
                    Id = id,
                    DisplayName = name,
                    Credits = credits,
                    AssessmentType = Assessment.Exam,
                    Slot = new TimeSlot(DayOfWeek.Tuesday, new TimeOnly(10, 0), new TimeOnly(12, 0), "A202"),
                    Semester = new Semester(2, 2),
                    Prerequisites = prereqs,
                    Room = "A202"
                };

            store.Add(newCourse);
            await store.SaveAsync(filePath);
            await SendText(res, JsonSerializer.Serialize(new { message = $"Added {id} ({kind}) and saved to {filePath}." }, jsonOptions), "application/json");
        }
        else if (path == "/api/test-missing")
        {
            var empty = await CourseStore.LoadAsync("missing-folder/does-not-exist.json");
            await SendText(res, JsonSerializer.Serialize(new { message = $"Missing file check passed -> returned empty store (Count = {empty.Count})." }, jsonOptions), "application/json");
        }
        else if (path == "/api/test-corrupt")
        {
            string broken = "broken.json";
            await File.WriteAllTextAsync(broken, "{ invalid json");
            try
            {
                await CourseStore.LoadAsync(broken);
                await SendText(res, JsonSerializer.Serialize(new { message = "Unexpected: no exception thrown." }, jsonOptions), "application/json");
            }
            catch (InvalidDataException ex)
            {
                await SendText(res, JsonSerializer.Serialize(new { message = $"Corrupt file check passed -> threw InvalidDataException: {ex.Message}" }, jsonOptions), "application/json");
            }
            finally
            {
                if (File.Exists(broken)) File.Delete(broken);
            }
        }
        else
        {
            res.StatusCode = 404;
            res.Close();
        }
    }
    catch (Exception ex)
    {
        res.StatusCode = 400;
        await SendText(res, JsonSerializer.Serialize(new { message = $"Error: {ex.Message}" }, jsonOptions), "application/json");
    }
}

static async Task SendText(HttpListenerResponse res, string content, string contentType)
{
    byte[] buffer = Encoding.UTF8.GetBytes(content);
    res.ContentType = contentType;
    res.ContentLength64 = buffer.Length;
    await res.OutputStream.WriteAsync(buffer);
    res.Close();
}

static string GetHtmlPage() => """
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>University Course Planner</title>
<style>
  * { box-sizing: border-box; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; }
  body { margin: 0; background: #0f172a; color: #e2e8f0; padding: 24px; }
  .wrap { max-width: 1100px; margin: 0 auto; }
  h1 { margin: 0 0 4px; font-size: 26px; color: #fff; }
  .sub { color: #94a3b8; margin-bottom: 20px; font-size: 14px; }
  .stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; margin-bottom: 20px; }
  .card { background: #1e293b; border: 1px solid #334155; border-radius: 10px; padding: 14px; }
  .card .label { font-size: 12px; color: #94a3b8; text-transform: uppercase; }
  .card .val { font-size: 24px; font-weight: 700; margin-top: 4px; color: #38bdf8; }
  .toolbar { display: flex; flex-wrap: wrap; gap: 10px; margin-bottom: 14px; }
  button { background: #3b82f6; color: #fff; border: none; padding: 9px 14px; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 13px; }
  button:hover { background: #2563eb; }
  button.sec { background: #334155; }
  button.sec:hover { background: #475569; }
  button.warn { background: #b45309; }
  button.warn:hover { background: #92400e; }
  #status { background: #064e3b; border: 1px solid #10b981; color: #d1fae5; padding: 10px 14px; border-radius: 8px; margin-bottom: 18px; display: none; font-size: 14px; }
  table { width: 100%; border-collapse: collapse; background: #1e293b; border-radius: 10px; overflow: hidden; margin-bottom: 20px; }
  th, td { padding: 11px 12px; text-align: left; border-bottom: 1px solid #334155; font-size: 14px; }
  th { background: #0f172a; color: #94a3b8; font-size: 12px; text-transform: uppercase; }
  .badge { padding: 3px 8px; border-radius: 999px; font-size: 11px; font-weight: 700; }
  .Mandatory { background: #1d4ed8; color: #dbeafe; }
  .Elective { background: #7c3aed; color: #ede9fe; }
  .grid2 { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 20px; }
  input, select { background: #0f172a; color: #fff; border: 1px solid #475569; padding: 8px 10px; border-radius: 6px; font-size: 13px; }
  .form-row { display: flex; gap: 8px; flex-wrap: wrap; margin-top: 10px; }
</style>
</head>
<body>
<div class="wrap">
  <h1>University Course Planner</h1>
  <div class="sub">Lab 08 — Async JSON Persistence, Course Kinds, TimeSlots &amp; Prerequisites</div>

  <div class="stats">
    <div class="card"><div class="label">Total Courses</div><div class="val" id="sCount">-</div></div>
    <div class="card"><div class="label">Total Credits</div><div class="val" id="sCredits">-</div></div>
    <div class="card"><div class="label">Year 1 Term 1</div><div class="val" id="sY1">-</div></div>
    <div class="card"><div class="label">Timetable Clashes</div><div class="val" id="sClash">-</div></div>
  </div>

  <div class="toolbar">
    <button onclick="callApi('/api/save','POST')">SaveAsync (JSON + .tmp)</button>
    <button class="sec" onclick="callApi('/api/load','POST')">LoadAsync (Reload Disk)</button>
    <button class="warn" onclick="callApi('/api/test-missing','GET')">Test Missing File</button>
    <button class="warn" onclick="callApi('/api/test-corrupt','GET')">Test Corrupt File</button>
  </div>

  <div id="status"></div>

  <table>
    <thead>
      <tr><th>Kind</th><th>ID</th><th>Course</th><th>Credits</th><th>Sem</th><th>TimeSlot</th><th>Room</th><th>Prerequisites</th></tr>
    </thead>
    <tbody id="rows"></tbody>
  </table>

  <div class="grid2">
    <div class="card">
      <div class="label">Add Course (Live Save)</div>
      <div class="form-row">
        <select id="fKind"><option>Mandatory</option><option>Elective</option></select>
        <input id="fId" placeholder="ID (e.g. CS401)" style="width:110px">
        <input id="fName" placeholder="Course name" style="flex:1">
        <input id="fCredits" type="number" value="5" style="width:65px">
        <input id="fPrereqs" placeholder="Prereqs (e.g. CS101)" style="width:150px">
        <button onclick="addCourse()">Add &amp; Save</button>
      </div>
    </div>
    <div class="card">
      <div class="label">Can I Take It? (Passed Courses)</div>
      <div class="form-row">
        <input id="fPassed" value="CS101" style="flex:1" placeholder="Comma-separated IDs">
        <button class="sec" onclick="refresh()">Check</button>
      </div>
      <div id="availList" style="margin-top:10px; font-size:13px; color:#cbd5e1;"></div>
    </div>
  </div>
</div>

<script>
async function refresh() {
  const passed = encodeURIComponent(document.getElementById('fPassed').value);
  const res = await fetch('/api/state?passed=' + passed);
  const d = await res.json();
  document.getElementById('sCount').textContent = d.count;
  document.getElementById('sCredits').textContent = d.totalCredits + ' ECTS';
  document.getElementById('sY1').textContent = d.y1t1Credits + ' / 30';
  document.getElementById('sClash').textContent = d.clashes.length ? d.clashes.join(', ') : 'None';
  document.getElementById('availList').innerHTML = '<b>Open now:</b> ' + (d.available.length ? d.available.join(' · ') : 'None');
  document.getElementById('rows').innerHTML = d.courses.map(c => `
    <tr>
      <td><span class="badge ${c.kind}">${c.kind}</span></td>
      <td><b>${c.id}</b></td>
      <td>${c.displayName}</td>
      <td>${c.credits}</td>
      <td>${c.semester}</td>
      <td>${c.slot}</td>
      <td>${c.room}</td>
      <td>${c.prerequisites.length ? c.prerequisites.join(', ') : 'None'}</td>
    </tr>`).join('');
}
function showStatus(msg) {
  const el = document.getElementById('status');
  el.textContent = msg;
  el.style.display = 'block';
}
async function callApi(url, method) {
  const res = await fetch(url, { method });
  const d = await res.json();
  showStatus(d.message);
  await refresh();
}
async function addCourse() {
  const body = {
    kind: document.getElementById('fKind').value,
    id: document.getElementById('fId').value,
    name: document.getElementById('fName').value,
    credits: parseInt(document.getElementById('fCredits').value) || 4,
    prereqs: document.getElementById('fPrereqs').value
  };
  const res = await fetch('/api/add', { method: 'POST', body: JSON.stringify(body) });
  const d = await res.json();
  showStatus(d.message);
  document.getElementById('fId').value = '';
  document.getElementById('fName').value = '';
  await refresh();
}
refresh();
</script>
</body>
</html>
""";