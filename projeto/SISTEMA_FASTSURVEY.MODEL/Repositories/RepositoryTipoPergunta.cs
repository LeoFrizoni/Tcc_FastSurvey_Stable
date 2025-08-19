using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryTipoPergunta : RepositoryBase<tipopergunta>
    {
        public RepositoryTipoPergunta(FastSurveyContext context, bool saveChanges = true)
            : base(context, saveChanges)
        {
        }
    }
}
