using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Parcial_VargasMedina.Models;

namespace Parcial_VargasMedina.Services
{
    public interface ICursosCacheService
    {
        Task<List<Curso>?> GetCursosActivosAsync();
        Task SetCursosActivosAsync(List<Curso> cursos);
        Task InvalidarCacheAsync();
    }

    public class CursosCacheService : ICursosCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CursosCacheService> _logger;
        private const string CACHE_KEY = "cursos_activos";
        private const int CACHE_EXPIRATION_SECONDS = 60;

        public CursosCacheService(IDistributedCache cache, ILogger<CursosCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<Curso>?> GetCursosActivosAsync()
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(CACHE_KEY);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Cursos obtenidos del cache");
                    return JsonConvert.DeserializeObject<List<Curso>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener datos del cache");
            }
            
            return null;
        }

        public async Task SetCursosActivosAsync(List<Curso> cursos)
        {
            try
            {
                var serializedCursos = JsonConvert.SerializeObject(cursos, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_EXPIRATION_SECONDS)
                };

                await _cache.SetStringAsync(CACHE_KEY, serializedCursos, options);
                _logger.LogInformation($"Cursos guardados en cache por {CACHE_EXPIRATION_SECONDS} segundos");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al guardar datos en cache");
            }
        }

        public async Task InvalidarCacheAsync()
        {
            try
            {
                await _cache.RemoveAsync(CACHE_KEY);
                _logger.LogInformation("Cache de cursos invalidado");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al invalidar cache");
            }
        }
    }
}