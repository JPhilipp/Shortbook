using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class OpenAiChatService
{
    const string openAiUrl = "https://api.openai.com/v1/chat/completions";
    const string model = "gpt-4";

    readonly string apiKey;
    readonly HttpClient client;

    // You can define a summary language like "German", though it mostly
    // also works without setting it, based on the original's language.
    public string summaryLanguage = null;

    public string translationLanguage = null;

    // Optionally, e.g. "Write in the style of Douglas Adams."
    public string additionalSummaryInstructions = null;

    public OpenAiChatService(string keyPath)
    {
        apiKey = File.ReadAllText(keyPath).Trim();
        client = new HttpClient();
        client.Timeout = TimeSpan.FromMinutes(3);

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<string> GetSummary(string bookChunk)
    {
        string role = "You are a helpful assistant. " +
            "Please shorten the provided book excerpt while keeping the narrative perspective and style. " +
            "Include speech of characters, also shortened.";
        if (!string.IsNullOrEmpty(summaryLanguage))
        {
            role += $" Write in {summaryLanguage}.";
        }
        if (!string.IsNullOrEmpty(additionalSummaryInstructions))
        {
            role += " " + additionalSummaryInstructions;
        }

        var prompt = new { model, messages = new object[] {
            new { role = "system", content = role },
            new { role = "user", content = bookChunk } },
            max_tokens = 4096
        };
        var content = new StringContent(JsonConvert.SerializeObject(prompt), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(openAiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
            Console.WriteLine(responseString);

            if (responseObject?["choices"]?[0]?["message"]?["content"] != null)
            {
                string summary = responseObject["choices"][0]["message"]["content"].ToString();
                return summary;
            }
            else
            {
                throw new Exception("Unexpected response structure from OpenAI API");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to communicate with OpenAI API. Status code: {response.StatusCode}. Response content: {errorContent}");
        }
    }

    public async Task<string> GetTranslation(string bookChunk)
    {
        translationLanguage = translationLanguage ?? "German";
        string role = "You are a helpful assistant. " +
            "Please translate the provided book excerpt to " + translationLanguage + ".";

        var prompt = new { model, messages = new object[] {
            new { role = "system", content = role },
            new { role = "user", content = bookChunk } },
            max_tokens = 4096
        };
        var content = new StringContent(JsonConvert.SerializeObject(prompt), Encoding.UTF8, "application/json");

        var response = await client.PostAsync(openAiUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
            Console.WriteLine(responseString);

            if (responseObject?["choices"]?[0]?["message"]?["content"] != null)
            {
                string summary = responseObject["choices"][0]["message"]["content"].ToString();
                return summary;
            }
            else
            {
                throw new Exception("Unexpected response structure from OpenAI API");
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to communicate with OpenAI API. Status code: {response.StatusCode}. Response content: {errorContent}");
        }
    }
}
