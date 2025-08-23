using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Dtos.Login.Avatar
{
    public class AvatarUploadRequest
    {
        [Required] public IFormFile File { get; set; } = default!;
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? W { get; set; }
        public int? H { get; set; }
        public float? Rotate { get; set; }
        public float? Scale { get; set; }
    }
}
