using EFT_item_checker.Model;
using EFT_item_checker.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EFT_item_checker.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        #region PROPERTY

        public Dictionary<string, Item> ValidItems { get; set; } = new();

        public ObservableCollection<Model.Task> TaskList { get; set; } = new();

        public ObservableCollection<Model.Task> VisibleTaskList
        {
            get
            {
                var tasks = TaskList
                    .Where(task => task.IsVisibility == Visibility.Visible)
                    .Where(task => string.IsNullOrEmpty(SearchText) || task.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                return sortTasks(tasks);
            }
        }

        private Dictionary<string, int> _allItemQuantities = new();

        private string _searchText = string.Empty;
        public string SearchText 
        {
            get => _searchText; 
            set
            {
                _searchText = value;

                OnPropertyChanged(nameof(VisibleTaskList));
            }
        }

        private string _searchItemText = string.Empty;

        public string SearchItemText
        {
            get => _searchItemText;
            set
            {
                _searchItemText = value;
                UpdateItemLists();
                OnPropertyChanged(nameof(FilteredItems));
            }
        }

        

        public List<ItemSortType> ItemSortList { get; } = Enum.GetValues<ItemSortType>().ToList();

        private ItemSortType _itemSortElement = ItemSortType.Name;
        public ItemSortType ItemSortElement
        {
            get => _itemSortElement;
            set
            {
                _itemSortElement = value;

                UpdateItemLists();
            }
        }

        public List<string> LanguageList => Enum.GetNames(typeof(LanguageType)).ToList();

        public string SelectedLanguage
        {
            get => Document.Instance.Settings[SettingType.Language];
            set
            {
                if (Document.Instance.Settings[SettingType.Language] != value)
                {
                    Document.Instance.Settings[SettingType.Language] = value;

                    OnPropertyChanged(nameof(SelectedLanguage));

                    MessageBox.Show("설정이 변경되었습니다. 프로그램을 다시 시작해주세요.");
                }
            }
        }

        private ObservableCollection<RequiredItem> _filteredItems = new ObservableCollection<RequiredItem>();
        public ObservableCollection<RequiredItem> FilteredItems { 
            get => _filteredItems;
            set
            {
                _filteredItems = value;

                OnPropertyChanged(nameof(FilteredItems));
            }
        }

        public ICommand ToggleSelectCommand { get; }
        public ICommand OpenLinkCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ItemClickCommand { get; }

        private string WikiUrl = "https://escapefromtarkov.fandom.com";
        private string HideoutWikiUrl = "https://escapefromtarkov.fandom.com/wiki/Hideout";

        public string Description { get; set; } = "escapefromtarkov wiki";
        private string _linkUrl;

        private Model.Task _task;
        public Model.Task SelectedTask { 
            get => _task; 
            set 
            {
                _task = value;
                OnPropertyChanged(nameof(SelectedTask));

                DescriptionUpdate();
            }
        }

        

        public Model.Task SelectedItem { get; set; } = new Model.Task();

        public bool ControlEnabled { get; set; } = false;

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

                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsStationsVisible));
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

                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsQuestsVisible));
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

                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsKappaQuestsVisible));
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

                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsSelectableQuestsVisible));
                }
            }
        }

       

        private bool _isPraporVisible = true;
        public bool IsPraporVisible
        {
            get => _isPraporVisible;
            set
            {
                if( _isPraporVisible != value)
                {
                    _isPraporVisible = value;

                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsPraporVisible));
                }
            }
        }

        private bool _isTherapistVisible = true;
        public bool IsTherapistVisible
        {
            get => _isTherapistVisible;
            set
            {
                if (_isTherapistVisible != value)
                {
                    _isTherapistVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsTherapistVisible));
                }
            }
        }

        private bool _isFenceVisible = true;
        public bool IsFenceVisible
        {
            get => _isFenceVisible;
            set
            {
                if (_isFenceVisible != value)
                {
                    _isFenceVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsFenceVisible));
                }
            }
        }

        private bool _isSkierVisible = true;
        public bool IsSkierVisible
        {
            get => _isSkierVisible;
            set
            {
                if (_isSkierVisible != value)
                {
                    _isSkierVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsSkierVisible));
                }
            }
        }

        private bool _isPeacekeeperVisible = true;
        public bool IsPeacekeeperVisible
        {
            get => _isPeacekeeperVisible;
            set
            {
                if (_isPeacekeeperVisible != value)
                {
                    _isPeacekeeperVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsPeacekeeperVisible));
                }
            }
        }

        private bool _isMechanicVisible = true;
        public bool IsMechanicVisible
        {
            get => _isMechanicVisible;
            set
            {
                if (_isMechanicVisible != value)
                {
                    _isMechanicVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsMechanicVisible));
                }
            }
        }

        private bool _isRagmanVisible = true;
        public bool IsRagmanVisible
        {
            get => _isRagmanVisible;
            set
            {
                if (_isRagmanVisible != value)
                {
                    _isRagmanVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsRagmanVisible));
                }
            }
        }

        private bool _isJaegerVisible = true;
        public bool IsJaegerVisible
        {
            get => _isJaegerVisible;
            set
            {
                if (_isJaegerVisible != value)
                {
                    _isJaegerVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsJaegerVisible));
                }
            }
        }

        private bool _isLightkeeperVisible = true;
        public bool IsLightkeeperVisible
        {
            get => _isLightkeeperVisible;
            set
            {
                if (_isLightkeeperVisible != value)
                {
                    _isLightkeeperVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsLightkeeperVisible));
                }
            }
        }

        private bool _isRefVisible = true;
        public bool IsRefVisible
        {
            get => _isRefVisible;
            set
            {
                if (_isRefVisible != value)
                {
                    _isRefVisible = value;
                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsRefVisible));
                }
            }
        }

        private bool _isBtrVisible = true;
        public bool IsBtrVisible
        {
            get => _isBtrVisible;
            set
            {
                if (_isBtrVisible != value)
                {
                    _isBtrVisible = value;

                    UpdateTaskVisibility();
                    OnPropertyChanged(nameof(IsBtrVisible));
                }
            }
        }

        public List<SortType> SortList => Enum.GetValues<SortType>().ToList();

        private string _sortType = Document.Instance.Settings[SettingType.TaskSort];
        public SortType SortElement
        {
            get => Enum.Parse<SortType>(_sortType);
            set
            {
                if (_sortType != value.ToString())
                {
                    _sortType = value.ToString();
                    
                    OnPropertyChanged(nameof(VisibleTaskList));
                }
            }
        }

        #endregion

        public MainViewModel()
        {
            _linkUrl = WikiUrl;

            // Task의 체크박스 선택 상태가 변경될 때
            ToggleSelectCommand = new RelayCommand<Model.Task>(OnToggleSelect);
            OpenLinkCommand = new RelayCommand(ExecuteOpenLink);
            ResetCommand = new RelayCommand(TaskSelectionReset);
            ItemClickCommand = new RelayCommand<RequiredItem>(ItemClicked);

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

        

        private void UpdateTaskVisibility()
        {
            foreach (var task in TaskList)
            {
                if (task.Type == TaskType.Station)
                {
                    task.IsVisibility = _isStationsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (task.Type == TaskType.Quest)
                {
                    // 트레이더별 표시 여부
                    bool traderVisible = task.Trader switch
                    {
                        TraderType.Prapor => _isPraporVisible,
                        TraderType.Therapist => _isTherapistVisible,
                        TraderType.Fence => _isFenceVisible,
                        TraderType.Skier => _isSkierVisible,
                        TraderType.Peacekeeper => _isPeacekeeperVisible,
                        TraderType.Mechanic => _isMechanicVisible,
                        TraderType.Ragman => _isRagmanVisible,
                        TraderType.Jaeger => _isJaegerVisible,
                        TraderType.LightKeeper => _isLightkeeperVisible,
                        TraderType.Ref => _isRefVisible,
                        TraderType.Btr => _isBtrVisible,
                        _ => true
                    };

                    if (traderVisible == false)
                    {
                        task.IsVisibility = Visibility.Collapsed;
                    }
                    else
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
        }

        private ObservableCollection<Model.Task> sortTasks(IEnumerable<Model.Task> tasks)
        {
            ObservableCollection<Model.Task> sortedTasks;

            switch (SortElement)
            {
                case SortType.Name:
                    sortedTasks = new ObservableCollection<Model.Task>(tasks.OrderBy(task => task.Name));
                    break;
                case SortType.Trader:
                    sortedTasks = new ObservableCollection<Model.Task>(tasks.OrderBy(task => task.Trader));
                    break;
                case SortType.Type:
                    sortedTasks = new ObservableCollection<Model.Task>(tasks.OrderBy(task => task.Type.ToString()));
                    break;
                case SortType.Completed:
                    sortedTasks = new ObservableCollection<Model.Task>(tasks.OrderBy(task => task.IsSelected));
                    break;

                default:
                    sortedTasks = new ObservableCollection<Model.Task>(tasks);
                    break;
            }

            return sortedTasks;
        }

        private void TaskSelectionReset(object obj)
        {
            foreach( var task in TaskList)
            {
                task.IsSelected = false;
            }

            IsQuestsVisible = true;
            IsStationsVisible = true;

            Document.Instance.Reset();
        }

        /// <summary>
        /// 아이템 선택시 Description 및 위키 링크 업데이트
        /// </summary>
        /// <param name="item"></param>
        private void ItemClicked(RequiredItem item)
        {
            _linkUrl = item.Item.WikiLink;

            Description = "≫ " + item.Item.Name + "\n";
            Description += "Requied Tasks : ";

            var requiredTask = TaskList.Where(task => task.RequiredItems != null && task.RequiredItems.Any(ri => ri.Item.Id == item.Item.Id));

            foreach (var task in requiredTask)
            {
                Description += "\n\t" + task.Name;
            }

            _task = null;
            OnPropertyChanged(nameof(Description));
        }

        /// <summary>
        /// task 선택 시 Description 및 위키 링크 업데이트
        /// </summary>
        private void DescriptionUpdate()
        {
            if (_task == null)
            {
                _linkUrl = WikiUrl;
            }
            else if (_task.Type == TaskType.Quest)
            {
                _linkUrl = _task?.WikiLink ?? WikiUrl;
            }
            else if (_task.Type == TaskType.Station)
            {
                _linkUrl = HideoutWikiUrl;
            }

            Description = "≫ " + (_task?.Name ?? "") + "\n";
            Description += "Trader : " + (_task?.Trader.ToString() ?? "") + "\n";
            Description += "Kappa " + (_task.IsKappa ? "○" : "×");

            OnPropertyChanged(nameof(Description));
        }

        /// <summary>
        /// 위키 링크 열기
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteOpenLink(object obj)
        {
            var url = _linkUrl ?? WikiUrl;

            if (!string.IsNullOrEmpty(url))
            {
                // .NET Core/.NET 5+ 에서는 UseShellExecute를 true로 설정해야 합니다.
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
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
            ValidItems = TaskManager.Instance.AllItems.ToDictionary(item => item.Id, item => item);

            TaskList.Clear();

            foreach (var task in TaskManager.Instance.AllQuests.Where(task => IsCurrentTask(task)))
            {
                TaskList.Add(task);
            }

            foreach (var station in TaskManager.Instance.AllStations)
            {
                TaskList.Add(station);
            }

            // 사용자의 진행상황 적용
            var selections = Document.Instance.Selections;

            if (selections != null || selections.Count != 0)
            {
                foreach (var task in TaskList)
                {
                    task.IsSelected = selections.Contains(task.Id);
                }
            }

            ControlEnabled = true;
            OnPropertyChanged(nameof(ControlEnabled));

            // 최초 1회만 Collects를 이용해 FilteredItems의 Quantity를 초기화
            UpdateItemLists(useCollects: true);
        }

        

        /// <summary>
        /// Task의 RequiredItems를 아이템별로 수량을 합산하여 FilteredItems로 변환
        /// </summary>
        private void UpdateItemLists(bool useCollects = false)
        {
            // 1. 아이템의 Quantity 정보 저장
            if (useCollects)
            {
                _allItemQuantities = Document.Instance.collects != null
                    ? new Dictionary<string, int>(Document.Instance.collects)
                    : new Dictionary<string, int>();
            }
            else
            {
                foreach (var item in FilteredItems)
                {
                    _allItemQuantities[item.Item.Id] = item.Quantity;
                }
            }

            // 2. TaskList 필터링
            var filteredTasks = TaskList.Where(t => IsCurrentTask(t) && !t.IsSelected);

            var grouped = filteredTasks
                .SelectMany(task => task.RequiredItems ?? Enumerable.Empty<RequiredItem>())
                .Where(ri => ri.Item != null
                             && !string.IsNullOrWhiteSpace(ri.Item.Id)
                             && ValidItems.ContainsKey(ri.Item.Id))
                .GroupBy(ri => ri.Item.Id)
                .Select(g =>
                {
                    var first = g.First();
                    var itemInfo = ValidItems[first.Item.Id];

                    int quantity = _allItemQuantities.TryGetValue(first.Item.Id, out var q) ? q : 0;

                    return new RequiredItem
                    {
                        Item = itemInfo,
                        Required = g.Sum(x => x.Required),
                        FoundInRaid = g.Any(x => x.FoundInRaid),
                        Quantity = quantity
                    };
                });

            // 3. 검색어 필터링 및 정렬
            var searchItems = grouped
                .Where(x => x.Item.Name.Contains(SearchItemText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Item.Name);

            var orderedItems = ItemSortElement switch
            {
                ItemSortType.Category => searchItems.OrderBy(x => x.Item.Category),
                ItemSortType.Collection_Rate => searchItems.OrderBy(x => x.Quantity / x.Required),
                _ => searchItems
            };

            FilteredItems = new ObservableCollection<RequiredItem>(orderedItems);

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

                        //해당 퀘스트의 아이템 필요량만큼 수집 아이템 수량을 감소
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

                    //해당 퀘스트의 아이템 필요량만큼 수집 아이템 수량을 감소

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

        #region interface implement

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Document.Instance.Selections = TaskList.Where(Task => Task.IsSelected).Select(task => task.Id).ToList();
            Document.Instance.collects = FilteredItems
                .Where(item => item.Quantity > 0)
                .ToDictionary(item => item.Item.Id, item => item.Quantity);

            Document.Instance.Dispose();
        }

        #endregion
    }
}
