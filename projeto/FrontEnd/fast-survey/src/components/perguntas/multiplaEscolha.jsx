import React, { useMemo } from "react";
import { Trash2, Plus } from "lucide-react";
import "./pergunta.css";

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

  const isCorreta = (idx) => Array.isArray(b.corretas) && b.corretas.includes(idx);

  return (
    <div
      className={`bloco-pergunta ${b.selecionado ? "selecionado" : ""}`}
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
        className="input-pergunta"
        onClick={(e) => e.stopPropagation()}
      />

      {/* PREVIEW ENTRE INPUT E OPÇÕES */}
      {imagens.length > 0 && (
        <div className={`galeria-imagens ${unica ? "centralizada" : ""}`}>
          {imagens.map((img, i) => {
            const src = pickImgSrc(img);
            const alt = pickImgAlt(img, i);
            return (
              <div key={i} className={`thumb-imagem ${unica ? "grande" : ""}`}>
                {src ? <img src={src} alt={alt} /> : <span className="thumb-placeholder">{alt}</span>}
                <button
                  type="button"
                  className="btn-remover-img"
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
      <div className="grupo-config" onClick={(e) => e.stopPropagation()}>
        <label className="switch">
          <input
            type="checkbox"
            checked={!!b.temGabarito}
            onChange={(e) => onToggleGabarito(b.id, e.target.checked)}
          />
          <span>Há gabarito?</span>
        </label>

        <label className="switch">
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
          <input type="checkbox" disabled className="checkbox-simulador" />
        ) : (
          <input type="radio" disabled className="radio-simulador" />
        );

        return (
          <div key={index} className="opcao-input" onClick={(e) => e.stopPropagation()}>
            {simulador}

            <input
              type="text"
              value={valor}
              onChange={(e) => atualizarOpcao(index, e.target.value)}
              placeholder={`Opção ${index + 1}`}
              className="input-opcao"
            />

            {b.temGabarito && (
              <label className="marcar-correta">
                <input
                  type={b.permitirMultiplaSelecao ? "checkbox" : "radio"}
                  name={`gabarito-multipla-${b.id}`}
                  checked={isCorreta(index)}
                  onChange={() => onToggleCorreta(b.id, index)}
                />
                <span>Correta</span>
              </label>
            )}

            <button
              type="button"
              className="btn-remover"
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
        className="adicionar-opcao-btn"
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

function areEqualMult(a, b) {
  const A = a.bloco || {};
  const B = b.bloco || {};
  return (
    A.id === B.id &&
    A.selecionado === B.selecionado &&
    A.texto === B.texto &&
    A.temGabarito === B.temGabarito &&
    A.permitirMultiplaSelecao === B.permitirMultiplaSelecao &&
    A.opcoes === B.opcoes &&         // referência
    A.corretas === B.corretas &&     // referência
    A.imagens === B.imagens &&       // referência
    A.estilo === B.estilo &&
    a.style === b.style &&
    a.onChangeTexto === b.onChangeTexto &&
    a.onChangeOpcoes === b.onChangeOpcoes &&
    a.onRemoveImagem === b.onRemoveImagem &&
    a.onToggleGabarito === b.onToggleGabarito &&
    a.onTogglePermiteMultipla === b.onTogglePermiteMultipla &&
    a.onToggleCorreta === b.onToggleCorreta &&
    a.onClick === b.onClick
  );
}

export default React.memo(MultiplaEscolhaBase, areEqualMult);
