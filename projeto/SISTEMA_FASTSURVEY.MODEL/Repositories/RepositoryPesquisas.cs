using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryPesquisas : RepositoryBase<pesquisas>
    {
        public RepositoryPesquisas(FastSurveyContext context, bool saveChanges = true)
            : base(context, saveChanges)
        {
        }
    }
}
