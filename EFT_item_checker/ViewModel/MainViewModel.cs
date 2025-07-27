using EFT_item_checker.Model;
using EFT_item_checker.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace EFT_item_checker.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        #region PROPERTY

        public ObservableCollection<Model.Task> TaskList { get; set; } = new ObservableCollection<Model.Task>();

        public ObservableCollection<Model.Task> VisibleTaskList         {
            get
            {
                // TaskList에서 IsVisibility가 Visible인 항목만 필터링하여 반환
                return new ObservableCollection<Model.Task>(TaskList.Where(task => task.IsVisibility == Visibility.Visible));
            }
        }

        public ObservableCollection<RequiredItem> FilteredItems { get; set; } = new ObservableCollection<RequiredItem>();

        public Model.Task SelectedItem { get; set; }

        public ICommand ToggleSelectCommand { get; }

        private Model.Task _task;
        public Model.Task SelectedTask { get => _task; set => SelectedTaskData(value); }

        public string Description { get; set; } = "EFT 아이템 체크";

        public bool CheckBoxEnabled { get; set; } = false;

        object loc = new object();

        private bool _isQuestsVisible = true;
        private bool _isStationsVisible = true;
        private bool _isKappaQuestsVisible = true;
        private bool _isSelectableQuestsVisible = true;

        public bool IsStationsVisible
        {
            get => _isStationsVisible;
            set
            {
                if (_isStationsVisible != value)
                {
                    _isStationsVisible = value;

                    UpdateVisible();

                    OnPropertyChanged(nameof(IsStationsVisible));
                    UpdateItemLists();
                }
            }
        }

        public bool IsQuestsVisible
        {
            get => _isQuestsVisible;
            set
            {
                if (_isQuestsVisible != value)
                {
                    _isQuestsVisible = value;

                    _isKappaQuestsVisible = value;
                    _isSelectableQuestsVisible = value;
                    
                    UpdateQuestVisibleProperty(value);

                    UpdateItemLists();
                }
            }
        }

        public bool IsKappaQuestsVisible
        {
            get => _isKappaQuestsVisible;
            set
            {
                if (_isKappaQuestsVisible != value)
                {
                    _isKappaQuestsVisible = value;

                    if(_isKappaQuestsVisible == false)
                    {
                        _isQuestsVisible = false;
                    }

                    UpdateQuestVisibleProperty(value);

                    UpdateItemLists();
                }
            }
        }

        public bool IsSelectableQuestsVisible
        {
            get => _isSelectableQuestsVisible;
            set
            {
                if (_isSelectableQuestsVisible != value)
                {
                    _isSelectableQuestsVisible = value;

                    if(_isSelectableQuestsVisible == false)
                    {
                        _isQuestsVisible = false;
                    }

                    UpdateQuestVisibleProperty(value);

                    UpdateItemLists();
                }
            }
        }

        private void UpdateQuestVisibleProperty(bool value)
        {
            UpdateVisible();

            OnPropertyChanged(nameof(IsQuestsVisible));
            OnPropertyChanged(nameof(IsSelectableQuestsVisible));
            OnPropertyChanged(nameof(IsKappaQuestsVisible));
        }

        private void UpdateVisible()
        {
            // TaskType에 따라 IsVisible 속성을 업데이트합니다.
            foreach (var task in TaskList)
            {
                if (task.Type == TaskType.Station)
                {
                    if (_isStationsVisible == true)
                    {
                        task.IsVisibility = Visibility.Visible;
                    }
                    else
                    {
                        task.IsVisibility = Visibility.Collapsed;
                    }
                }
                else if(task.Type == TaskType.Quest)
                {
                    if (_isQuestsVisible == true)
                    {
                        task.IsVisibility = Visibility.Visible;
                    }
                    else if (_isKappaQuestsVisible == true && task.IsKappa == true)
                    {
                        task.IsVisibility = Visibility.Visible;
                    }
                    else if (_isSelectableQuestsVisible == true && task.IsKappa == false)
                    {
                        task.IsVisibility = Visibility.Visible;
                    }
                    else
                    {
                        task.IsVisibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion

        private void SelectedTaskData(Model.Task task)
        {
            // 선택된 Task의 정보를 화면에 업데이트합니다.
            Description = task?.WikiLink ?? string.Empty;

            OnPropertyChanged(nameof(SelectedTask));
        }

        public MainViewModel()
        {
            // Task의 체크박스 선택 상태가 변경될 때
            ToggleSelectCommand = new RelayCommand<Model.Task>(OnToggleSelect);

            // TaskList의 항목이 추가/제거될 때마다 이벤트 구독/해제
            TaskList.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Model.Task task in e.NewItems)
                    {
                        task.PropertyChanged += OnTaskPropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (Model.Task task in e.OldItems)
                    {
                        task.PropertyChanged -= OnTaskPropertyChanged;
                    }
                }
            };

            LoadData();
        }

        private void OnTaskPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.Task.IsSelected))
            {
                UpdateItemLists();
            }
        }

        private void LoadData()
        {
            // TaskManager에서 데이터 로드되면 화면에 업데이트하도록 이벤트를 구독
            // 화면 업데이트를 위한 로직이며 Mediator 패턴을 이용하여 구현할 수도 있다.

            TaskManager.Instance.DataLoaded += DataLoaded;
        }

        private void DataLoaded(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadDataCheck();
            });
        }

        private void LoadDataCheck()
        {
            // 데이터가 모두 로드되면 TaskList와 ValidItems를 초기화합니다.

            ValidItems = TaskManager.Instance.AllItems
                .Where(item => !string.IsNullOrWhiteSpace(item.IconPath))
                .ToDictionary(item => item.Id, item => item);

            TaskList.Clear();

            foreach (var task in TaskManager.Instance.AllQuests.Where(task => IsCurrentTask(task)))
            {
                TaskList.Add(task);
            }

            foreach (var station in TaskManager.Instance.AllStations)
            {
                TaskList.Add(station);
            }

            CheckBoxEnabled = true;
            OnPropertyChanged(nameof(CheckBoxEnabled));

            UpdateItemLists();
        }

        public Dictionary<string, Item> ValidItems { get; private set; } = new Dictionary<string, Item>();

        /// <summary>
        /// TaskList에서 체크박스 옵션에 따라 Type, IsKappa를 필터링
        /// 선택되지 않은 Task의 RequiredItems를 아이템별로 수량을 합산하여 FilteredItems로 변환
        /// </summary>
        private void UpdateItemLists()
        {
            // TaskList 필터링
            var filteredTasks = TaskList.Where(task => IsCurrentTask(task));

            // 선택되지 않은 Task만 대상으로 아이템 집계
            var grouped = filteredTasks
                .Where(task => !task.IsSelected)
                .SelectMany(task => task.RequiredItems ?? Enumerable.Empty<RequiredItem>())
                .Where(ri => ri.Item != null
                             && !string.IsNullOrWhiteSpace(ri.Item.Id)
                             && ValidItems.ContainsKey(ri.Item.Id))
                .GroupBy(ri => ri.Item.Id)
                .Select(g =>
                {
                    var first = g.First();
                    var itemWithIcon = ValidItems[first.Item.Id];
                    return new RequiredItem
                    {
                        Item = itemWithIcon,
                        Quantity = g.Sum(x => x.Quantity),
                        FoundInRaid = g.Any(x => x.FoundInRaid)
                    };
                });

            FilteredItems.Clear();

            foreach (var item in grouped)
            {
                FilteredItems.Add(item);
            }

            OnPropertyChanged(nameof(VisibleTaskList));
        }


        // 체크박스 옵션에 따라 현재 Task의 필터링 여부를 결정합니다.
        private bool IsCurrentTask(Model.Task task)
        {
            if(task.Type == TaskType.Station && _isStationsVisible) return true;

            if (task.Type == TaskType.Quest)
            {
                if(_isQuestsVisible == true) return true;

                if (task.IsKappa == true && _isKappaQuestsVisible == true) return true;
                if (task.IsKappa == false && _isSelectableQuestsVisible == true) return true;
            }

            return false;
        }

        private void OnToggleSelect(Model.Task task)
        {
            if(task.IsSelected)
            {
                // Task가 선택되면 선행 Task들의 선택 상태를 업데이트
                SetPredecessorsSelected(task, new HashSet<string>());
            }
            else
            {
                // Task가 선택 해제되면 후행 Task들의 선택 상태를 업데이트
                SetSuccessorsSelected(task, new HashSet<string>());
            }
        }


        // Task의 선택 상태가 변경되면 선행/후행 Task들의 선택 상태를 업데이트합니다.
        private void OnTaskSelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.Task.IsSelected))
            {
                if (sender is Model.Task changedTask)
                {
                    if (changedTask.IsSelected)
                    {
                        // 선행 Task들(IsSelected = true)
                        SetPredecessorsSelected(changedTask, new HashSet<string>());
                    }
                    else
                    {
                        // 후행 Task들(IsSelected = false)
                        SetSuccessorsSelected(changedTask, new HashSet<string>());
                    }
                }
                UpdateItemLists();
            }
        }

        // 선행 Task들의 IsSelected를 true로 설정
        private void SetPredecessorsSelected(Model.Task task, HashSet<string> visited)
        {
            if (task.RequiredTasks == null) return;

            foreach (var reqId in task.RequiredTasks)
            {
                if (!visited.Add(reqId)) continue;

                var preTask = TaskList.FirstOrDefault(t => t.Id == reqId);
                if (preTask != null && preTask.IsSelected != true)
                {
                    preTask.IsSelected = true;
                    SetPredecessorsSelected(preTask, visited);
                }
            }
        }

        // 후행 Task들의 IsSelected를 false로 설정
        private void SetSuccessorsSelected(Model.Task task, HashSet<string> visited)
        {
            foreach (var succTask in TaskList.Where(t => t.RequiredTasks != null && t.RequiredTasks.Contains(task.Id)))
            {
                if (!visited.Add(succTask.Id)) continue;

                if (succTask.IsSelected != false)
                {
                    succTask.IsSelected = false;
                    SetSuccessorsSelected(succTask, visited);
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            var selectionList = TaskList.Where(Task => Task.IsSelected).Select(task => task.Id).ToList();

            TaskManager.Instance.SaveSelections(selectionList);
        }
    }
}
