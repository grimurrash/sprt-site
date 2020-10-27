using System;
using System.Collections.Generic;

namespace NewSprt.Data.Zarnica.Models
{
    public class ОthersClasses
    {
        
    }
    
    public class TrainingStatistic
    {
        public int AllCount { get; set; }
        public int AllACategoryCount { get; set; }
        public int AllCTypeCount { get; set; }
        public int AllSPOTypeCount { get; set; }
        
        public int OneCount { get; set; }
        public int OneCTypeCount { get; set; }
        public int TwoCount { get; set; }
        public int TwoCTypeCount { get; set; }
        public int ThreeCount { get; set; }
        public int ThreeCTypeCount { get; set; }

        public MilitaryComissariat MilitaryComissariat { get; set; }
        
        public List<TeamDistrictDistribution> Teams { get; set; }

        public TrainingStatistic()
        {
            Teams = new List<TeamDistrictDistribution>();
        }
    }
    
    public class DayStatistic
    {
        public DateTime Day { get; set; }
        public int AllCount { get; set; }
        public int CDriversCount { get; set; }
        public int MtlbCount { get; set; }

        public int BdbDriversCount { get; set; }
        public int DDriversCount { get; set; }
        public int BDriversCount { get; set; }
        public int EDriversCount { get; set; }
        public int SvedDriversCount { get; set; }
        public int AnotFormCount { get; set; }
        public int AThreeFormCount { get; set; }
        public int OneFormCount { get; set; }
        public int TwoFormCount { get; set; }
        public int BThreeFormCount { get; set; }
        
        public List<TeamDistrictDistribution> Teams { get; set; }

        public DayStatistic()
        {
            Teams = new List<TeamDistrictDistribution>();
        }
    }
    
    
}