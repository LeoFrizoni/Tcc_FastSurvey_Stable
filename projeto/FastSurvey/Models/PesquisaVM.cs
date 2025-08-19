// FASTSURVEY/Models/PesquisaVM.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FASTSURVEY.Models
{
    public class PesquisaVM
    {
        /// <summary>Usado no PUT (id da pesquisa).</summary>
        public int CodigoPesquisa { get; set; }

        [Required, StringLength(100)]
        public string? Titulo { get; set; }

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int TipoPesquisaId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int LoginId { get; set; }

        /// <summary>Pasta opcional onde a pesquisa está.</summary>
        public int? PastaId { get; set; }

        /// <summary>JSON do template de blocos.</summary>
        public string? TemplateJson { get; set; }

        // ---- Somente leitura (preenchidos ao retornar para o front) ----
        public string? Autor { get; set; }           // mapeia de login.usuario
        public DateTime? DataCriacao { get; set; }   // mapeia de pesquisas.datacriacao

        // ---- Estruturas opcionais para criação em lote (se você usar) ----
        public List<PerguntaDiscursivaDto>? PerguntasDiscursivas { get; set; }
        public List<PerguntaObjetivaDto>? PerguntasObjetivas { get; set; }
        public List<PerguntaMultiplaEscolhaDto>? PerguntasMultiplaEscolha { get; set; }
    }
}
