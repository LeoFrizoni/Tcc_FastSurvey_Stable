using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryLogin : RepositoryBase<login>
    {
        public RepositoryLogin(FastSurveyContext context, bool saveChanges = true)
            : base(context, saveChanges)
        {
        }
    }
}
