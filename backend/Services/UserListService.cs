using backend.Model;
using backend.Repositories;

namespace backend.Services
{
    public class UserListService
    {
        private readonly UserListRepository _repository;

        public UserListService()
        {
            _repository = new UserListRepository();
        }

        public UserListResponseModel GetUsers(int page, int pageSize, string? search)
        {
            var (users, totalCount) = _repository.GetUsers(page, pageSize, search);

            return new UserListResponseModel
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
