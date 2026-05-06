namespace backend.Model
{
    public class UserListItemModel
    {
        public string FullName { get; set; }
        public string Ssn { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class UserListResponseModel
    {
        public List<UserListItemModel> Users { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
