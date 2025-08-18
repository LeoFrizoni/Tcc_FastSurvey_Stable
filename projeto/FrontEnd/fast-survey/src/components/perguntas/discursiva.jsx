import React from "react";
import { Trash2 } from "lucide-react";
import "./pergunta.css";

const Discursiva = ({
  bloco,
  onChangeTexto,
  onRemoveImagem,
  style,
  onClick,
  onChangeRespostaExemplo,
}) => {
  const estiloContainer = {
    backgroundColor: style?.backgroundColor || "transparent",
    fontFamily: style?.fontFamily || "inherit",
    color: style?.color || "inherit",
  };

  const unica = Array.isArray(bloco.imagens) && bloco.imagens.length === 1;

  return (
    <div
      className={`bloco-pergunta ${bloco.selecionado ? "selecionado" : ""}`}
      style={estiloContainer}
      onClick={(e) => {
        e.stopPropagation();
        onClick();
      }}
    >
      <input
        type="text"
        value={bloco.texto}
        onChange={(e) => onChangeTexto(bloco.id, e.target.value)}
        placeholder="Digite sua pergunta aqui..."
        className="input-pergunta"
      />

      {/* PREVIEW ENTRE INPUT E RESPOSTA */}
      {Array.isArray(bloco.imagens) && bloco.imagens.length > 0 && (
        <div className={`galeria-imagens ${unica ? "centralizada" : ""}`}>
          {bloco.imagens.map((img, i) => (
            <div key={i} className={`thumb-imagem ${unica ? "grande" : ""}`}>
              <img src={img.previewUrl} alt={img.file?.name || `imagem-${i}`} />
              <button
                className="btn-remover-img"
                title="Remover imagem"
                onClick={(e) => {
                  e.stopPropagation();
                  onRemoveImagem(bloco.id, i);
                }}
                aria-label="Remover imagem"
              >
                <Trash2 size={18} />
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Exemplo de resposta (para futura IA) */}
      <div className="grupo-config">
        <label className="label-inline" htmlFor={`exemplo-${bloco.id}`}>
          Exemplo de resposta (para IA no futuro)
        </label>
        <textarea
          id={`exemplo-${bloco.id}`}
          value={bloco.respostaExemplo || ""}
          onChange={(e) => onChangeRespostaExemplo(bloco.id, e.target.value)}
          placeholder="Escreva aqui um exemplo de boa resposta..."
          className="input-exemplo"
          rows={3}
        />
      </div>

      <textarea
        disabled
        placeholder="Resposta do usuário..."
        className="resposta-simulada"
      />
    </div>
  );
};

export default Discursiva;
