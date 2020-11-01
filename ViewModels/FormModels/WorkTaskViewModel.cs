using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NewSprt.Data.App.Models;

namespace NewSprt.ViewModels.FormModels
{
    public class WorkTaskViewModel
    {
        [DefaultValue(0)] public int Id { get; set; }

        [Required(ErrorMessage = "Не указано наименование")]
        [DisplayName("Наименование")]
        public string DocumentName { get; set; }

        [Required(ErrorMessage = "Не указан номер документа")]
        [DisplayName("Номер документа")]
        public string DocumentNumber { get; set; }

        [Required(ErrorMessage = "Не указано описание")]
        [DisplayName("Описание")]
        public string Discription { get; set; }

        [Required(ErrorMessage = "Не выбран исполнитель")]
        [DisplayName("Исполнитель")]
        public int TaskResponsibleId { get; set; }

        [DefaultValue(false)]
        [DisplayName("Обратить особое внимание")]
        public bool IsUrgent { get; set; }

        [DefaultValue(true)]
        [DisplayName("Переодичность")]
        public bool IsRepeat { get; set; }

        [DisplayName("Срок исполнения")] 
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime CompletionDate { get; set; }

        [DisplayName("Срок исполнения (переодичность)")]
        public string AdditionToDeadlines { get; set; }

        [DisplayName("Поставил задачу")]
        public int TaskManagerId { get; set; }
        [DisplayName("Дата создания задачи")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:ii}")]
        public DateTime CreateDate { get; set; }
        [DisplayName("Дата последнего изменения задачи")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd HH:ii}")]
        public DateTime UpdateDate { get; set; }
        
        public void Set(WorkTask task)
        {
            Id = task.Id;
            DocumentName = task.Name;
            DocumentNumber = task.DocumentNumber;
            Discription = task.Discription;
            TaskResponsibleId = task.TaskResponsibleId;
            TaskManagerId = task.TaskManagerId;
            IsUrgent = task.IsUrgent;
            IsRepeat = task.IsRepeat;
            CompletionDate = task.CompletionDate;
            AdditionToDeadlines = task.AdditionToDeadlines;
            CreateDate = task.CreateDate;
            UpdateDate = task.UpdateDate;
        }
    }
}