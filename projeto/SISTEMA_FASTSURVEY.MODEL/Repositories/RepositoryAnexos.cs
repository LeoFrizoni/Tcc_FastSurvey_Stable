using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryAnexos : RepositoryBase<anexos>
    {
        public RepositoryAnexos(FastSurveyContext context, bool saveChanges)
            : base(context, saveChanges)
        {
        }
    }
}
