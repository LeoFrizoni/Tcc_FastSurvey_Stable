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
  
  // Se há apenas uma imagem, centralizar
  const isSingleImage = imagens.length === 1;
  
  return (
    <div style={{ 
      display: "flex", 
      gap: 12, 
      flexWrap: "wrap", 
      margin: "10px 0 12px 0",
      justifyContent: isSingleImage ? "center" : "flex-start"
    }}>
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

const PaperclipIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="m21.44 11.05-9.19 9.19a6 6 0 0 1-8.49-8.49l8.57-8.57A4 4 0 1 1 18 8.84l-8.59 8.57a2 2 0 0 1-2.83-2.83l8.49-8.48"/>
  </svg>
);

const DiscursivaPreview = ({ bloco }) => (
  <div className="bloco-pergunta" style={applyBlockStyle(bloco?.estilo)}>
    <div className="cabecalho-pergunta"><h4>Pergunta Discursiva</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{safeText(bloco?.texto, "Sem enunciado")}</div>

    <GaleriaPreview imagens={bloco?.imagens} />

    {/* Exemplo de resposta (para futura IA) */}
    <div style={{ marginTop: 16 }}>
      <div style={{ marginBottom: 12 }}>
        <label style={{ display: "block", marginBottom: 4, fontWeight: 500, color: "#374151" }}>
          Exemplo de resposta (para IA no futuro)
        </label>
        <textarea
          value={bloco?.respostaExemplo || ""}
          disabled
          style={{
            width: "100%",
            minHeight: 80,
            padding: 12,
            border: "1px solid #d1d5db",
            borderRadius: 6,
            fontFamily: "inherit",
            fontSize: 14,
            resize: "vertical",
            background: "#f9fafb"
          }}
          placeholder="Escreva aqui um exemplo de boa resposta..."
        />
      </div>

      <div>
        <label style={{ display: "block", marginBottom: 4, fontWeight: 500, color: "#374151" }}>
          Resposta do usuário...
        </label>
        <textarea
          disabled
          style={{
            width: "100%",
            minHeight: 80,
            padding: 12,
            border: "1px solid #d1d5db",
            borderRadius: 6,
            fontFamily: "inherit",
            fontSize: 14,
            resize: "vertical",
            background: "#f9fafb"
          }}
          placeholder="Resposta do usuário..."
        />
      </div>
    </div>

    {/* Anexos da Pergunta */}
    {Array.isArray(bloco?.attachments) && bloco.attachments.length > 0 && (
      <div style={{ marginTop: "16px", padding: "16px", background: "white", border: "1px solid #e2e8f0", borderRadius: "8px" }}>
        <h4 style={{ margin: "0 0 12px 0", fontSize: "14px", fontWeight: "600", color: "#374151" }}>Anexos da Pergunta</h4>
        <ul style={{ listStyle: "none", padding: 0, margin: 0, display: "flex", flexDirection: "column", gap: "8px" }}>
          {bloco.attachments.map((f, i) => (
            <li key={i} style={{ display: "flex", alignItems: "center", gap: "12px", padding: "12px", background: "white", border: "1px solid #e2e8f0", borderRadius: "8px" }}>
              <span style={{ fontSize: "18px", color: "#7c3aed", flexShrink: 0 }}>
                <PaperclipIcon />
              </span>
              <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: "2px", minWidth: 0 }}>
                <span style={{ fontSize: "14px", fontWeight: "500", color: "#374151", wordBreak: "break-word", lineHeight: "1.4" }}>{f.name}</span>
                <span style={{ fontSize: "12px", color: "#6b7280", fontWeight: "400" }}>
                  {f.size ? `${(f.size / 1024 / 1024).toFixed(2)} MB` : ''}
                </span>
              </div>
            </li>
          ))}
        </ul>
      </div>
    )}
  </div>
);

const MultiplaEscolhaPreview = ({ bloco }) => (
  <div className="bloco-pergunta" style={applyBlockStyle(bloco?.estilo)}>
    <div className="cabecalho-pergunta"><h4>Pergunta Múltipla Escolha</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{safeText(bloco?.texto, "Sem enunciado")}</div>

    <GaleriaPreview imagens={bloco?.imagens} />

    {/* Configurações da pergunta */}
    {(bloco?.TemGabarito || bloco?.permitirMultiplaSelecao) && (
      <div style={{ margin: "16px 0", padding: "12px", background: "#f8f9fa", borderRadius: "8px", border: "1px solid #e9ecef" }}>
        {bloco?.TemGabarito && (
          <div style={{ display: "flex", alignItems: "center", gap: "8px", marginBottom: "8px" }}>
            <div style={{ width: "44px", height: "24px", background: "#7c3aed", borderRadius: "12px", position: "relative" }}>
              <div style={{ position: "absolute", top: "2px", left: "22px", width: "20px", height: "20px", background: "white", borderRadius: "50%", boxShadow: "0 2px 4px rgba(0,0,0,0.2)" }}></div>
            </div>
            <span style={{ fontSize: "14px", fontWeight: "500", color: "#374151" }}>Há gabarito?</span>
          </div>
        )}
        {bloco?.permitirMultiplaSelecao && (
          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
            <div style={{ width: "44px", height: "24px", background: "#7c3aed", borderRadius: "12px", position: "relative" }}>
              <div style={{ position: "absolute", top: "2px", left: "22px", width: "20px", height: "20px", background: "white", borderRadius: "50%", boxShadow: "0 2px 4px rgba(0,0,0,0.2)" }}></div>
            </div>
            <span style={{ fontSize: "14px", fontWeight: "500", color: "#374151" }}>Permitir múltipla seleção</span>
          </div>
        )}
      </div>
    )}

    {(bloco?.opcoes || []).map((opcao, idx) => {
      const label = typeof opcao === "object" ? (opcao?.texto ?? opcao?.opcao ?? "") : opcao;
      const isCorreta = Array.isArray(bloco?.corretas) && bloco.corretas.includes(idx);
      return (
        <div key={idx} className="opcao-input" style={{ display: "flex", alignItems: "center", marginBottom: 4 }}>
          <input type="checkbox" disabled style={{ marginRight: 8 }} />
          <span>{safeText(label, "—")}</span>
          {bloco?.TemGabarito && (
            <div style={{ marginLeft: "auto", display: "flex", alignItems: "center", gap: "8px", padding: "6px 12px", background: isCorreta ? "#f0f9ff" : "#f8f9fa", border: `1px solid ${isCorreta ? "#bae6fd" : "#e9ecef"}`, borderRadius: "8px", fontSize: "13px", color: isCorreta ? "#0369a1" : "#6b7280" }}>
              <input type="checkbox" checked={isCorreta} disabled style={{ margin: 0, accentColor: "#7c3aed" }} />
              <span style={{ fontWeight: "500" }}>Correta</span>
            </div>
          )}
        </div>
      );
    })}

    {/* Anexos da Pergunta */}
    {Array.isArray(bloco?.attachments) && bloco.attachments.length > 0 && (
      <div style={{ marginTop: "16px", padding: "16px", background: "white", border: "1px solid #e2e8f0", borderRadius: "8px" }}>
        <h4 style={{ margin: "0 0 12px 0", fontSize: "14px", fontWeight: "600", color: "#374151" }}>Anexos da Pergunta</h4>
        <ul style={{ listStyle: "none", padding: 0, margin: 0, display: "flex", flexDirection: "column", gap: "8px" }}>
          {bloco.attachments.map((f, i) => (
            <li key={i} style={{ display: "flex", alignItems: "center", gap: "12px", padding: "12px", background: "white", border: "1px solid #e2e8f0", borderRadius: "8px" }}>
              <span style={{ fontSize: "18px", color: "#7c3aed", flexShrink: 0 }}>
                <PaperclipIcon />
              </span>
              <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: "2px", minWidth: 0 }}>
                <span style={{ fontSize: "14px", fontWeight: "500", color: "#374151", wordBreak: "break-word", lineHeight: "1.4" }}>{f.name}</span>
                <span style={{ fontSize: "12px", color: "#6b7280", fontWeight: "400" }}>
                  {f.size ? `${(f.size / 1024 / 1024).toFixed(2)} MB` : ''}
                </span>
              </div>
            </li>
          ))}
        </ul>
      </div>
    )}
  </div>
);

const ObjetivaPreview = ({ bloco }) => (
  <div className="bloco-pergunta" style={applyBlockStyle(bloco?.estilo)}>
    <div className="cabecalho-pergunta"><h4>Pergunta Objetiva (única)</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{safeText(bloco?.texto, "Sem enunciado")}</div>

    <GaleriaPreview imagens={bloco?.imagens} />

    {/* Configurações da pergunta */}
    {bloco?.TemGabarito && (
      <div style={{ margin: "16px 0", padding: "12px", background: "#f8f9fa", borderRadius: "8px", border: "1px solid #e9ecef" }}>
        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
          <div style={{ width: "44px", height: "24px", background: "#7c3aed", borderRadius: "12px", position: "relative" }}>
            <div style={{ position: "absolute", top: "2px", left: "22px", width: "20px", height: "20px", background: "white", borderRadius: "50%", boxShadow: "0 2px 4px rgba(0,0,0,0.2)" }}></div>
          </div>
          <span style={{ fontSize: "14px", fontWeight: "500", color: "#374151" }}>Há gabarito?</span>
        </div>
      </div>
    )}

    {(bloco?.opcoes || []).map((opcao, idx) => {
      const label = typeof opcao === "object" ? (opcao?.texto ?? opcao?.opcao ?? "") : opcao;
      const isCorreta = bloco?.corretaIndex === idx;
      return (
        <div key={idx} className="opcao-input" style={{ display: "flex", alignItems: "center", marginBottom: 4 }}>
          <input type="radio" disabled style={{ marginRight: 8 }} />
          <span>{safeText(label, "—")}</span>
          {bloco?.TemGabarito && (
            <div style={{ marginLeft: "auto", display: "flex", alignItems: "center", gap: "8px", padding: "6px 12px", background: isCorreta ? "#f0f9ff" : "#f8f9fa", border: `1px solid ${isCorreta ? "#bae6fd" : "#e9ecef"}`, borderRadius: "8px", fontSize: "13px", color: isCorreta ? "#0369a1" : "#6b7280" }}>
              <input type="radio" checked={isCorreta} disabled style={{ margin: 0, accentColor: "#7c3aed" }} />
              <span style={{ fontWeight: "500" }}>Correta</span>
            </div>
          )}
        </div>
      );
    })}

    {/* Anexos da Pergunta */}
    {Array.isArray(bloco?.attachments) && bloco.attachments.length > 0 && (
      <div style={{ marginTop: "16px", padding: "16px", background: "white", border: "1px solid #e2e8f0", borderRadius: "8px" }}>
        <h4 style={{ margin: "0 0 12px 0", fontSize: "14px", fontWeight: "600", color: "#374151" }}>Anexos da Pergunta</h4>
        <ul style={{ listStyle: "none", padding: 0, margin: 0, display: "flex", flexDirection: "column", gap: "8px" }}>
          {bloco.attachments.map((f, i) => (
            <li key={i} style={{ display: "flex", alignItems: "center", gap: "12px", padding: "12px", background: "white", border: "1px solid #e2e8f0", borderRadius: "8px" }}>
              <span style={{ fontSize: "18px", color: "#7c3aed", flexShrink: 0 }}>
                <PaperclipIcon />
              </span>
              <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: "2px", minWidth: 0 }}>
                <span style={{ fontSize: "14px", fontWeight: "500", color: "#374151", wordBreak: "break-word", lineHeight: "1.4" }}>{f.name}</span>
                <span style={{ fontSize: "12px", color: "#6b7280", fontWeight: "400" }}>
                  {f.size ? `${(f.size / 1024 / 1024).toFixed(2)} MB` : ''}
                </span>
              </div>
            </li>
          ))}
        </ul>
      </div>
    )}
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
