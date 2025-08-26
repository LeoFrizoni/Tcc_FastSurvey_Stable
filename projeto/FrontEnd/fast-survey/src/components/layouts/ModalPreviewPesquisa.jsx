import styles from "./ModalPreviewPesquisa.module.css";
import CabecalhoPesquisa from "./CabecalhoPesquisa";
import InformacoesPesquisa from "./InformacoesPesquisa";

/** Util: traduz TemplateJson do bloco para style inline */
function applyBlockStyle(estilo = {}) {
  const {
    corFundo,
    corTexto,
    fonte,
    padding,           // ex.: "12px 16px"
    largura,           // ex.: "100%" | 640
    alinhamento,       // "left" | "center" | "right" | "justify"
    borda,             // ex.: "1px solid #e5e7eb"
    sombra,            // ex.: "0 2px 10px rgba(0,0,0,.06)"
    bordaRadius,       // ex.: 10 | "10px"
    margemInferior,    // ex.: 12
  } = estilo;

  const style = {};
  if (corFundo) style.backgroundColor = corFundo;
  if (corTexto) style.color = corTexto;
  if (fonte) style.fontFamily = fonte;
  if (padding) style.padding = padding;
  if (largura != null) style.width = typeof largura === "number" ? `${largura}px` : largura;
  if (alinhamento) style.textAlign = alinhamento;
  if (borda) style.border = borda;
  if (sombra) style.boxShadow = sombra;
  if (bordaRadius != null) style.borderRadius = typeof bordaRadius === "number" ? `${bordaRadius}px` : bordaRadius;
  style.marginBottom = margemInferior != null ? (typeof margemInferior === "number" ? `${margemInferior}px` : margemInferior) : "1.2rem";
  return style;
}

function safeText(v, fallback = "—") {
  const t = typeof v === "string" ? v.trim() : v;
  if (t === null || t === undefined || t === "") return fallback;
  return t;
}

function pickImgSrc(img) {
  // ordem de preferência no preview
  return img?.previewUrl || img?.url || img?.base64 || null;
}

function pickImgAlt(img, i) {
  return img?.file?.name || img?.nome || img?.alt || `imagem-${i}`;
}

function GaleriaPreview({ imagens }) {
  if (!Array.isArray(imagens) || imagens.length === 0) return null;
  return (
    <div style={{ display: "flex", gap: 12, flexWrap: "wrap", margin: "10px 0 12px 0" }}>
      {imagens.map((img, i) => {
        const src = pickImgSrc(img);
        const alt = pickImgAlt(img, i);

        if (!src) {
          return (
            <span key={i} style={{
              padding: "6px 10px",
              border: "1px solid #e5e7eb",
              borderRadius: 8,
              fontSize: 12,
              background: "#fafafa",
              color: "#666"
            }}>
              {alt}
            </span>
          );
        }

        return (
          <div
            key={i}
            style={{
              position: "relative",
              width: 220,
              height: 160,
              borderRadius: 10,
              overflow: "hidden",
              border: "1px solid #e5e7eb",
              background: "#fff",
              boxShadow: "0 4px 10px rgba(0,0,0,.04)"
            }}
          >
            <img src={src} alt={alt} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
          </div>
        );
      })}
    </div>
  );
}

const DiscursivaPreview = ({ bloco }) => (
  <div className="bloco-pergunta" style={applyBlockStyle(bloco?.estilo)}>
    <div className="cabecalho-pergunta"><h4>Pergunta Discursiva</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{safeText(bloco?.texto, "Sem enunciado")}</div>

    <GaleriaPreview imagens={bloco?.imagens} />

    <textarea
      disabled
      placeholder="Resposta do usuário..."
      className="resposta-simulada"
      style={{ width: "100%", minHeight: 48 }}
    />
  </div>
);

const MultiplaEscolhaPreview = ({ bloco }) => (
  <div className="bloco-pergunta" style={applyBlockStyle(bloco?.estilo)}>
    <div className="cabecalho-pergunta"><h4>Pergunta Múltipla Escolha</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{safeText(bloco?.texto, "Sem enunciado")}</div>

    <GaleriaPreview imagens={bloco?.imagens} />

    {(bloco?.opcoes || []).map((opcao, idx) => {
      const label = typeof opcao === "object" ? (opcao?.texto ?? opcao?.opcao ?? "") : opcao;
      return (
        <div key={idx} className="opcao-input" style={{ display: "flex", alignItems: "center", marginBottom: 4 }}>
          <input type="checkbox" disabled style={{ marginRight: 8 }} />
          <span>{safeText(label, "—")}</span>
        </div>
      );
    })}
  </div>
);

const ObjetivaPreview = ({ bloco }) => (
  <div className="bloco-pergunta" style={applyBlockStyle(bloco?.estilo)}>
    <div className="cabecalho-pergunta"><h4>Pergunta Objetiva (única)</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{safeText(bloco?.texto, "Sem enunciado")}</div>

    <GaleriaPreview imagens={bloco?.imagens} />

    {(bloco?.opcoes || []).map((opcao, idx) => {
      const label = typeof opcao === "object" ? (opcao?.texto ?? opcao?.opcao ?? "") : opcao;
      return (
        <div key={idx} className="opcao-input" style={{ display: "flex", alignItems: "center", marginBottom: 4 }}>
          <input type="radio" disabled style={{ marginRight: 8 }} />
          <span>{safeText(label, "—")}</span>
        </div>
      );
    })}
  </div>
);

const AnexoPreview = ({ bloco }) => (
  <div className="bloco-pergunta" style={applyBlockStyle(bloco?.estilo)}>
    <div className="cabecalho-pergunta"><h4>Arquivo Anexo</h4></div>
    <div><strong>Arquivo:</strong> {bloco?.arquivo?.name || "Nenhum arquivo selecionado"}</div>
  </div>
);

function normalizaTipo(tipo) {
  const t = (tipo || "").toString().toLowerCase();
  if (t.startsWith("disc")) return "discursiva";
  if (t.startsWith("mult")) return "multipla";
  if (t.startsWith("obj")) return "objetiva";
  if (t.startsWith("anex")) return "anexo";
  return t; // manter se já vier correto
}

const ModalPreviewPesquisa = ({ isOpen, onClose, dadosPesquisa, blocos = [], nomeTipoPesquisa, autorNome }) => {
  if (!isOpen) return null;

  const dataCriacao = (() => {
    if (dadosPesquisa?.DataCriacao) return dadosPesquisa.DataCriacao;
    if (dadosPesquisa?.dataCriacao) return dadosPesquisa.dataCriacao;
    const hoje = new Date();
    return hoje.toISOString(); // CabecalhoPesquisa formatará
  })();

  return (
    <div className={styles.modalOverlay}>
      <div className={styles.modalContent} role="dialog" aria-modal="true">
        <button className={styles.closeBtn} onClick={onClose} aria-label="Fechar">X</button>

        <CabecalhoPesquisa
          autor={autorNome || "—"}
          data={dataCriacao}
          selecionado={false}
        />

        <InformacoesPesquisa
          titulo={dadosPesquisa?.Titulo ?? dadosPesquisa?.titulo}
          descricao={dadosPesquisa?.Descricao ?? dadosPesquisa?.descricao}
          tipoPesquisa={nomeTipoPesquisa}
        />

        <div style={{ marginTop: 24 }}>
          {blocos.length === 0 ? (
            <p style={{ textAlign: "center", color: "#888" }}>Nenhum bloco adicionado.</p>
          ) : (
            blocos.map((bloco, idx) => {
              const tipo = normalizaTipo(bloco?.tipo);
              const key = bloco?.id ?? idx;
              if (tipo === "discursiva") return <DiscursivaPreview key={key} bloco={bloco} />;
              if (tipo === "multipla")   return <MultiplaEscolhaPreview key={key} bloco={bloco} />;
              if (tipo === "objetiva")   return <ObjetivaPreview key={key} bloco={bloco} />;
              if (tipo === "anexo")      return <AnexoPreview key={key} bloco={bloco} />;
              return null;
            })
          )}
        </div>
      </div>
    </div>
  );
};

export default ModalPreviewPesquisa;
