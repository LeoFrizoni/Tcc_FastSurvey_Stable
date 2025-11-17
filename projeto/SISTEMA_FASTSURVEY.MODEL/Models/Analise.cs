using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Models
{
    public partial class Analise
    {
        public int id { get; set; }
        public int respostaId { get; set; }
        public JsonDocument analyics { get; set; }
        public JsonDocument keywords { get; set; }
    }
}
