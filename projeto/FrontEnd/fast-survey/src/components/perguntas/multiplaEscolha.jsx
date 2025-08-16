import React from "react";
import { Trash2, Plus } from "lucide-react";
import "./pergunta.css";

const MultiplaEscolha = ({ bloco, onChangeTexto, onChangeOpcoes, onRemove, style, onClick }) => {
  const atualizarOpcao = (index, valor) => {
    const novasOpcoes = [...bloco.opcoes];
    novasOpcoes[index] = valor;
    onChangeOpcoes(bloco.id, novasOpcoes);
  };

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
        <h4 style={{ color: style?.color || "inherit" }}>Pergunta Múltipla Escolha</h4>
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

      {bloco.opcoes.map((opcao, index) => {
        const valor = typeof opcao === "object" && opcao !== null ? (opcao.texto ?? opcao.opcao ?? "") : opcao;
        return (
          <div key={index} className="opcao-input">
            <input type="checkbox" disabled className="checkbox-simulador" />
            <input
              type="text"
              value={valor}
              onChange={(e) => atualizarOpcao(index, e.target.value)}
              placeholder={`Opção ${index + 1}`}
              className="input-opcao"
            />
            <button className="btn-remover" onClick={() => onRemove(bloco.id)} title="Remover opção">
              <Trash2 size={16} />
            </button>
          </div>
        );
      })}

      <button className="adicionar-opcao-btn" onClick={() => onChangeOpcoes(bloco.id, [...bloco.opcoes, ""])}>
        <Plus size={16} /> Adicionar Opção
      </button>
    </div>
  );
};

export default MultiplaEscolha;
