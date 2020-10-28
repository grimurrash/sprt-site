namespace NewSprt.ViewModels
{
    public class Pagination
    {
        public int Rows { get; }
        public int CurrentPage { get; }

        public Pagination(int rows, int currentPage = 1)
        {
             Rows = rows;
             CurrentPage = currentPage;
        }
    }
}