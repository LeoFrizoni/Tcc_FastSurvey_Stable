import React, { useMemo, useCallback } from "react";
import { Trash2, Plus, Paperclip } from "lucide-react";
import styles from "./pergunta.module.css";

function applyBlockStyle(estilo = {}, override = {}) {
  const { corFundo, corTexto, fonte, padding, largura, alinhamento, borda, sombra, bordaRadius, margemInferior } = estilo || {};
  const s = {};
  if (corFundo) s.backgroundColor = corFundo;
  if (corTexto) s.color = corTexto;
  if (fonte) s.fontFamily = fonte;
  if (padding) s.padding = padding;
  if (largura != null) s.width = typeof largura === "number" ? `${largura}px` : largura;
  if (alinhamento) s.textAlign = alinhamento;
  if (borda) s.border = borda;
  if (sombra) s.boxShadow = sombra;
  if (bordaRadius != null) s.borderRadius = typeof bordaRadius === "number" ? `${bordaRadius}px` : bordaRadius;
  s.marginBottom = margemInferior != null ? (typeof margemInferior === "number" ? `${margemInferior}px` : margemInferior) : undefined;
  return { ...s, ...override };
}
function pickImgSrc(img) { return img?.previewUrl || img?.url || img?.base64 || null; }
function pickImgAlt(img, i) { return img?.file?.name || img?.nome || img?.alt || `imagem-${i}`; }
function getOpcaoTexto(opcao) {
  return typeof opcao === "object" && opcao !== null ? (opcao.texto ?? opcao.opcao ?? "") : (opcao ?? "");
}

/** Mantém foco no input quando clica no “fundo” do card */
function handleMouseDownContainer(e) {
  const isEditable = e.target.closest('input, textarea, select, [contenteditable="true"]');
  if (!isEditable) e.preventDefault();
}

const MultiplaEscolhaBase = ({
  bloco,
  onChangeTexto,
  onChangeOpcoes,
  onChangeBloco,
  onRemoveImagem,
  style,
  onClick,
  onToggleGabarito,
  onTogglePermiteMultipla,
  onToggleCorreta,
}) => {
  const b = bloco || {};
  const opcoes = Array.isArray(b.opcoes) ? b.opcoes : [];
  const imagens = Array.isArray(b.imagens) ? b.imagens : [];
  const unica = imagens.length === 1;

  const estiloContainer = useMemo(
    () =>
      applyBlockStyle(b.estilo, {
        backgroundColor: style?.backgroundColor || undefined,
        fontFamily: style?.fontFamily || undefined,
        color: style?.color || undefined,
      }),
    [b.estilo, style]
  );



  const isCorreta = useCallback((idx) => {
    const result = Array.isArray(b.corretas) && b.corretas.includes(idx);
    return result;
  }, [b.corretas]);

  const handleToggleCorreta = useCallback((index) => {
    onToggleCorreta(b.id, index);
  }, [onToggleCorreta, b.id]);

  const handleAdicionarOpcao = useCallback(() => {
    onChangeOpcoes(b.id, [...opcoes, ""]);
  }, [onChangeOpcoes, b.id, opcoes]);

  const handleRemoverOpcao = useCallback((index) => {
    const novas = opcoes.filter((_, i) => i !== index);
    onChangeOpcoes(b.id, novas);
  }, [opcoes, onChangeOpcoes, b.id]);

  const handleAtualizarOpcao = useCallback((index, value) => {
    const novas = [...opcoes];
    if (typeof novas[index] === "object" && novas[index] !== null) {
      novas[index] = { ...novas[index], texto: value };
    } else {
      novas[index] = value;
    }
    onChangeOpcoes(b.id, novas);
  }, [opcoes, onChangeOpcoes, b.id]);

  return (
    <div
      className={`${styles["bloco-pergunta"]} ${b.selecionado ? styles.selecionado : ""}`}
      style={estiloContainer}
      onMouseDown={handleMouseDownContainer}
      onClick={(e) => {
        e.stopPropagation();
        onClick?.();
      }}
    >
      <input
        type="text"
        value={b.texto ?? ""}
        onChange={(e) => onChangeTexto(b.id, e.target.value)}
        placeholder="Digite sua pergunta aqui..."
        className={styles["input-pergunta"]}
        onClick={(e) => e.stopPropagation()}
      />

      {/* PREVIEW ENTRE INPUT E OPÇÕES */}
      {imagens.length > 0 && (
        <div className={`${styles["galeria-imagens"]} ${unica ? styles.centralizada : ""}`}>
          {imagens.map((img, i) => {
            const src = pickImgSrc(img);
            const alt = pickImgAlt(img, i);
            return (
              <div key={i} className={`${styles["thumb-imagem"]} ${unica ? styles.grande : ""}`}>
                {src ? <img src={src} alt={alt} /> : <span className={styles["thumb-placeholder"]}>{alt}</span>}
                <button
                  type="button"
                  className={styles["btn-remover-img"]}
                  title="Remover imagem"
                  onClick={(e) => {
                    e.stopPropagation();
                    onRemoveImagem(b.id, i);
                  }}
                  aria-label="Remover imagem"
                >
                  <Trash2 size={18} />
                </button>
              </div>
            );
          })}
        </div>
      )}

      {/* Configurações da pergunta */}
              <div className={styles["grupo-config"]} onClick={(e) => e.stopPropagation()}>
        <label className={styles.switch}>
          <input
            type="checkbox"
            checked={!!b.TemGabarito}
            onChange={(e) => onToggleGabarito(b.id, e.target.checked)}
          />
          <span>Há gabarito?</span>
        </label>

        <label className={styles.switch}>
          <input
            type="checkbox"
            checked={!!b.permitirMultiplaSelecao}
            onChange={(e) => onTogglePermiteMultipla(b.id, e.target.checked)}
          />
          <span>Permitir múltipla seleção</span>
        </label>
      </div>

      {opcoes.map((opcao, index) => {
        const valor = getOpcaoTexto(opcao);
        const simulador = b.permitirMultiplaSelecao ? (
          <input type="checkbox" disabled className={styles["checkbox-simulador"]} />
        ) : (
                      <input type="radio" disabled className={styles["radio-simulador"]} />
        );

        return (
          <div key={index} className={styles["opcao-input"]} onClick={(e) => e.stopPropagation()}>
            {simulador}

            <input
              type="text"
              value={valor}
              onChange={(e) => handleAtualizarOpcao(index, e.target.value)}
              placeholder={`Opção ${index + 1}`}
              className={styles["input-opcao"]}
            />

            {b.TemGabarito && (
              <div 
                className={styles["marcar-correta"]}
                onClick={(e) => {
                  e.stopPropagation();
                  handleToggleCorreta(index);
                }}
              >
                <input
                  type={b.permitirMultiplaSelecao ? "checkbox" : "radio"}
                  name={`gabarito-multipla-${b.id}`}
                  checked={isCorreta(index)}
                  onChange={(e) => {
                    e.stopPropagation();
                    handleToggleCorreta(index);
                  }}
                />
                <span>Correta</span>
              </div>
            )}

            <button
              type="button"
              className={styles["btn-remover"]}
              onClick={() => handleRemoverOpcao(index)}
              title="Remover opção"
              aria-label={`Remover opção ${index + 1}`}
            >
              <Trash2 size={16} />
            </button>
          </div>
        );
      })}

      <button
        type="button"
        className={styles["adicionar-opcao-btn"]}
        onClick={(e) => {
          e.stopPropagation();
          handleAdicionarOpcao();
        }}
      >
        <Plus size={16} /> Adicionar Opção
      </button>

      {/* Anexos da Pergunta */}
      {Array.isArray(b.attachments) && b.attachments.length > 0 && (
        <div className={styles["anexos-container"]}>
          <h4 className={styles["anexos-titulo"]}>Anexos da Pergunta</h4>
          <ul className={styles["anexos-lista"]}>
            {b.attachments.map((f, i) => (
              <li key={i} className={styles["anexo-item"]}>
                <span className={styles["anexo-icon"]}>
                  <Paperclip size={16} />
                </span>
                <div className={styles["anexo-info"]}>
                  <span className={styles["anexo-nome"]}>{f.name}</span>
                  <span className={styles["anexo-tamanho"]}>
                    {f.size ? `${(f.size / 1024 / 1024).toFixed(2)} MB` : ''}
                  </span>
                </div>
                <button
                  type="button"
                  className={styles["btn-remover-anexo"]}
                  onClick={(e) => {
                    e.stopPropagation();
                    // Remover anexo da lista
                    const novasAttachments = b.attachments.filter((_, index) => index !== i);
                    // Atualizar o bloco com as novas attachments
                    if (onChangeBloco) {
                      onChangeBloco(b.id, { attachments: novasAttachments });
                    }
                  }}
                  title="Remover anexo"
                >
                  ×
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
};

function areEqualMult(a, b) {
  const A = a.bloco || {};
  const B = b.bloco || {};
  return (
    A.id === B.id &&
    A.selecionado === B.selecionado &&
    A.texto === B.texto &&
    A.TemGabarito === B.TemGabarito &&
    A.permitirMultiplaSelecao === B.permitirMultiplaSelecao &&
    A.opcoes === B.opcoes &&         // referência
    A.corretas === B.corretas &&     // referência
    A.imagens === B.imagens &&       // referência
    A.attachments === B.attachments && // referência
    A.estilo === B.estilo &&
    a.style === b.style &&
    a.onChangeTexto === b.onChangeTexto &&
    a.onChangeOpcoes === b.onChangeOpcoes &&
    a.onChangeBloco === b.onChangeBloco &&
    a.onRemoveImagem === b.onRemoveImagem &&
    a.onToggleGabarito === b.onToggleGabarito &&
    a.onTogglePermiteMultipla === b.onTogglePermiteMultipla &&
    a.onToggleCorreta === b.onToggleCorreta &&
    a.onClick === b.onClick
  );
}

export default React.memo(MultiplaEscolhaBase, areEqualMult);
