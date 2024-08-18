using System.Globalization;
using System.Net.Http;
using app.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _configuration = configuration;
    }


    public async Task<IActionResult> Index()
    {
        var apiUrl = _configuration["ApiSettings:ApiUrl"];
        var apiKey = _configuration["ApiSettings:ApiKey"];
        var requestUrl = $"{apiUrl}?code={apiKey}";

        var response = await _httpClient.GetAsync(requestUrl);
        if (response.IsSuccessStatusCode)
        {
            var jsonData = await response.Content.ReadAsStringAsync();
            var timeEntries = JsonConvert.DeserializeObject<List<TimeEntry>>(jsonData);
            if (timeEntries == null)
            {
                return View(new List<Employee>());
            }

            var employeeMap = new Dictionary<string, double>();

            foreach (var entry in timeEntries)
            {
                if (string.IsNullOrEmpty(entry.EmployeeName) || string.IsNullOrEmpty(entry.StarTimeUtc) || string.IsNullOrEmpty(entry.EndTimeUtc))
                {
                    continue;
                }

                var hours = CalculateWorkHours(entry.StarTimeUtc, entry.EndTimeUtc);

                if (employeeMap.ContainsKey(entry.EmployeeName))
                {
                    employeeMap[entry.EmployeeName] += hours;
                }
                else
                {
                    employeeMap[entry.EmployeeName] = hours;
                }
            }

            var employees = employeeMap.Select(e => new Employee
            {
                EmployeeName = e.Key,
                TotalHours = e.Value
            }).OrderByDescending(e => e.TotalHours).ToList();

            return View(employees);
        }
        else
        {
            return View(new List<Employee>());
        }
    }

    private double CalculateWorkHours(string startTime, string endTime)
    {
        //end-start
        return (DateTime.Parse(endTime, null, DateTimeStyles.RoundtripKind) - DateTime.Parse(startTime, null, DateTimeStyles.RoundtripKind)).TotalHours;
    }
}

public class TimeEntry
{
    public string? EmployeeName { get; set; }
    public string? StarTimeUtc { get; set; }
    public string? EndTimeUtc { get; set; }
}
