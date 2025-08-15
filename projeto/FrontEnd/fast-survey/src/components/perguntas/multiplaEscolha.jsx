import React from "react";
import { Trash2, Plus } from "lucide-react";
import "./pergunta.css";

const MultiplaEscolha = ({ bloco, onChangeTexto, onChangeOpcoes, onRemove, style, onClick }) => {
  const atualizarOpcao = (index, valor) => {
    const novasOpcoes = [...bloco.opcoes];
    novasOpcoes[index] = valor;
    onChangeOpcoes(bloco.id, novasOpcoes);
  };

  const adicionarOpcao = () => {
    onChangeOpcoes(bloco.id, [...bloco.opcoes, ""]);
  };

  const removerOpcao = (index) => {
    const novasOpcoes = bloco.opcoes.filter((_, i) => i !== index);
    onChangeOpcoes(bloco.id, novasOpcoes);
  };

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
        <h4 style={estiloTexto}>Pergunta Múltipla Escolha</h4>
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

      {bloco.opcoes.map((opcao, index) => (
        <div key={index} className="opcao-input">
          <input type="checkbox" disabled style={{ marginRight: "8px" }} />
          <input
            type="text"
            value={opcao}
            onChange={(e) => atualizarOpcao(index, e.target.value)}
            placeholder={`Opção ${index + 1}`}
            className="input-opcao"
            style={estiloTexto}
          />
          <button className="btn-remover" onClick={() => removerOpcao(index)} title="Remover opção">
            <Trash2 size={16} />
          </button>
        </div>
      ))}

      <button className="adicionar-opcao-btn" onClick={adicionarOpcao}>
        <Plus size={16} /> Adicionar Opção
      </button>
    </div>
  );
};

export default MultiplaEscolha;
