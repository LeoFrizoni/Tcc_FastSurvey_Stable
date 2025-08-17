import styles from "./ModalPreviewPesquisa.module.css";
import CabecalhoPesquisa from "./CabecalhoPesquisa";
import InformacoesPesquisa from "./InformacoesPesquisa";

function GaleriaPreview({ imagens }) {
  if (!Array.isArray(imagens) || imagens.length === 0) return null;
  return (
    <div style={{ display: "flex", gap: 12, flexWrap: "wrap", margin: "10px 0 12px 0" }}>
      {imagens.map((img, i) => {
        // No preview (dentro do Criar), costumamos ter previewUrl
        const src = img.previewUrl;
        const alt = img.file?.name || img.nome || `imagem-${i}`;
        if (!src) return (
          <span key={i} style={{
            padding: "6px 10px", border: "1px solid #e5e7eb", borderRadius: 8,
            fontSize: 12, background: "#fafafa"
          }}>
            {alt}
          </span>
        );
        return (
          <div key={i} style={{
            position: "relative", width: 220, height: 160, borderRadius: 10,
            overflow: "hidden", border: "1px solid #e5e7eb", background: "#fff",
            boxShadow: "0 4px 10px rgba(0,0,0,.04)"
          }}>
            <img src={src} alt={alt} style={{ width: "100%", height: "100%", objectFit: "cover" }} />
          </div>
        );
      })}
    </div>
  );
}

const DiscursivaPreview = ({ bloco }) => (
  <div
    className="bloco-pergunta"
    style={{
      backgroundColor: bloco.estilo?.corFundo || "#fff",
      color: bloco.estilo?.corTexto || "inherit",
      fontFamily: bloco.estilo?.fonte || "inherit",
      marginBottom: "1.2rem"
    }}
  >
    <div className="cabecalho-pergunta"><h4>Pergunta Discursiva</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{bloco.texto}</div>

    {/* IMAGENS ENTRE INPUT E RESPOSTA */}
    <GaleriaPreview imagens={bloco.imagens} />

    <textarea
      disabled
      placeholder="Resposta do usuário..."
      className="resposta-simulada"
      style={{ width: "100%", minHeight: 48 }}
    />
  </div>
);

const MultiplaEscolhaPreview = ({ bloco }) => (
  <div
    className="bloco-pergunta"
    style={{
      backgroundColor: bloco.estilo?.corFundo || "#fff",
      color: bloco.estilo?.corTexto || "inherit",
      fontFamily: bloco.estilo?.fonte || "inherit",
      marginBottom: "1.2rem"
    }}
  >
    <div className="cabecalho-pergunta"><h4>Pergunta Múltipla Escolha</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{bloco.texto}</div>

    <GaleriaPreview imagens={bloco.imagens} />

    {bloco.opcoes?.map((opcao, idx) => (
      <div key={idx} className="opcao-input" style={{ display: "flex", alignItems: "center", marginBottom: 4 }}>
        <input type="checkbox" disabled style={{ marginRight: 8 }} />
        <span>{typeof opcao === 'object' ? (opcao.texto ?? opcao.opcao ?? '') : opcao}</span>
      </div>
    ))}
  </div>
);

const ObjetivaPreview = ({ bloco }) => (
  <div
    className="bloco-pergunta"
    style={{
      backgroundColor: bloco.estilo?.corFundo || "#fff",
      color: bloco.estilo?.corTexto || "inherit",
      fontFamily: bloco.estilo?.fonte || "inherit",
      marginBottom: "1.2rem"
    }}
  >
    <div className="cabecalho-pergunta"><h4>Pergunta Objetiva (única)</h4></div>
    <div className="input-pergunta" style={{ marginBottom: 8 }}>{bloco.texto}</div>

    <GaleriaPreview imagens={bloco.imagens} />

    {bloco.opcoes?.map((opcao, idx) => (
      <div key={idx} className="opcao-input" style={{ display: "flex", alignItems: "center", marginBottom: 4 }}>
        <input type="radio" disabled style={{ marginRight: 8 }} />
        <span>{typeof opcao === 'object' ? (opcao.texto ?? opcao.opcao ?? '') : opcao}</span>
      </div>
    ))}
  </div>
);

const AnexoPreview = ({ bloco }) => (
  <div
    className="bloco-pergunta"
    style={{
      backgroundColor: bloco.estilo?.corFundo || "#fff",
      color: bloco.estilo?.corTexto || "inherit",
      fontFamily: bloco.estilo?.fonte || "inherit",
      marginBottom: "1.2rem"
    }}
  >
    <div className="cabecalho-pergunta"><h4>Arquivo Anexo</h4></div>
    <div><strong>Arquivo:</strong> {bloco.arquivo?.name || "Nenhum arquivo selecionado"}</div>
  </div>
);

const ModalPreviewPesquisa = ({ isOpen, onClose, dadosPesquisa, blocos, nomeTipoPesquisa, autorNome }) => {
  if (!isOpen) return null;
  return (
    <div className={styles.modalOverlay}>
      <div className={styles.modalContent}>
        <button className={styles.closeBtn} onClick={onClose}>X</button>

        <CabecalhoPesquisa
          autor={autorNome || "—"}
          data={(() => {
            if (dadosPesquisa && dadosPesquisa.dataCriacao) return dadosPesquisa.dataCriacao;
            const hoje = new Date();
            return hoje.toLocaleDateString('pt-BR');
          })()}
          selecionado={false}
        />

        <InformacoesPesquisa
          titulo={dadosPesquisa.titulo}
          descricao={dadosPesquisa.descricao}
          tipoPesquisa={nomeTipoPesquisa}
        />

        <div style={{ marginTop: 24 }}>
          {blocos.length === 0 ? (
            <p style={{ textAlign: "center", color: "#888" }}>Nenhum bloco adicionado.</p>
          ) : (
            blocos.map((bloco, idx) => {
              switch (bloco.tipo) {
                case "discursiva": return <DiscursivaPreview key={bloco.id || idx} bloco={bloco} />;
                case "multipla":   return <MultiplaEscolhaPreview key={bloco.id || idx} bloco={bloco} />;
                case "objetiva":   return <ObjetivaPreview key={bloco.id || idx} bloco={bloco} />;
                case "anexo":      return <AnexoPreview key={bloco.id || idx} bloco={bloco} />;
                default:           return null;
              }
            })
          )}
        </div>
      </div>
    </div>
  );
};

export default ModalPreviewPesquisa;
