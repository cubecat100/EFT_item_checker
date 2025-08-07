using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EFT_item_checker.Model
{
    public class Item
    {
        public string Id { get; set; } // 아이템의 고유 ID
        public string Name { get; set; }
        public string ShortName { get; set; } // 아이템의 짧은 이름
        public string Category { get; set; }

        private string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            set
            {
                // 아이콘 경로가 null이거나 비어있으면 기본 아이콘 경로를 사용
                if (string.IsNullOrEmpty(value))
                {
                    _iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/Images", "default_icon.png");
                }
                else
                {
                    _iconPath = value;
                }
            }
        }

        public string WikiLink { get; internal set; }
    }

    public class RequiredItem
    {
        public Item Item { get; set; } // 필요한 아이템 객체
        public int Required { get; set; } // 필요한 수량

        private int _quantity;
        public int Quantity //수집 수량
        {
            get => _quantity;
            set
            {
                // TextBox에서 값이 비어있거나 null일 때 0으로 처리
                if (value == null)
                {
                    _quantity = 0;
                }
                else if (value < 0)
                {
                    _quantity = 0; // 수량이 음수가 되지 않도록 보장
                }
                else if (value > Required)
                {
                    _quantity = Required;
                }
                else
                {
                    _quantity = value;
                }
            }
        }

        public bool FoundInRaid { get; set; } // 레이드에서 찾아야 하는지 여부
    }

    // 목록의 각 항목(퀘스트, 모듈)을 위한 기본 클래스
    public class Task : INotifyPropertyChanged
    {
        public string Id { get; set; } // 퀘스트 또는 모듈의 고유 ID
        public string Name { get; set; }
        public TaskType Type { get; set; }

        public string WikiLink { get; set; } = ""; // 퀘스트 위키 링크 URL

        public bool IsKappa { get; set; } // Kappa 퀘스트 여부

        public TraderType Trader { get; set; } // 퀘스트를 주는 트레이더 이름

        public List<string> RequiredTasks { get; set; } = new List<string>(); // 선행 Task ID 목록

        public List<RequiredItem> RequiredItems { get; set; } = new List<RequiredItem>();

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(); // IsSelected 값이 바뀌면 UI에 알림
            }
        }

        private Visibility _isVisible = Visibility.Visible;

        public Visibility IsVisibility
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditionPass { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public enum SortType
    {
        Name, // 이름 오름차순
        Trader, // 트레이더 내림차순
        Type, // 타입 내림차순
        Completed, // 완료 여부 내림차순
    }

    public enum TaskType
    {
        Item,
        Quest,
        Station,
    }

    public enum SettingType
    {
        //언어 설정
        Language,
        TaskSort,
        GameEdition,
    }

    public enum LanguageType
    {
        en,
        ko,
        ru,
    }

    public enum TraderType
    {
        // 트레이더 이름
        Prapor,
        Therapist,
        Fence,
        Skier,
        Peacekeeper,
        Mechanic,
        Ragman,
        Jaeger,
        LightKeeper,
        Btr,
        Ref,

        Unknown,
    }

    public enum ItemSortType
    {
        Name,
        Category,
        Collection_Rate,
    }

    public enum EditionType
    {
        Standard,
        Left_Behind,
        Prepare_for_Escape,
        Edge_of_Darkness,
        The_Unheard,
    }
}
