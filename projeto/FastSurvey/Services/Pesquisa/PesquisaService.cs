// FASTSURVEY/Services/Pesquisa/PesquisaService.cs
#nullable enable
using System.Text.Json;
using FASTSURVEY.Dtos.Pesquisas;
using FastSurvey.Dtos.Pesquisas;
using FASTSURVEY.Services.Cache;
using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Models;

namespace FASTSURVEY.Services.Pesquisa
{
    public class PesquisaService : IPesquisaService
    {
        private readonly FastSurveyContext _ctx;
        private readonly ICacheService _cache;
        private const string CACHE_PREFIX = "pesquisa:";
        private const string CACHE_LIST_PREFIX = "pesquisa:list:";

        public PesquisaService(FastSurveyContext ctx, ICacheService cache)
        {
            _ctx = ctx;
            _cache = cache;
        }

        // ------------------ CREATE ------------------
        public async Task<int> CriarAsync(CriarPesquisaRequest req, CancellationToken ct = default)
        {
            // Valida JSON do template
            string template = string.IsNullOrWhiteSpace(req.TemplateJson)
                ? "[]"
                : req.TemplateJson.Trim();
            try
            {
                JsonDocument.Parse(template);
            }
            catch
            {
                template = "[]";
            }

            var entity = new Pesquisas
            {
                LoginId = req.LoginId,
                TipoPesquisaId = req.TipoPesquisaId,
                PastaId = req.PastaId,
                Titulo = req.Titulo,
                Descricao = req.Descricao ?? string.Empty,
                TemplateJson = template,
                QRCodeUrl = req.QRCodeUrl,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = null,

                TemLimitadorTempo = req.TemLimitadorTempo,
                DataFechamento = req.DataFechamento,
                IsInterativa = req.IsInterativa,
                PermiteRespostasAnonimas = req.PermiteRespostasAnonimas,
                LimiteRespostas = req.LimiteRespostas,
                Ativa = req.Ativa,
                // Se sua entidade tiver estes campos, descomente:
                // Slug = req.Slug,
                // RequerIdentificacao = req.RequerIdentificacao,
                // Instrucoes = req.Instrucoes,
                // MostrarProgresso = req.MostrarProgresso,
                // PermitirEdicao = req.PermitirEdicao,
                // TempoLimitePorPergunta = req.TempoLimitePorPergunta
            };

            _ctx.Pesquisas.Add(entity);
            await _ctx.SaveChangesAsync(ct);

            // Parse templateJson e cria as perguntas no banco
            await ParseTemplateAndCreatePerguntas(entity.PesquisaId, template, ct);

            await InvalidateUserCache(req.LoginId, ct);
            return entity.PesquisaId;
        }

        /// <summary>
        /// Parse o TemplateJson e cria os registros de Perguntas e OpcoesPerguntas no banco
        /// </summary>
        private async Task ParseTemplateAndCreatePerguntas(int pesquisaId, string templateJson, CancellationToken ct = default)
        {
            try
            {
                Console.WriteLine($"🔄 Parseando templateJson para pesquisa {pesquisaId}");
                Console.WriteLine($"📄 JSON: {templateJson}");

                var blocos = JsonSerializer.Deserialize<List<BlocoTemplateDto>>(templateJson);
                if (blocos == null || !blocos.Any())
                {
                    Console.WriteLine("⚠️ Nenhum bloco encontrado no templateJson");
                    return;
                }

                Console.WriteLine($"✅ {blocos.Count} blocos encontrados");

                int ordem = 0;
                var perguntasMap = new Dictionary<string, int>(); // temp-id -> real-id

                foreach (var bloco in blocos)
                {
                    Console.WriteLine($"🔍 Processando bloco {ordem + 1}: tipo={bloco.Tipo}, texto={bloco.Texto}");

                    // Mapeia tipo de pergunta
                    int tipoPerguntaId = MapTipoPergunta(bloco.Tipo);

                    var pergunta = new Perguntas
                    {
                        PesquisaId = pesquisaId,
                        TipoPerguntaId = tipoPerguntaId,
                        Texto = bloco.Texto,
                        Ordem = bloco.Ordem ?? ordem,
                        TemGabarito = bloco.TemGabarito ?? false,
                        PermiteMultiplasSelecao = bloco.PermitirMultiplaSelecao ?? bloco.Tipo.Equals("multipla", StringComparison.OrdinalIgnoreCase),
                        PontuacaoTotal = bloco.PontuacaoTotal ?? 0,
                        TempoLimite = bloco.TempoLimite,
                        MostrarExplicacao = bloco.MostrarExplicacao ?? false,
                        DeletadoEm = null
                    };

                    _ctx.Perguntas.Add(pergunta);
                    await _ctx.SaveChangesAsync(ct); // Salva para obter o PerguntaId

                    Console.WriteLine($"✅ Pergunta criada com ID {pergunta.PerguntaId}");

                    // Guarda mapeamento do ID temporário para o real
                    if (!string.IsNullOrEmpty(bloco.Id))
                    {
                        perguntasMap[bloco.Id] = pergunta.PerguntaId;
                    }

                    // Se tiver opções (objetiva ou multipla), cria as OpcoesPerguntas
                    if (bloco.Opcoes != null && bloco.Opcoes.Any())
                    {
                        Console.WriteLine($"📋 Processando {bloco.Opcoes.Count} opções");

                        // Detecta textos duplicados e adiciona sufixo se necessário
                        var textosUsados = new Dictionary<string, int>();
                        var opcoesComTextoUnico = bloco.Opcoes.Select(opcao =>
                        {
                            string textoOriginal = opcao.Texto ?? "";
                            string textoFinal = textoOriginal;

                            if (textosUsados.ContainsKey(textoOriginal))
                            {
                                textosUsados[textoOriginal]++;
                                textoFinal = $"{textoOriginal} ({textosUsados[textoOriginal]})";
                                Console.WriteLine($"⚠️ Texto duplicado detectado: '{textoOriginal}' renomeado para '{textoFinal}'");
                            }
                            else
                            {
                                textosUsados[textoOriginal] = 1;
                            }

                            return new { Opcao = opcao, TextoFinal = textoFinal };
                        }).ToList();

                        int ordemOpcao = 0;
                        foreach (var item in opcoesComTextoUnico)
                        {
                            var opcao = item.Opcao;

                            // Para perguntas objetivas, usa o corretaIndex para determinar a correta
                            bool isCorreta = opcao.Correta ?? false;
                            if (bloco.Tipo.Equals("objetiva", StringComparison.OrdinalIgnoreCase) && bloco.CorretaIndex.HasValue)
                            {
                                isCorreta = (opcao.OpcaoId == bloco.CorretaIndex.Value) || (ordemOpcao + 1 == bloco.CorretaIndex.Value);
                            }

                            var opcaoPergunta = new OpcoesPergunta
                            {
                                PerguntaId = pergunta.PerguntaId,
                                Texto = item.TextoFinal, // Usa o texto com sufixo se houver duplicata
                                Correta = isCorreta,
                                Ordem = opcao.Ordem ?? ordemOpcao,
                                Ativa = true,
                                Pontuacao = opcao.Pontuacao ?? 0,
                                Explicacao = opcao.Explicacao,
                                DeletadoEm = null
                            };

                            _ctx.OpcoesPergunta.Add(opcaoPergunta);
                            Console.WriteLine($"  ➤ Opção {ordemOpcao + 1}: {item.TextoFinal} (correta: {isCorreta})");
                            ordemOpcao++;
                        }

                        await _ctx.SaveChangesAsync(ct);
                        Console.WriteLine($"✅ {bloco.Opcoes.Count} opções criadas");
                    }

                    ordem++;
                }

                Console.WriteLine($"🎉 Total de {ordem} perguntas processadas com sucesso!");

                // Atualiza o TemplateJson com os IDs reais das perguntas
                await UpdateTemplateJsonWithRealIds(pesquisaId, templateJson, perguntasMap, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao parsear templateJson: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                // Não lança exceção para não quebrar a criação da pesquisa
            }
        }

        /// <summary>
        /// Atualiza o TemplateJson substituindo IDs temporários pelos IDs reais das perguntas
        /// </summary>
        private async Task UpdateTemplateJsonWithRealIds(int pesquisaId, string templateJson, Dictionary<string, int> perguntasMap, CancellationToken ct = default)
        {
            try
            {
                var blocos = JsonSerializer.Deserialize<List<BlocoTemplateDto>>(templateJson);
                if (blocos == null || !blocos.Any())
                    return;

                int index = 0;
                foreach (var bloco in blocos)
                {
                    // Usa o mapeamento se existir, senão busca pela ordem
                    if (!string.IsNullOrEmpty(bloco.Id) && perguntasMap.ContainsKey(bloco.Id))
                    {
                        bloco.PerguntaId = perguntasMap[bloco.Id].ToString();
                    }
                    else
                    {
                        // Fallback: busca pela ordem
                        var pergunta = await _ctx.Perguntas
                            .Where(p => p.PesquisaId == pesquisaId && p.Ordem == index)
                            .FirstOrDefaultAsync(ct);

                        if (pergunta != null)
                        {
                            bloco.PerguntaId = pergunta.PerguntaId.ToString();
                        }
                    }
                    index++;
                }

                // Serializa de volta e atualiza usando SQL direto para evitar conflito com trigger
                string updatedTemplate = JsonSerializer.Serialize(blocos);

                // Usa SQL direto ao invés de SaveChanges para evitar problema com trigger
                var dataAtualizacao = DateTime.UtcNow;
                var rowsAffected = await _ctx.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE ""Pesquisas"" 
                       SET ""TemplateJson"" = {updatedTemplate}, 
                           ""DataAtualizacao"" = {dataAtualizacao}
                       WHERE ""PesquisaId"" = {pesquisaId}",
                    ct
                );

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"✅ TemplateJson atualizado com IDs reais para {rowsAffected} registro(s)");
                }
                else
                {
                    Console.WriteLine($"⚠️ Nenhum registro atualizado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao atualizar templateJson com IDs reais: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Mapeia o tipo de pergunta do template para o ID no banco
        /// </summary>
        private int MapTipoPergunta(string tipo)
        {
            return tipo.ToLower() switch
            {
                "discursiva" => 1,
                "objetiva" => 2,
                "multipla" => 3,
                "escala" => 4,
                "data" => 5,
                "hora" => 6,
                "arquivo" => 7,
                _ => 1 // Default: discursiva
            };
        }

        // ------------------ UPDATE (PUT) ------------------
        public async Task<bool> AtualizarAsync(
            int id,
            AtualizarPesquisaRequest req,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx.Pesquisas.FirstOrDefaultAsync(x => x.PesquisaId == id, ct);
            if (entity is null)
                return false;

            entity.Titulo = req.Titulo;
            entity.Descricao = req.Descricao ?? entity.Descricao;
            entity.PastaId = req.PastaId ?? entity.PastaId;
            entity.TipoPesquisaId = req.TipoPesquisaId ?? entity.TipoPesquisaId;
            entity.QRCodeUrl = req.QRCodeUrl ?? entity.QRCodeUrl;

            if (req.TemLimitadorTempo.HasValue)
                entity.TemLimitadorTempo = req.TemLimitadorTempo.Value;
            if (req.DataFechamento.HasValue)
                entity.DataFechamento = req.DataFechamento;
            if (req.IsInterativa.HasValue)
                entity.IsInterativa = req.IsInterativa.Value;
            if (req.PermiteRespostasAnonimas.HasValue)
                entity.PermiteRespostasAnonimas = req.PermiteRespostasAnonimas.Value;
            if (req.LimiteRespostas.HasValue)
                entity.LimiteRespostas = req.LimiteRespostas;
            if (req.Ativa.HasValue)
                entity.Ativa = req.Ativa.Value;

            // Se a entidade tiver estes campos, descomente:
            // if (req.Slug is not null) entity.Slug = req.Slug;
            // if (req.RequerIdentificacao.HasValue) entity.RequerIdentificacao = req.RequerIdentificacao.Value;
            // if (req.Instrucoes is not null) entity.Instrucoes = req.Instrucoes;
            // if (req.MostrarProgresso.HasValue) entity.MostrarProgresso = req.MostrarProgresso.Value;
            // if (req.PermitirEdicao.HasValue) entity.PermitirEdicao = req.PermitirEdicao.Value;
            // if (req.TempoLimitePorPergunta.HasValue) entity.TempoLimitePorPergunta = req.TempoLimitePorPergunta;

            entity.DataAtualizacao = DateTime.UtcNow;

            await _ctx.SaveChangesAsync(ct);
            await InvalidatePesquisaCache(id, entity.LoginId, ct);
            return true;
        }

        // ------------------ DELETE ------------------
        public async Task<bool> ExcluirAsync(int id, CancellationToken ct = default)
        {
            var entity = await _ctx
                .Pesquisas
                .FirstOrDefaultAsync(x => x.PesquisaId == id, ct);
            if (entity is null)
                return false;

            try
            {
                _ctx.Pesquisas.Remove(entity);
                var deleted = await _ctx.SaveChangesAsync(ct);

                if (deleted > 0)
                    await InvalidatePesquisaCache(id, entity.LoginId, ct);

                return deleted > 0;
            }
            catch (Exception ex)
            {
                // Log do erro para debug
                Console.WriteLine($"Erro ao excluir pesquisa {id}: {ex.Message}");
                return false;
            }
        }

        // ------------------ GET BY ID ------------------
        public async Task<PesquisaResponse?> ObterPorIdAsync(int id, CancellationToken ct = default)
        {
            var cacheKey = $"{CACHE_PREFIX}{id}";
            return await _cache.GetOrSetAsync<PesquisaResponse?>(
                cacheKey,
                async token =>
                {
                    var q = await _ctx
                        .Pesquisas.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.PesquisaId == id, token);
                    if (q is null)
                        return null;

                    return new PesquisaResponse
                    {
                        PesquisaId = q.PesquisaId,
                        Titulo = q.Titulo,
                        Descricao = q.Descricao,
                        LoginId = q.LoginId,
                        TipoPesquisaId = q.TipoPesquisaId,
                        PastaId = q.PastaId,
                        TemplateJson = q.TemplateJson ?? "[]",
                        QRCodeUrl = q.QRCodeUrl,
                        DataCriacao = q.DataCriacao,
                        DataAtualizacao = q.DataAtualizacao,
                        TemLimitadorTempo = q.TemLimitadorTempo,
                        DataFechamento = q.DataFechamento,
                        IsInterativa = q.IsInterativa,
                        PermiteRespostasAnonimas = q.PermiteRespostasAnonimas,
                        LimiteRespostas = q.LimiteRespostas,
                        Ativa = q.Ativa,
                    };
                },
                absoluteExpiration: TimeSpan.FromMinutes(15),
                ct: ct
            );
        }

        // ------------------ LIST / SEARCH (PAGINADO) ------------------
        public async Task<PagedResult<PesquisaListItemResponse>> ListarAsync(
            PesquisaFiltroRequest filtro,
            CancellationToken ct = default
        )
        {
            int page = Math.Max(1, filtro.Page ?? 1);
            int pageSize = Math.Clamp(filtro.PageSize ?? 20, 1, 200);

            string K(object? v) =>
                v switch
                {
                    null => "_",
                    DateTime d => d.ToString("yyyy-MM-dd"),
                    _ => v.ToString() ?? "_",
                };

            var cacheKey =
                $"{CACHE_LIST_PREFIX}"
                + $"{K(filtro.LoginId)}:{K(filtro.PastaId)}:{K(filtro.TipoPesquisaId)}:{K(filtro.Busca)}:"
                + $"{K(filtro.IsInterativa)}:{K(filtro.Ativa)}:{K(filtro.TemLimitadorTempo)}:"
                + $"{K(filtro.DataInicio)}:{K(filtro.DataFim)}:{page}:{pageSize}";

            return await _cache.GetOrSetAsync<PagedResult<PesquisaListItemResponse>>(
                cacheKey,
                async token =>
                {
                    var query = _ctx.Pesquisas.AsNoTracking().AsQueryable();

                    if (filtro.LoginId.HasValue)
                        query = query.Where(x => x.LoginId == filtro.LoginId.Value);
                    if (filtro.PastaId.HasValue)
                        query = query.Where(x => x.PastaId == filtro.PastaId.Value);
                    if (filtro.TipoPesquisaId.HasValue)
                        query = query.Where(x => x.TipoPesquisaId == filtro.TipoPesquisaId.Value);
                    if (filtro.IsInterativa.HasValue)
                        query = query.Where(x => x.IsInterativa == filtro.IsInterativa.Value);
                    if (filtro.Ativa.HasValue)
                        query = query.Where(x => x.Ativa == filtro.Ativa.Value);
                    if (filtro.TemLimitadorTempo.HasValue)
                        query = query.Where(x =>
                            x.TemLimitadorTempo == filtro.TemLimitadorTempo.Value
                        );
                    if (filtro.DataInicio.HasValue)
                        query = query.Where(x =>
                            (x.DataAtualizacao ?? x.DataCriacao) >= filtro.DataInicio.Value
                        );
                    if (filtro.DataFim.HasValue)
                        query = query.Where(x =>
                            (x.DataAtualizacao ?? x.DataCriacao) < filtro.DataFim.Value
                        );

                    if (!string.IsNullOrWhiteSpace(filtro.Busca))
                    {
                        var b = filtro.Busca.Trim();
                        query = query.Where(x =>
                            EF.Functions.ILike(x.Titulo, $"%{b}%")
                            || (x.Descricao != null && EF.Functions.ILike(x.Descricao, $"%{b}%"))
                        );
                    }

                    var countTask = query.CountAsync(token);
                    var dataTask = query
                        .OrderByDescending(x => x.DataAtualizacao ?? x.DataCriacao)
                        .ThenByDescending(x => x.PesquisaId)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(q => new PesquisaListItemResponse
                        {
                            PesquisaId = q.PesquisaId,
                            Titulo = q.Titulo,
                            Descricao = q.Descricao,
                            LoginId = q.LoginId,
                            TipoPesquisaId = q.TipoPesquisaId,
                            PastaId = q.PastaId,
                            QRCodeUrl = q.QRCodeUrl,
                            DataCriacao = q.DataCriacao,
                            DataAtualizacao = q.DataAtualizacao,
                            TemLimitadorTempo = q.TemLimitadorTempo,
                            DataFechamento = q.DataFechamento,
                            IsInterativa = q.IsInterativa,
                            PermiteRespostasAnonimas = q.PermiteRespostasAnonimas,
                            LimiteRespostas = q.LimiteRespostas,
                            Ativa = q.Ativa,
                        })
                        .ToListAsync(token);

                    await Task.WhenAll(countTask, dataTask);
                    var total = await countTask;
                    var items = await dataTask;

                    return new PagedResult<PesquisaListItemResponse>
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalItems = total,
                        Items = items,
                    };
                },
                absoluteExpiration: TimeSpan.FromMinutes(10),
                ct: ct
            );
        }

        // ------------------ PATCH TEMPLATE ------------------
        public async Task<bool> AtualizarTemplateAsync(
            int id,
            string templateJson,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx
                .Pesquisas.Select(x => new { x.PesquisaId, x.LoginId })
                .FirstOrDefaultAsync(x => x.PesquisaId == id, ct);
            if (entity is null)
                return false;

            if (string.IsNullOrWhiteSpace(templateJson))
                templateJson = "[]";
            try
            {
                JsonDocument.Parse(templateJson);
            }
            catch
            {
                templateJson = "[]";
            }

            // Remove perguntas antigas (soft delete)
            var perguntasAntigas = await _ctx.Perguntas
                .Where(p => p.PesquisaId == id && p.DeletadoEm == null)
                .ToListAsync(ct);

            foreach (var pergunta in perguntasAntigas)
            {
                pergunta.DeletadoEm = DateTime.UtcNow;
            }
            await _ctx.SaveChangesAsync(ct);

            var updated = await _ctx.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE pesquisas 
                   SET templatejson = {templateJson}, dataatualizacao = {DateTime.UtcNow}
                   WHERE pesquisaid = {id}",
                ct
            );

            if (updated > 0)
            {
                // Cria as novas perguntas a partir do templateJson atualizado
                await ParseTemplateAndCreatePerguntas(id, templateJson, ct);
                await InvalidatePesquisaCache(id, entity.LoginId, ct);
            }

            return updated > 0;
        }

        // -------- Extras exigidos pela interface --------

        public async Task<List<PesquisaListItemResponse>> ListarPorLoginAsync(
            int loginid,
            CancellationToken ct = default
        )
        {
            var cacheKey = $"{CACHE_LIST_PREFIX}user:{loginid}";
            return await _cache.GetOrSetAsync<List<PesquisaListItemResponse>>(
                cacheKey,
                async token =>
                {
                    var list = await _ctx
                        .Pesquisas.AsNoTracking()
                        .Where(x => x.LoginId == loginid)
                        .OrderByDescending(x => x.DataAtualizacao ?? x.DataCriacao)
                        .ThenByDescending(x => x.PesquisaId)
                        .Select(q => new PesquisaListItemResponse
                        {
                            PesquisaId = q.PesquisaId,
                            Titulo = q.Titulo,
                            Descricao = q.Descricao,
                            LoginId = q.LoginId,
                            TipoPesquisaId = q.TipoPesquisaId,
                            PastaId = q.PastaId,
                            QRCodeUrl = q.QRCodeUrl,
                            DataCriacao = q.DataCriacao,
                            DataAtualizacao = q.DataAtualizacao,
                            TemLimitadorTempo = q.TemLimitadorTempo,
                            DataFechamento = q.DataFechamento,
                            IsInterativa = q.IsInterativa,
                            PermiteRespostasAnonimas = q.PermiteRespostasAnonimas,
                            LimiteRespostas = q.LimiteRespostas,
                            Ativa = q.Ativa,
                        })
                        .ToListAsync(token);

                    return list;
                },
                absoluteExpiration: TimeSpan.FromMinutes(15),
                ct: ct
            );
        }

        public async Task<bool> DefinirPastaAsync(
            int pesquisaid,
            int? pastaid,
            CancellationToken ct = default
        )
        {
            var entity = await _ctx
                .Pesquisas.Select(x => new { x.PesquisaId, x.LoginId })
                .FirstOrDefaultAsync(x => x.PesquisaId == pesquisaid, ct);
            if (entity is null)
                return false;

            var updated = await _ctx.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE pesquisas 
                   SET pastaid = {pastaid}, dataatualizacao = {DateTime.UtcNow}
                   WHERE pesquisaid = {pesquisaid}",
                ct
            );

            if (updated > 0)
                await InvalidatePesquisaCache(pesquisaid, entity.LoginId, ct);

            return updated > 0;
        }

        private async Task InvalidatePesquisaCache(
            int pesquisaid,
            int loginid,
            CancellationToken ct
        )
        {
            await Task.WhenAll(
                new Task[]
                {
                    _cache.RemoveAsync($"{CACHE_PREFIX}{pesquisaid}", ct),
                    _cache.RemoveAsync($"{CACHE_LIST_PREFIX}user:{loginid}", ct),
                }
            );
        }

        private async Task InvalidateUserCache(int loginid, CancellationToken ct)
        {
            await _cache.RemoveAsync($"{CACHE_LIST_PREFIX}user:{loginid}", ct);
        }

        public async Task<PesquisaResponse?> ObterPorSlugAsync(
            string slug,
            CancellationToken ct = default
        )
        {
            if (int.TryParse(slug, out int pesquisaId))
            {
                var q = await _ctx
                    .Pesquisas.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PesquisaId == pesquisaId, ct);
                if (q is null)
                    return null;

                return new PesquisaResponse
                {
                    PesquisaId = q.PesquisaId,
                    Titulo = q.Titulo,
                    Descricao = q.Descricao,
                    LoginId = q.LoginId,
                    TipoPesquisaId = q.TipoPesquisaId,
                    PastaId = q.PastaId,
                    QRCodeUrl = q.QRCodeUrl,
                    DataCriacao = q.DataCriacao,
                    DataAtualizacao = q.DataAtualizacao,
                    TemLimitadorTempo = q.TemLimitadorTempo,
                    DataFechamento = q.DataFechamento,
                    IsInterativa = q.IsInterativa,
                    PermiteRespostasAnonimas = q.PermiteRespostasAnonimas,
                    LimiteRespostas = q.LimiteRespostas,
                    Ativa = q.Ativa,
                    TemplateJson = q.TemplateJson ?? "[]",
                };
            }
            return null;
        }

        public async Task<int> DuplicarAsync(
            int pesquisaId,
            DuplicarPesquisaRequest request,
            CancellationToken ct = default
        )
        {
            var original = await _ctx
                .Pesquisas.Include(p => p.Perguntas)
                .ThenInclude(p => p.OpcoesPergunta)
                .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (original is null)
                return 0;

            var nova = new Pesquisas
            {
                LoginId = original.LoginId,
                TipoPesquisaId = original.TipoPesquisaId,
                PastaId = request.NovaPastaId ?? original.PastaId,
                Titulo = request.NovoTitulo ?? $"{original.Titulo} (Cópia)",
                Descricao = original.Descricao,
                TemplateJson = original.TemplateJson,
                TemLimitadorTempo = original.TemLimitadorTempo,
                DataFechamento = original.DataFechamento,
                IsInterativa = original.IsInterativa,
                PermiteRespostasAnonimas = original.PermiteRespostasAnonimas,
                LimiteRespostas = original.LimiteRespostas,
                Ativa = true,
                DataCriacao = DateTime.UtcNow,
            };

            _ctx.Pesquisas.Add(nova);
            await _ctx.SaveChangesAsync(ct);

            if (!request.IncluirRespostas)
            {
                foreach (var pergunta in original.Perguntas)
                {
                    var novaPergunta = new Perguntas
                    {
                        PesquisaId = nova.PesquisaId,
                        TipoPerguntaId = pergunta.TipoPerguntaId,
                        Texto = pergunta.Texto,
                        Ordem = pergunta.Ordem,
                        TemGabarito = pergunta.TemGabarito,
                    };
                    _ctx.Perguntas.Add(novaPergunta);
                    await _ctx.SaveChangesAsync(ct);

                    foreach (var opcao in pergunta.OpcoesPergunta)
                    {
                        _ctx.OpcoesPergunta.Add(
                            new OpcoesPergunta
                            {
                                PerguntaId = novaPergunta.PerguntaId,
                                Texto = opcao.Texto,
                                Ordem = opcao.Ordem,
                                Correta = opcao.Correta,
                            }
                        );
                    }
                }
                await _ctx.SaveChangesAsync(ct);
            }

            await InvalidateUserCache(original.LoginId, ct);
            return nova.PesquisaId;
        }

        public async Task<object?> GerarQRCodeAsync(int pesquisaId, CancellationToken ct = default)
        {
            var pesquisa = await _ctx
                .Pesquisas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (pesquisa is null)
                return null;

            // Ajuste a base URL conforme o ambiente:
            var url = $"http://localhost:3000/responder/{pesquisaId}";
            return new
            {
                url,
                slug = pesquisaId.ToString(),
                expiracao = pesquisa.DataFechamento,
            };
        }

        public async Task<byte[]?> ExportarPDFAsync(int pesquisaId, CancellationToken ct = default)
        {
            var pesquisa = await _ctx
                .Pesquisas.Include(p => p.Perguntas)
                .ThenInclude(p => p.OpcoesPergunta)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);
            if (pesquisa is null)
                return null;

            // TODO: integrar biblioteca de PDF
            return Array.Empty<byte>();
        }

        public async Task<object?> ValidarAcessoAsync(
            int pesquisaId,
            ValidarPesquisaRequest request,
            CancellationToken ct = default
        )
        {
            var p = await _ctx
                .Pesquisas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PesquisaId == pesquisaId, ct);
            if (p is null)
                return null;

            var expirada =
                p.TemLimitadorTempo
                && p.DataFechamento.HasValue
                && p.DataFechamento.Value < DateTime.UtcNow;
            return new
            {
                pesquisaId = p.PesquisaId,
                ativa = p.Ativa,
                expirada,
                dataExpiracao = p.DataFechamento,
                permiteAnonimo = p.PermiteRespostasAnonimas,
                requerIdentificacao = false, // ajuste se tiver a coluna
            };
        }

        public async Task<object?> ObterEstatisticasAsync(
            int pesquisaId,
            EstatisticasPesquisaRequest request,
            CancellationToken ct = default
        )
        {
            var p = await _ctx
                .Pesquisas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PesquisaId == pesquisaId, ct);
            if (p is null)
                return null;

            // TODO: implementar contagens reais
            return new
            {
                pesquisaId = p.PesquisaId,
                titulo = p.Titulo,
                totalRespostas = 0,
                taxaConclusao = 0.0,
                tempoMedio = TimeSpan.Zero,
            };
        }

        public async Task<StatusPesquisaResponse?> ObterStatusAsync(
            int pesquisaId,
            CancellationToken ct = default
        )
        {
            var p = await _ctx
                .Pesquisas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PesquisaId == pesquisaId, ct);
            if (p is null)
                return null;

            var expirada =
                p.TemLimitadorTempo
                && p.DataFechamento.HasValue
                && p.DataFechamento.Value < DateTime.UtcNow;

            return new StatusPesquisaResponse
            {
                PesquisaId = p.PesquisaId,
                Titulo = p.Titulo,
                Ativa = p.Ativa,
                Expirada = expirada,
                DataExpiracao = p.DataFechamento,
                TotalRespostas = 0,
                LimiteRespostas = p.LimiteRespostas,
                LimiteAtingido = false,
                RequerIdentificacao = false,
                Instrucoes = null,
                PermiteRespostasAnonimas = p.PermiteRespostasAnonimas,
            };
        }

        public async Task<object> ListarTodasPesquisasAsync(CancellationToken ct = default)
        {
            var pesquisas = await _ctx
                .Pesquisas.AsNoTracking()
                .OrderByDescending(p => p.DataCriacao)
                .Select(p => new
                {
                    p.PesquisaId,
                    p.Titulo,
                    p.LoginId,
                    p.DataCriacao,
                    p.Ativa,
                })
                .ToListAsync(ct);

            return pesquisas;
        }

        public async Task<object> ObterEstatisticasGeraisAsync(CancellationToken ct = default)
        {
            var totalPesquisas = await _ctx.Pesquisas.CountAsync(ct);
            var totalUsuarios = await _ctx.Login.CountAsync(ct);
            var pesquisasAtivas = await _ctx.Pesquisas.CountAsync(p => p.Ativa, ct);

            return new
            {
                totalPesquisas,
                totalUsuarios,
                pesquisasAtivas,
                dataGeracao = DateTime.UtcNow,
            };
        }

        /// <summary>
        /// Reprocessa as perguntas de uma pesquisa existente a partir do TemplateJson
        /// </summary>
        public async Task<bool> ReprocessarPerguntasAsync(int pesquisaId, CancellationToken ct = default)
        {
            try
            {
                Console.WriteLine($"🔄 Reprocessando perguntas da pesquisa {pesquisaId}");

                var pesquisa = await _ctx.Pesquisas
                    .FirstOrDefaultAsync(p => p.PesquisaId == pesquisaId, ct);

                if (pesquisa == null)
                {
                    Console.WriteLine($"❌ Pesquisa {pesquisaId} não encontrada");
                    return false;
                }

                // Marca perguntas existentes como deletadas (soft delete)
                var perguntasAntigas = await _ctx.Perguntas
                    .Where(p => p.PesquisaId == pesquisaId && p.DeletadoEm == null)
                    .ToListAsync(ct);

                Console.WriteLine($"🗑️ Marcando {perguntasAntigas.Count} perguntas antigas como deletadas");

                foreach (var pergunta in perguntasAntigas)
                {
                    pergunta.DeletadoEm = DateTime.UtcNow;
                }
                await _ctx.SaveChangesAsync(ct);

                // Parse e cria novas perguntas
                await ParseTemplateAndCreatePerguntas(pesquisaId, pesquisa.TemplateJson ?? "[]", ct);

                Console.WriteLine($"✅ Reprocessamento concluído para pesquisa {pesquisaId}");
                await InvalidatePesquisaCache(pesquisaId, pesquisa.LoginId, ct);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao reprocessar perguntas: {ex.Message}");
                return false;
            }
        }
    }
}
