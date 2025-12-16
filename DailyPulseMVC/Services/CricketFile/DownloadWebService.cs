using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;

namespace DailyPulseMVC.Services.CricketFile
{
    public class DownloadWebService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy _retryPolicy;

        public DownloadWebService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Define a retry policy with exponential backoff
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        /// <summary>
        /// Downloads the content of a single URL as a string.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <returns>The content of the URL as a string.</returns>
        public async Task<string> DownloadContentAsync(string url)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] Attempting to download content from URL: {url}");
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });
        }

        /// <summary>
        /// Downloads the content of multiple URLs in parallel.
        /// </summary>
        /// <param name="urls">The list of URLs to download.</param>
        /// <returns>A list of strings containing the content of each URL.</returns>
        public async Task<List<string>> DownloadContentsAsync(List<string> urls)
        {
            var tasks = new List<Task<string>>();

            foreach (var url in urls)
            {
                tasks.Add(DownloadContentAsync(url));
            }

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            return new List<string>(results);
        }
    }
}