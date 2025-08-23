using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface IExternalLoginRepository : IRepository<Externallogins>
    {
        // Métodos adicionais específicos para ExternalLogin
        Task<Externallogins> GetByProviderAsync(string provider, string providerUserId);
    }
}
