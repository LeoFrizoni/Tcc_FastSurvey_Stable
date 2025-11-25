using System.Threading;
using System.Threading.Tasks;

namespace FASTSURVEY.Services.Analises
{
    public interface ISentimentAnalysisDispatcher
    {
        Task AnalyzeAsync(int respostaId, string texto, CancellationToken ct = default);
    }
}
