using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace OwaspTool.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly HttpClient _http;
        private readonly ILogger<AiController> _logger;

        public AiController(IHttpClientFactory httpClientFactory, ILogger<AiController> logger)
        {
            _http = httpClientFactory.CreateClient("Ollama");
            _logger = logger;
        }

        public record RequirementHelpRequest(string RequirementNumber, string RequirementText, string Question);

        [HttpPost("requirement-help")]
        public async Task<ActionResult<string>> RequirementHelp([FromBody] RequirementHelpRequest request)
        {
            try
            {
                var prompt = $"""
                You are an OWASP ASVS assistant.
                Requirement {request.RequirementNumber}: {request.RequirementText}

                User question: {request.Question}

                Answer concisely in English, with practical implementation guidance.
                """;

                var payload = new
                {
                    model = "llama3.2", // cambia se il modello ha un altro nome
                    prompt = prompt,
                    stream = false
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Ollama with payload: {Payload}", json);

                var response = await _http.PostAsync("/api/generate", content);

                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Ollama status: {Status}, body: {Body}", response.StatusCode, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode,
                        $"Ollama error {response.StatusCode}: {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                var answer = root.TryGetProperty("response", out var respProp)
                    ? respProp.GetString()
                    : responseBody; // fallback: restituisco tutto

                return Ok(answer ?? "No answer generated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RequirementHelp");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
