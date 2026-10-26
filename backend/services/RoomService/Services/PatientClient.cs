using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using RoomService.Dtos;

namespace RoomService.Services
{
    public class PatientClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatientClient(HttpClient http, IHttpContextAccessor accessor)
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

        // Receptionist path
        public async Task<PatientSummaryDto?> GetBasicAsync(int patientId, CancellationToken ct = default)
        {
            ForwardJwt();
            var resp = await _http.GetAsync($"/api/Patient/{patientId}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<PatientSummaryDto>(cancellationToken: ct);
        }

        // Doctor/Admin path
        public async Task<PatientSummaryDto?> GetDetailedAsync(int patientId, CancellationToken ct = default)
        {
            ForwardJwt();
            var resp = await _http.GetAsync($"/api/Patient/{patientId}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<PatientSummaryDto>(cancellationToken: ct);
        }
    }
}