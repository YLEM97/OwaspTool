using Microsoft.AspNetCore.Components.Authorization;
using OwaspTool.DTOs;
using OwaspTool.Models.Database;

namespace OwaspTool.ViewModels
{
    public interface IWebAppRegistryViewModel
    {
        List<UserWebAppDTO> userWebApps { get; set; }
        bool IsLoading { get; set; }
        string NewName { get; set; }
        int NewLevelID { get; set; }
        Task LoadAsync();
        Task AddAsync();
        Task DeleteAsync(int userWebAppId);
        Task<List<Level>> GetLevelsAsync();
    }
    public class WebAppRegistryViewModel : IWebAppRegistryViewModel
    {
        private readonly IUserWebAppRepository _userWebAppRepository;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public WebAppRegistryViewModel(IUserWebAppRepository userWebAppRepository, AuthenticationStateProvider authenticationStateProvider)
        {
            _userWebAppRepository = userWebAppRepository;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public List<UserWebAppDTO> userWebApps { get; set; } = new();
        public bool IsLoading { get; set; }
        public string NewName { get; set; } = string.Empty;
        public int NewLevelID { get; set; }
        public async Task LoadAsync()
        {
            IsLoading = true;

            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Email);

            if (userIdClaim != null)
            {
                var email = userIdClaim.Value;
                userWebApps = await _userWebAppRepository.GetAllByUserAsync(email);

                // Evaluate IsSurveyCompleted sequentially to avoid concurrent DbContext usage
                foreach (var uwa in userWebApps)
                {
                    uwa.IsSurveyCompleted = await _userWebAppRepository.IsSurveyCompletedAsync(uwa.UserWebAppID);
                }
            }

            IsLoading = false;
        }
        public async Task AddAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Email);

            if(userIdClaim != null)
            {
                var email = userIdClaim.Value;
                await _userWebAppRepository.AddAsync(email, NewName, NewLevelID);
                NewName = string.Empty;
                NewLevelID = 0;
                await LoadAsync();
            }
        }
        public async Task DeleteAsync(int userWebAppId)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Email);

            if (userIdClaim != null)
            {
                var email = userIdClaim.Value;
                await _userWebAppRepository.DeleteAsync(userWebAppId, email);
                await LoadAsync();
            }
        }
        public async Task<List<Level>> GetLevelsAsync()
        {
            return await _userWebAppRepository.GetAllLevelsAsync();
        }
    }
}
