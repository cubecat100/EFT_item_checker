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

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private string _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private string _selectionsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "selections.json");

        private Document()
        {

        }

        public void Init()
        {
            LoadSettings();
            LoadSelections();
        }

        private void LoadSettings()
        {
            //유저의 설정정보를 불러오기
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<Dictionary<SettingType, string>>(json, _jsonOptions);

                    if (settings == null)
                    {
                        DefaultSettings();
                    }
                    else
                    {
                        Settings = settings;
                    }
                }
                else
                {
                    // 설정 파일이 없으면 기본값을 생성하거나 초기화
                    DefaultSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"설정 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                DefaultSettings();
            }
        }

        private void DefaultSettings()
        {
            // 기본 설정값을 정의
            Settings[SettingType.Language] = LanguageType.en.ToString();

            // 설정 저장
            SaveSettings();
        }

        private void SaveSettings()
        {
            // 설정 저장 로직이 필요하다면 여기에 구현
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

        public void LoadSelections()
        {
            //유저의 진행정보를 불러오기
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
        }
    }
}
