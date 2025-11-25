#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Services.Cache;

namespace FASTSURVEY.Services.Resultados
{
    internal static class ResultadosCacheHelper
    {
        private const string PesquisaPrefix = "resultados:pesquisa:";
        private const string PerguntaPrefix = "resultados:pergunta:";
        private const string GraficosPrefix = "resultados:graficos:";
        private const string DashboardPrefix = "resultados:dashboard:";

        public static string PesquisaKey(int pesquisaId) => $"{PesquisaPrefix}{pesquisaId}";
        public static string PerguntaKey(int perguntaId) => $"{PerguntaPrefix}{perguntaId}";
        public static string GraficosKey(int pesquisaId) => $"{GraficosPrefix}{pesquisaId}";
        public static string DashboardKey(int loginId) => $"{DashboardPrefix}{loginId}";

        public static Task InvalidatePesquisaAsync(
            ICacheService cache,
            int pesquisaId,
            IEnumerable<int>? perguntaIds,
            CancellationToken ct = default
        )
        {
            if (cache is null)
                return Task.CompletedTask;

            var tasks = new List<Task>
            {
                cache.RemoveAsync(PesquisaKey(pesquisaId), ct),
                cache.RemoveAsync(GraficosKey(pesquisaId), ct),
            };

            if (perguntaIds is not null)
            {
                foreach (var perguntaId in perguntaIds.Where(pid => pid > 0).Distinct())
                {
                    tasks.Add(cache.RemoveAsync(PerguntaKey(perguntaId), ct));
                }
            }

            return Task.WhenAll(tasks);
        }
    }
}
