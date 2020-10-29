using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewSprt.Data.App.Models
{
    public class WorkTask : BaseModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string DocumentNumber { get; set; }
        public string Discription { get; set; }

        public int DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public Department Department { get; set; }
        public int ResponsibleId { get; set; }
        [ForeignKey(nameof(ResponsibleId))]
        public User ResponsibleUser { get; set; }
        public int ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public User ManagerUser { get; set; }
        
        [DefaultValue(1)]
        public int StatusId { get; set; }
        [ForeignKey(nameof(StatusId))]
        public WorkTaskStatus Status { get; set; }
        [DefaultValue(false)]
        public bool IsUrgent { get; set; }
        [DefaultValue(false)]
        public bool IsRepeat { get; set; }
        public DateTime CompletionDate { get; set; }
        public string TimelineForCompliance { get; set; }
    
        public string FilePath { get; set; }
    }
}