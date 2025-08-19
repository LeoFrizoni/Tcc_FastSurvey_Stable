import React, { useMemo } from "react";
import { Trash2 } from "lucide-react";
import "./pergunta.css";

/** Util: aplica TemplateJson + style externo */
function applyBlockStyle(estilo = {}, override = {}) {
  const {
    corFundo,
    corTexto,
    fonte,
    padding,
    largura,
    alinhamento,
    borda,
    sombra,
    bordaRadius,
    margemInferior,
  } = estilo || {};

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

function pickImgSrc(img) {
  return img?.previewUrl || img?.url || img?.base64 || null;
}
function pickImgAlt(img, i) {
  return img?.file?.name || img?.nome || img?.alt || `imagem-${i}`;
}

const Discursiva = ({
  bloco,
  onChangeTexto,
  onRemoveImagem,
  style,
  onClick,
  onChangeRespostaExemplo,
}) => {
  const b = bloco || {};
  const imagens = Array.isArray(b.imagens) ? b.imagens : [];
  const unica = imagens.length === 1;

  const estiloContainer = useMemo(
    () => applyBlockStyle(b.estilo, {
      backgroundColor: style?.backgroundColor || undefined,
      fontFamily: style?.fontFamily || undefined,
      color: style?.color || undefined,
    }),
    [b.estilo, style]
  );

  return (
    <div
      className={`bloco-pergunta ${b.selecionado ? "selecionado" : ""}`}
      style={estiloContainer}
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

      {/* PREVIEW ENTRE INPUT E RESPOSTA */}
      {imagens.length > 0 && (
        <div className={`galeria-imagens ${unica ? "centralizada" : ""}`}>
          {imagens.map((img, i) => {
            const src = pickImgSrc(img);
            const alt = pickImgAlt(img, i);
            return (
              <div key={i} className={`thumb-imagem ${unica ? "grande" : ""}`}>
                {src ? (
                  <img src={src} alt={alt} />
                ) : (
                  <span className="thumb-placeholder">{alt}</span>
                )}
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

      {/* Exemplo de resposta (para futura IA) */}
      <div className="grupo-config" onClick={(e) => e.stopPropagation()}>
        <label className="label-inline" htmlFor={`exemplo-${b.id}`}>
          Exemplo de resposta (para IA no futuro)
        </label>
        <textarea
          id={`exemplo-${b.id}`}
          value={b.respostaExemplo || ""}
          onChange={(e) => onChangeRespostaExemplo(b.id, e.target.value)}
          placeholder="Escreva aqui um exemplo de boa resposta..."
          className="input-exemplo"
          rows={3}
        />
      </div>

      <textarea
        disabled
        placeholder="Resposta do usuário..."
        className="resposta-simulada"
        onClick={(e) => e.stopPropagation()}
      />
    </div>
  );
};

export default Discursiva;
