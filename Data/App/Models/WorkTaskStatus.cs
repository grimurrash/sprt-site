using System.ComponentModel.DataAnnotations;

namespace NewSprt.Data.App.Models
{
    public class WorkTaskStatus
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public const int Created = 1;
        public const int InProcess = 2;
        public const int Done = 3;
        public const int InArchive = 4;
    }
}