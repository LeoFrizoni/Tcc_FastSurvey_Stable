using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryTipoPesquisa : RepositoryBase<tipopesquisa>
    {
        public RepositoryTipoPesquisa(FastSurveyContext context, bool saveChanges = true)
            : base(context, saveChanges)
        {
        }
    }
}
