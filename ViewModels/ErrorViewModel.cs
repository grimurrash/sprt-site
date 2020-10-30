namespace NewSprt.ViewModels
{
    /// <summary>
    /// Класс для вывода ошибок
    /// </summary>
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}