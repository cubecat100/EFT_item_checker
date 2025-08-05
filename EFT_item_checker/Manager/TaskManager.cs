using EFT_item_checker.Manager;
using EFT_item_checker.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Xml;
using Task = EFT_item_checker.Model.Task;

namespace EFT_item_checker.Service
{
    internal class TaskManager
    {
        // Singleton 인스턴스
        private static readonly TaskManager _instance = new TaskManager();
        public static TaskManager Instance => _instance;

        // 데이터 변경을 알리기 위한 이벤트.
        public event EventHandler DataLoaded;

        public List<Item> AllItems { get; private set; } = new List<Model.Item>();
        public List<Task> AllQuests { get; private set; } = new List<Model.Task>();
        public List<Task> AllStations { get; private set; } = new List<Task>();

        JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private bool _isLoadedQuests = false;
        private bool _isLoadedItems = false;
        private bool _isLoadedStations = false;

        private object loc = new object();

        private TaskManager()
        {
            
        }
        

        public void GetTarkovDevRequests()
        {
            LoadAllItems();
            LoadQuests();
            LoadStations();
        }

        private void LoadStations()
        {
            NetworkManager.Instance.ConnectApiAsync(TaskType.Station).ContinueWith(task =>
            {
                //api 반환 문자열 검증
                if (string.IsNullOrEmpty(task.Result))
                {
                    MessageBox.Show("API 연결에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                RequestStationData DataObj;

                //JSON 파싱
                try
                {
                    DataObj = JsonSerializer.Deserialize<RequestStationData>(task.Result, _jsonOptions) ?? new RequestStationData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파싱 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //매핑 정보 검증
                if (DataObj.Data == null || DataObj.Data.hideoutStations == null || DataObj.Data.hideoutStations.Count == 0)
                {
                    MessageBox.Show("은신처 목록이 비어있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AllStations.Clear();

                //은신처 스테이션
                foreach (var station in DataObj.Data.hideoutStations)
                {
                    //스테이션 레벨 마다 목록 생성
                    foreach (var level in station.Levels)
                    {
                        var stationData = new Model.Task
                        {
                            Id = level.Id,
                            Name = "🔨 " + station.Name + " " + level.Level,
                            Type = TaskType.Station,
                            RequiredItems = new List<RequiredItem>()
                        };

                        foreach (var itemReq in level.ItemRequirements)
                        {
                            var item = new Model.Item
                            {
                                Id = itemReq.Item.Id,
                                Name = itemReq.Item.Name,
                            };

                            stationData.RequiredItems.Add(new RequiredItem
                            {
                                Item = item,
                                Required = itemReq.Count,
                                FoundInRaid = true,
                            });
                        }

                        foreach (var req in level.StationLevelRequirements)
                        {
                            var reqStation = new Model.Task
                            {
                                Id = req.Station.Id + "-" + req.Level,
                                Name = req.Station.Name + " " + req.Level,
                                Type = TaskType.Station,
                            };

                            stationData.RequiredTasks.Add(reqStation.Id);
                        }

                        AllStations.Add(stationData);
                    }

                }

                _isLoadedStations = true;

                DataLoadCheck();

            });
        }

        public void LoadQuests()
        {
            AllQuests = new List<Model.Task>();

            NetworkManager.Instance.ConnectApiAsync(TaskType.Quest).ContinueWith(task =>
            {
                //api 반환 문자열 검증
                if (string.IsNullOrEmpty(task.Result))
                {
                    MessageBox.Show("API 연결에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                RequestTaskData DataObj;

                //JSON 파싱
                try
                {
                    DataObj = JsonSerializer.Deserialize<RequestTaskData>(task.Result, _jsonOptions) ?? new RequestTaskData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파싱 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //매핑 정보 검증
                if (DataObj.Data == null || DataObj.Data.Tasks == null || DataObj.Data.Tasks.Count == 0)
                {
                    MessageBox.Show("퀘스트 목록이 비어있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AllQuests.Clear();

                int cnt = 0;

                foreach (var t in DataObj.Data.Tasks)
                {
                    if (t.Objectives == null || t.Objectives.Count == 0)
                    {
                        cnt++;
                        continue;
                    }

                    var taskData = new Task
                    {
                        Id = t.Id,
                        Name = "📜 " + t.Name,
                        IsKappa = t.KappaRequired,
                        WikiLink = t.WikiLink,
                        Type = TaskType.Quest,
                        Trader = Document.Instance.GetTraderById(t.Trader?.Id),
                    };

                    if(t.Id == "657315ddab5a49b71f098853") //First in Line, 의료용품 3개
                    {
                        taskData.RequiredItems.Add(new RequiredItem
                        {
                            Item = new Item() 
                            { 
                                Id = "any_medicine_items",
                                Name = "any medicine items",
                                ShortName = "any_medicine",
                            },
                            Required = 3,
                            FoundInRaid = true
                        });
                    }
                    else
                    {
                        foreach (var ItemData in t.Objectives)
                        {
                            if (ItemData.Item == null) continue;

                            var item = new Item
                            {
                                Id = ItemData.Item.Id,
                                Name = ItemData.Item.Name,
                                ShortName = ItemData.Item.ShortName,
                                IconPath = ItemData.Item.IconLink
                            };

                            taskData.RequiredItems.Add(new RequiredItem
                            {
                                Item = item,
                                Required = ItemData.Count,
                                FoundInRaid = ItemData.FoundInRaid
                            });
                        }
                    }

                    // 선행 퀘스트가 있는 경우 추가
                    foreach (var req in t.TaskRequirements)
                    {
                        if (req.Task.Id == null) continue;
                        // 선행 퀘스트 ID를 추가
                        taskData.RequiredTasks.Add(req.Task.Id);
                    }

                    AllQuests.Add(taskData);
                }

                _isLoadedQuests = true;

                DataLoadCheck();
            });
        }

        public void LoadAllItems()
        {
            AllQuests = new List<Model.Task>();

            NetworkManager.Instance.ConnectApiAsync(TaskType.Item).ContinueWith(task =>
            {
                //api 반환 문자열 검증
                if (string.IsNullOrEmpty(task.Result))
                {
                    MessageBox.Show("API 연결에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                RequestItemData DataObj;

                //json 파싱
                try
                {
                    DataObj = JsonSerializer.Deserialize<RequestItemData>(task.Result, _jsonOptions) ?? new RequestItemData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파싱 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //매핑 정보 검증
                if (DataObj.Data == null || DataObj.Data.Items == null || DataObj.Data.Items.Count == 0)
                {
                    MessageBox.Show("아이템 목록이 비어있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AllItems.Clear();

                foreach (var item in DataObj.Data.Items)
                {
                    var itemData = new Model.Item
                    {
                        Id = item.Id,
                        Name = item.Name,
                        ShortName = item.ShortName,
                        IconPath = item.IconLink,
                        WikiLink = item.WikiLink
                    };

                    AllItems.Add(itemData);
                }

                var any_medicine = new Model.Item
                {
                    Id = "any_medicine_items",
                    Name = "any medicine items",
                    ShortName = "any_medicine",
                    IconPath = "",
                };

                AllItems.Add(any_medicine);

                _isLoadedItems = true;

                DataLoadCheck();
            });

        }

        private void DataLoadCheck()
        {
            // 모든 데이터가 로드되었는지 확인
            lock (loc)
            {
                if (!_isLoadedQuests || !_isLoadedItems || !_isLoadedStations)
                    return;
            }

            DataLoaded?.Invoke(this, new EventArgs());
        }

        
    }

}
