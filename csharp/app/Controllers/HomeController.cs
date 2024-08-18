using System.Globalization;
using System.Net.Http;
using app.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ScottPlot;
using System.Drawing;
using System.IO;
using Color = System.Drawing.Color;

public class TimeEntry
{
    public string? EmployeeName { get; set; }
    public string? StarTimeUtc { get; set; }
    public string? EndTimeUtc { get; set; }
}


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


            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "charts", "employeePieChart.png");
            GeneratePieChart(employees, filePath);
            ViewBag.PieChartUrl = "/charts/employeePieChart.png";

        
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


    private void GeneratePieChart(List<Employee> employees, string filePath)
    {

        ScottPlot.Plot myPlot = new();

        double[] values = employees.Select(e => e.TotalHours).ToArray();
        string[] labels = employees.Select(e => e.EmployeeName).ToArray();
        double[] percentages = values.Select(v => (v / employees.Sum(e => e.TotalHours)) * 100).ToArray();
        var colors = GenerateRandomScottPlotColors(employees.Count);

        List<PieSlice> slices = employees.Select((e, index) => new PieSlice
        {
            Value = e.TotalHours,
            Label = $"{labels[index]} ({Math.Round(percentages[index])}%)",
            FillColor = colors[index],
        }).ToList();

        var pie = myPlot.Add.Pie(slices);
        pie.ShowSliceLabels = true;
        myPlot.Layout.Frameless();
        pie.ExplodeFraction = 0.0;
        myPlot.HideGrid();

        myPlot.Legend.IsVisible = false;
        //myPlot.ShowLegend(Alignment.UpperLeft, Orientation.Horizontal);

        myPlot.SavePng(filePath, 800, 600);
    }

    private List<ScottPlot.Color> GenerateRandomScottPlotColors(int count)
    {
        var colors = new List<ScottPlot.Color>();
        for (int i = 0; i < count; i++)
        {
            int r = new Random().Next(256);
            int g = new Random().Next(256);
            int b = new Random().Next(256);
            var systemColor = System.Drawing.Color.FromArgb(128, r, g, b);
            colors.Add(new ScottPlot.Color(systemColor.R, systemColor.G, systemColor.B, systemColor.A));
        }

        return colors;
    }




}



