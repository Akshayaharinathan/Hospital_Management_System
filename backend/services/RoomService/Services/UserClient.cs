using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using RoomService.Dtos;

namespace RoomService.Services
{
    public class UserClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserClient(HttpClient http, IHttpContextAccessor accessor)
        {
            _http = http;
            _httpContextAccessor = accessor;
        }

        private void ForwardJwt()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                _http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
            }
        }

        // Admin or Doctor only per your UserService policy
        public async Task<StaffSummaryDto?> GetStaffByIdAsync(int staffId, CancellationToken ct = default)
        {
            ForwardJwt();
            var resp = await _http.GetAsync($"/api/staff/{staffId}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<StaffSummaryDto>(cancellationToken: ct);
        }
    }
}
