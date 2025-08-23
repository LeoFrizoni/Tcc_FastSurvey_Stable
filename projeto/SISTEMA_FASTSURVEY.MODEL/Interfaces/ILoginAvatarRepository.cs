using SISTEMA_FASTSURVEY.MODEL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Interfaces
{
    public interface ILoginAvatarRepository : IRepository<Loginavatar>
    {
        // Métodos adicionais específicos para LoginAvatar, se precisar
        Task<Loginavatar> GetByLoginIdAsync(int loginId);
    }
}
