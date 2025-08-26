import React, { useMemo } from "react";
import { Trash2, Plus } from "lucide-react";
import styles from "./pergunta.module.css";
import '../layouts/global.css';

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

const ObjetivaBase = ({
  bloco,
  onChangeTexto,
  onChangeOpcoes,
  onRemoveImagem,
  style,
  onClick,
  onToggleGabarito,
  onSetCorretaIndex,
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

  const atualizarOpcao = (index, valor) => {
    const novas = [...opcoes];
    if (typeof novas[index] === "object" && novas[index] !== null) {
      novas[index] = { ...novas[index], texto: valor };
    } else {
      novas[index] = valor;
    }
    onChangeOpcoes(b.id, novas);
  };

  const removerOpcao = (index) => {
    const novas = opcoes.filter((_, i) => i !== index);
    onChangeOpcoes(b.id, novas);
  };

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

      {/* Config da pergunta */}
              <div className={styles["grupo-config"]} onClick={(e) => e.stopPropagation()}>
                  <label className={styles.switch}>
          <input
            type="checkbox"
            checked={!!b.TemGabarito}
            onChange={(e) => onToggleGabarito(b.id, e.target.checked)}
          />
          <span>Há gabarito?</span>
        </label>
      </div>

      {opcoes.map((opcao, index) => {
        const valor = getOpcaoTexto(opcao);
        return (
          <div key={index} className={styles["opcao-input"]} onClick={(e) => e.stopPropagation()}>
                          <input type="radio" disabled className={styles["radio-simulador"]} />

            <input
              type="text"
              value={valor}
              onChange={(e) => atualizarOpcao(index, e.target.value)}
              placeholder={`Opção ${index + 1}`}
              className={styles["input-opcao"]}
            />

            {b.TemGabarito && (
              <label className={styles["marcar-correta"]}>
                <input
                  type="radio"
                  name={`gabarito-objetiva-${b.id}`}
                  checked={b.corretaIndex === index}
                  onChange={() => onSetCorretaIndex(b.id, index)}
                />
                <span>Correta</span>
              </label>
            )}

            <button
              type="button"
              className={styles["btn-remover"]}
              onClick={() => removerOpcao(index)}
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
          onChangeOpcoes(b.id, [...opcoes, ""]);
        }}
      >
        <Plus size={16} /> Adicionar Opção
      </button>
    </div>
  );
};

function areEqualObj(a, b) {
  const A = a.bloco || {};
  const B = b.bloco || {};
  return (
    A.id === B.id &&
    A.selecionado === B.selecionado &&
    A.texto === B.texto &&
    A.TemGabarito === B.TemGabarito &&
    A.corretaIndex === B.corretaIndex &&
    A.opcoes === B.opcoes &&       // referência
    A.imagens === B.imagens &&     // referência
    A.estilo === B.estilo &&
    a.style === b.style &&
    a.onChangeTexto === b.onChangeTexto &&
    a.onChangeOpcoes === b.onChangeOpcoes &&
    a.onRemoveImagem === b.onRemoveImagem &&
    a.onToggleGabarito === b.onToggleGabarito &&
    a.onSetCorretaIndex === b.onSetCorretaIndex &&
    a.onClick === b.onClick
  );
}

export default React.memo(ObjetivaBase, areEqualObj);
