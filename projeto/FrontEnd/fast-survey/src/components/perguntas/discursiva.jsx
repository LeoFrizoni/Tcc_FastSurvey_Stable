import React from "react";
import { Trash2 } from "lucide-react";
import "./pergunta.css";

const Discursiva = ({ bloco, onChangeTexto, onRemove, style, onClick }) => {
  const estiloTexto = {
    color: style?.color || "inherit",
    fontFamily: style?.fontFamily || "inherit"
  };

  return (
    <div
      className={`bloco-pergunta ${bloco.selecionado ? "selecionado" : ""}`}
      style={{ backgroundColor: style?.backgroundColor || "transparent" }}
      onClick={(e) => {
        e.stopPropagation();
        onClick();
      }}
    >
      <div className="cabecalho-pergunta">
        <h4 style={estiloTexto}>Pergunta Discursiva</h4>
        <button className="btn-remover" onClick={() => onRemove(bloco.id)} title="Remover pergunta">
          <Trash2 size={18} />
        </button>
      </div>
      <input
        type="text"
        value={bloco.texto}
        onChange={(e) => onChangeTexto(bloco.id, e.target.value)}
        placeholder="Digite sua pergunta aqui..."
        className="input-pergunta"
        style={estiloTexto}
      />
      <textarea
        disabled
        placeholder="Resposta do usuário..."
        className="resposta-simulada"
        style={estiloTexto}
      />
    </div>
  );
};

export default Discursiva;
