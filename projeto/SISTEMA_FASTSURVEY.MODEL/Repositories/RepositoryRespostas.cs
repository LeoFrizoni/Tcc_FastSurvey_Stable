using SISTEMA_FASTSURVEY.MODEL.Models;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryRespostas : RepositoryBase<respostas>
    {
        public RepositoryRespostas(FastSurveyContext context, bool saveChanges = true)
            : base(context, saveChanges)
        {
        }
    }
}
