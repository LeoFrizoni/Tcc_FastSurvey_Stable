import React from "react";
import { Trash2, Plus } from "lucide-react";
import "./pergunta.css";

const MultiplaEscolha = ({
  bloco,
  onChangeTexto,
  onChangeOpcoes,
  onRemoveImagem,
  style,
  onClick
}) => {
  const atualizarOpcao = (index, valor) => {
    const novasOpcoes = [...bloco.opcoes];
    novasOpcoes[index] = valor;
    onChangeOpcoes(bloco.id, novasOpcoes);
  };
  const removerOpcao = (index) => {
    const novas = bloco.opcoes.filter((_, i) => i !== index);
    onChangeOpcoes(bloco.id, novas);
  };

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
      onClick={(e) => { e.stopPropagation(); onClick(); }}
    >
      <input
        type="text"
        value={bloco.texto}
        onChange={(e) => onChangeTexto(bloco.id, e.target.value)}
        placeholder="Digite sua pergunta aqui..."
        className="input-pergunta"
      />

      {/* PREVIEW ENTRE INPUT E OPÇÕES */}
      {Array.isArray(bloco.imagens) && bloco.imagens.length > 0 && (
        <div className={`galeria-imagens ${unica ? "centralizada" : ""}`}>
          {bloco.imagens.map((img, i) => (
            <div key={i} className={`thumb-imagem ${unica ? "grande" : ""}`}>
              <img src={img.previewUrl} alt={img.file?.name || `imagem-${i}`} />
              <button
                className="btn-remover-img"
                title="Remover imagem"
                onClick={(e) => { e.stopPropagation(); onRemoveImagem(bloco.id, i); }}
                aria-label="Remover imagem"
              >
                <Trash2 size={18} />
              </button>
            </div>
          ))}
        </div>
      )}

      {bloco.opcoes.map((opcao, index) => {
        const valor =
          typeof opcao === "object" && opcao !== null
            ? (opcao.texto ?? opcao.opcao ?? "")
            : opcao;
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
            <button
              className="btn-remover"
              onClick={() => removerOpcao(index)}
              title="Remover opção"
            >
              <Trash2 size={16} />
            </button>
          </div>
        );
      })}

      <button
        className="adicionar-opcao-btn"
        onClick={() => onChangeOpcoes(bloco.id, [...bloco.opcoes, ""])}
      >
        <Plus size={16} /> Adicionar Opção
      </button>
    </div>
  );
};

export default MultiplaEscolha;
