using EFT_item_checker.Manager;
using EFT_item_checker.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace EFT_item_checker
{
    internal class Document : IDisposable
    {
        private static readonly Document _instance = new Document();
        public static Document Instance => _instance;

        public Dictionary<SettingType, string> Settings { get; set; } = new Dictionary<SettingType, string>();

        public List<string> Selections { get; set; } = new List<string>();

        public Dictionary<string, int> collects = new Dictionary<string, int>();

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private string _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private string _selectionsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selections.json");
        private string _itemCollectFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "collects.json");

        private Document()
        {

        }

        public void Init()
        {
            LoadSettings();
            LoadSelections();

            LoadItemCollects();
        }

        // 아이템 수집 수량 가져오기
        private void LoadItemCollects()
        {
            try
            {
                if(File.Exists(_itemCollectFilepath))
                {
                    string json = File.ReadAllText(_itemCollectFilepath);
                    var loadedCollects = JsonSerializer.Deserialize<Dictionary<string, int>>(json, _jsonOptions);

                    if (loadedCollects != null)
                    {
                        collects = loadedCollects;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"아이템 수집 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveItemCollects()
        {
            try
            {
                string json = JsonSerializer.Serialize(collects, _jsonOptions);
                File.WriteAllText(_itemCollectFilepath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"수집 정보를 저장하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 유저의 설정 정보 가져오기 및 초기값 설정
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<Dictionary<SettingType, string>>(json, _jsonOptions);

                    // 설정이 null이거나 설정 목록의 개수가 맞지 않으면 기본값 설정
                    if (settings == null || settings.Count != Enum.GetValues<SettingType>().Length)
                    {
                        SetDefaultSettings();
                    }
                    else
                    {
                        Settings = settings;
                    }
                }
                else
                {
                    // 설정 파일이 없으면 기본값을 생성하거나 초기화
                    SetDefaultSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                SetDefaultSettings();
            }
        }

        // 기본 설정값을 정의 및 저장
        private void SetDefaultSettings()
        {
            Settings[SettingType.Language] = LanguageType.en.ToString();
            Settings[SettingType.TaskSort] = SortType.Completed.ToString();

            SaveSettings();
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(Settings, _jsonOptions);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 정보를 저장하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //유저의 진행 정보를 불러오기
        public void LoadSelections()
        {
            try
            {
                if (File.Exists(_selectionsFilePath))
                {
                    string json = File.ReadAllText(_selectionsFilePath);
                    var selections = JsonSerializer.Deserialize<List<string>>(json, _jsonOptions);

                    if (selections == null)
                    {
                        return;
                    }
                    else
                    {
                        Selections = selections;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장된 정보를 불러오는데 오류가 발생했습니다 : {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveSelections()
        {
            //유저의 진행정보를 저장하기
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selections.json");
                string json = JsonSerializer.Serialize(Selections, _jsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정된 정보를 저장하는데 오류가 발생했습니다 : {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            SaveSettings();
            SaveSelections();
            SaveItemCollects();
        }

        internal void Reset()
        {
            SetDefaultSettings();
        }

        public TraderType GetTraderById(string? id)
        {
            return id switch
            {
                "54cb50c76803fa8b248b4571" => TraderType.Prapor,
                "54cb57776803fa99248b456e" => TraderType.Therapist,
                "579dc571d53a0658a154fbec" => TraderType.Fence,
                "58330581ace78e27b8b10cee" => TraderType.Skier,

                "5935c25fb3acc3127c3d8cd9" => TraderType.Peacekeeper,
                "5a7c2eca46aef81a7ca2145d" => TraderType.Mechanic,
                "5ac3b934156ae10c4430e83c" => TraderType.Ragman,
                "5c0647fdd443bc2504c2d371" => TraderType.Jaeger,

                "638f541a29ffd1183d187f57" => TraderType.LightKeeper,
                "656f0f98d80a697f855d34b1" => TraderType.Btr,
                "6617beeaa9cfa777ca915b7c" => TraderType.Ref,

                _ => TraderType.Unknown
            };
        }
    }
}
