namespace NewSprt.ViewModels
{
    public class Pagination
    {
        public int Rows { get; set; }
        public int CurrentPage { get; set; }

        public Pagination(int rows, int currentPage = 1)
        {
             Rows = rows;
             CurrentPage = currentPage;
        }
    }
}