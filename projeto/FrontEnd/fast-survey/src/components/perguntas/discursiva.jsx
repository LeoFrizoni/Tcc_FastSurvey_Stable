import React from "react";
import { Trash2 } from "lucide-react";
import "./pergunta.css";

const Discursiva = ({ bloco, onChangeTexto, onRemove, style, onClick }) => {
  const estiloContainer = {
    backgroundColor: style?.backgroundColor || "transparent",
    fontFamily: style?.fontFamily || "inherit",
  };

  return (
    <div
      className={`bloco-pergunta ${bloco.selecionado ? "selecionado" : ""}`}
      style={estiloContainer}
      onClick={(e) => { e.stopPropagation(); onClick(); }}
    >
      <div className="cabecalho-pergunta">
        <h4 style={{ color: style?.color || "inherit" }}>Pergunta Discursiva</h4>
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
      />

      <textarea
        disabled
        placeholder="Resposta do usuário..."
        className="resposta-simulada"
      />
    </div>
  );
};

export default Discursiva;
