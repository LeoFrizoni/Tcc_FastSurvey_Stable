#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FASTSURVEY.Dtos.Login.Avatar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Login
{
    public sealed class AvatarService : IAvatarService
    {
        private readonly FastSurveyContext _ctx;
        private readonly IWebHostEnvironment _env;

        public AvatarService(FastSurveyContext ctx, IWebHostEnvironment env)
        {
            _ctx = ctx;
            _env = env;
        }

        public async Task<AvatarResponse> UploadAsync(int loginId, AvatarUploadRequest req, CancellationToken ct = default)
        {
            var login = await _ctx.Login.FirstOrDefaultAsync(l => l.Loginid == loginId, ct)
                        ?? throw new InvalidOperationException("Usu�rio inexistente.");

            // salva em wwwroot/uploads/avatars
            var root = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
            Directory.CreateDirectory(root);

            var ext = Path.GetExtension(req.File.FileName);
            var fileName = $"{loginId}_{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(root, fileName);

            using (var fs = new FileStream(fullPath, FileMode.Create))
                await req.File.CopyToAsync(fs, ct);

            var relUrl = $"/uploads/avatars/{fileName}";

            var avatar = await _ctx.Loginavatar.FirstOrDefaultAsync(a => a.Loginid == loginId, ct);
            if (avatar is null)
            {
                avatar = new Loginavatar
                {
                    Loginid = loginId,
                    Storageurl = relUrl,
                    Nomeoriginal = req.File.FileName,
                    Contenttype = req.File.ContentType,
                    Tamanhobytes = req.File.Length,
                    Criadoem = DateTime.UtcNow,
                    Versao = 1
                };
                _ctx.Loginavatar.Add(avatar);
            }
            else
            {
                avatar.Storageurl = relUrl;
                avatar.Nomeoriginal = req.File.FileName;
                avatar.Contenttype = req.File.ContentType;
                avatar.Tamanhobytes = req.File.Length;
                avatar.Atualizadoem = DateTime.UtcNow;
                avatar.Versao = (avatar.Versao <= 0 ? 1 : avatar.Versao + 1);
            }

            await _ctx.SaveChangesAsync(ct);

            return new AvatarResponse { Url = avatar.Storageurl, Versao = avatar.Versao };
        }

        public async Task<bool> RemoverAsync(int loginId, CancellationToken ct = default)
        {
            var avatar = await _ctx.Loginavatar.FirstOrDefaultAsync(a => a.Loginid == loginId, ct);
            if (avatar is null) return true;

            _ctx.Loginavatar.Remove(avatar);
            await _ctx.SaveChangesAsync(ct);
            return true;
        }
    }
}
