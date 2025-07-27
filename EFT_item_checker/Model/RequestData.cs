using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFT_item_checker.Model
{
    //api 데이터 매핑 클래스

    public class RequestItemData
    {
        public RequestItems Data { get; set; }
    }

    public class RequestItems
    {
        public List<RequestItem> Items { get; set; } = new List<RequestItem>();
    }

    public class RequestItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public string IconLink { get; set; } = "";
        public DateTime Updated { get; set; } // 업데이트 날짜
    }

    public class RequestTaskData
    {
        public RequestQuests Data { get; set; }

    }

    public class RequestQuests
    {
        public List<RequestQuest> Tasks { get; set; } = new List<RequestQuest>();
    }

    public class RequestQuest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool KappaRequired { get; set; }
        public string WikiLink { get; set; }
        public List<Objectives> Objectives { get; set; } // 아이템 목록
        public List<RequiredQuest> TaskRequirements { get; set; } // 선행 퀘스트 ID 목록
    }

    public class RequiredQuest
    {
        public ReqQuest Task { get; set; } // 선행 퀘스트 ID
    }

    public class ReqQuest
    {
        public string Id { get; set; } // 선행 퀘스트 ID
    }

    public class Objectives
    {
        public RequestItem Item { get; set; } = new RequestItem();
        public int Count { get; set; } // 필요한 아이템 수량
        public bool FoundInRaid { get; set; } // 레이드에서 찾아야하는지 여부
    }

    public class RequestStationData
    {
        public RequestStations Data { get; set; }
    }

    public class RequestStations
    {
        public List<RequestStation> hideoutStations { get; set; } = new List<RequestStation>();
    }

    public class RequestStation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<StationLevel> Levels { get; set; } // 스테이션 레벨
    }

    public class StationLevel
    {
        public string Id { get; set; } // 스테이션 레벨 ID
        public int Level { get; set; }

        public List<RequiredItemData> ItemRequirements { get; set; } = new List<RequiredItemData>();

        public List<RequirementStationLevel> StationLevelRequirements { get; set; } = new List<RequirementStationLevel>();

        public List<StationCraftResult> Crafts { get; set; } = new List<StationCraftResult>();
    }

    public class RequiredItemData
    {
        public RequestItem Item { get; set; } // 요구되는 아이템
        public int Count { get; set; } // 요구되는 수량
    }

    public class RequirementStationLevel
    {
        public RequirementStationData Station { get; set; } // 요구되는 스테이션 ID
        public int Level { get; set; } // 요구되는 레벨
    }

    public class RequirementStationData
    {
        public string Id { get; set; } // 요구되는 스테이션 ID
        public string Name { get; set; } // 요구되는 레벨
    }

    public class StationCraftResult
    {
        public List<RequestItems> rewardItems { get; set; } = new List<RequestItems>(); // 보상 아이템 목록
    }

}
