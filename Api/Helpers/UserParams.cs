namespace Api.Helpers
{
    public class UserParams
    {
        private const int MaxPageSize = 50;

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;

        public int PageSize {
            get => _pageSize;
            set => _pageSize = (MaxPageSize > value) ? value : MaxPageSize;
        }

        public string CurrentUserName  { get; set; }

        public string Gender { get; set; }
    }
}