using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Red_and_Green_boxes.Models;
using System.Linq;

namespace Red_and_Green_boxes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<IActionResult> Index()
        {
            string repoOwner = "Flareium";   // Change this
            string repoName = "Red-and-Green-boxes\r\n";        // Change this
            string githubToken = Environment.GetEnvironmentVariable("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\r\n"); // Store securely
            string url = $"https://api.github.com/repos/{repoOwner}/{repoName}/actions/runs";

            List<WorkflowBox> workflows = new();

            try
            {
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {githubToken}");

                var response = await _httpClient.GetStringAsync(url);
                var jsonResponse = JsonDocument.Parse(response);

                // Dictionary to store the latest run per workflow
                Dictionary<string, WorkflowBox> latestWorkflows = new();

                foreach (var run in jsonResponse.RootElement.GetProperty("workflow_runs").EnumerateArray())
                {
                    string workflowName = run.GetProperty("name").GetString();
                    string status = run.GetProperty("conclusion").GetString(); // Workflow conclusion
                    DateTime createdAt = run.GetProperty("created_at").GetDateTime(); // Run timestamp

                    string color = status switch
                    {
                        "success" => "green",
                        "failure" => "red",
                        "in_progress" => "yellow",
                        _ => "gray"
                    };

                    if (!latestWorkflows.ContainsKey(workflowName) || latestWorkflows[workflowName].CreatedAt < createdAt)
                    {
                        latestWorkflows[workflowName] = new WorkflowBox
                        {
                            Name = workflowName,
                            Color = color,
                            CreatedAt = createdAt
                        };
                    }
                }

                workflows = latestWorkflows.Values.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching workflow data: {ex.Message}");
            }

            return View(workflows);
        }
    }

    public class WorkflowBox
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public DateTime CreatedAt { get; set; } // Track latest run timestamp
    }
}
