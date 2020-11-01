using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NewSprt.ViewModels.FormModels;

namespace NewSprt.Data.App.Models
{
    /// <summary>
    /// Таблица задач
    /// </summary>
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
        public int TaskResponsibleId { get; set; }
        [ForeignKey(nameof(TaskResponsibleId))]
        public User TaskResponsibleUser { get; set; }
        public int TaskManagerId { get; set; }
        [ForeignKey(nameof(TaskManagerId))]
        public User TaskManagerUser { get; set; }
        
        [DefaultValue(false)]
        public bool IsArchive { get; set; }
        [DefaultValue(false)]
        public bool IsUrgent { get; set; }
        [DefaultValue(false)]
        public bool IsRepeat { get; set; }
        public DateTime CompletionDate { get; set; }
        public string AdditionToDeadlines { get; set; }
    
        public string FilePath { get; set; }

        public string ComplitionTimeLine => !IsRepeat ? $"<spandata-placement='top' data-toggle='tooltip' title='До указанной даты'><i class='fas fa-calendar-check'></i> {CompletionDate.ToShortDateString()}</span>" : ShortAdditionToDeadlines;

        public string ShortDiscription => Discription.Length > 90 ? Discription.Substring(1, 90) + "..." : Discription;
        public string ShortAdditionToDeadlines => AdditionToDeadlines.Length > 40 ? AdditionToDeadlines.Substring(1, 40) + "..." : AdditionToDeadlines;

        public void Set(WorkTaskViewModel model, User manager, User responsible)
        {
            Name = model.DocumentName;
            DocumentNumber = model.DocumentNumber;
            Discription = model.Discription;
            TaskManagerId = manager.Id;
            TaskResponsibleId = responsible.Id;
            DepartmentId = responsible.DepartmentId;
            IsRepeat = model.IsRepeat;
            IsUrgent = model.IsUrgent;
            CompletionDate = model.IsRepeat ? DateTime.Now.Date : model.CompletionDate.Date;
            AdditionToDeadlines = model.IsRepeat ? model.AdditionToDeadlines : "Один раз";
            IsArchive = false;
        }
    }
}