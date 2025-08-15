using SISTEMA_FASTSURVEY.MODEL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryTipoPesquisa : RepositoryBase<tipopesquisa>
    {
        public RepositoryTipoPesquisa(FastSurveyContext context, bool saveChanges) : base(context, saveChanges)
        {
        }
    }
}
