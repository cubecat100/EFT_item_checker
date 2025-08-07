using EFT_item_checker.Model;
using EFT_item_checker.Service;
using EFT_item_checker.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace EFT_item_checker.Manager
{
    internal class NetworkManager
    {
        // Singleton 인스턴스
        private static readonly NetworkManager _instance = new NetworkManager();
        public static NetworkManager Instance => _instance;

        private string uri = "https://api.tarkov.dev/graphql";

        private string lang = "en"; // 기본 언어 설정

        private Dictionary<string, string> itemQuery;

        private Dictionary<string, string> taskQuery;

        private Dictionary<string, string> stationQuery;

        private NetworkManager()
        {
            
        }

        public void init()
        {
            lang = Document.Instance.Settings[SettingType.Language];

            itemQuery = new Dictionary<string, string>()
            {
                {"query", "{ items (lang : " + lang + ") { id name shortName iconLink wikiLink category { parent { name }}}}" }
            };

            taskQuery = new Dictionary<string, string>()
            {
                {"query", "{ tasks (lang : " + lang + ") { id name kappaRequired wikiLink trader { id } taskRequirements { task { id } } objectives { ... on TaskObjectiveItem { item { id name shortName } count foundInRaid }}}}" }
            };

            stationQuery = new Dictionary<string, string>()
            {
                {"query", "{ hideoutStations (lang : " + lang + ") { id name levels { id level crafts { rewardItems { item{ id name }}} itemRequirements { item { id name } count quantity } stationLevelRequirements { station { id name } level } } } }" }
            };
        }

        public async Task<string> ConnectApiAsync(TaskType type)
        {
            using (var httpClient = new HttpClient())
            {
                // Determine the query to use based on the TaskType
                Dictionary<string, string> query = type switch
                {
                    TaskType.Item => itemQuery,
                    TaskType.Quest => taskQuery,
                    TaskType.Station => stationQuery,
                    _ => new Dictionary<string, string>()
                };

                if (query == null || query.Count == 0)
                {
                    //MessageBox.Show("유효하지 않은 TaskType입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }

                var httpResponse = await httpClient.PostAsJsonAsync(uri, query);

                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    return responseContent;
                }
                else
                {
                    MessageBox.Show($"API 호출 실패: {httpResponse.StatusCode}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
            }
        }
    }

}
