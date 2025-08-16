import React, { useState, useEffect } from "react";
import axios from "axios";
import "./modalCriarPesquisa.css";

const ModalCriarPesquisa = ({ onConfirm }) => {
  const [titulo, setTitulo] = useState("");
  const [descricao, setDescricao] = useState("");
  const [tipoSelecionado, setTipoSelecionado] = useState("");
  const [tempoExpiracao, setTempoExpiracao] = useState(1); 
  const [tipos, setTipos] = useState([]);
  const [erros, setErros] = useState({});

  useEffect(() => {
    async function carregarTipos() {
      try {
        const res = await axios.get("http://localhost:5062/api/TipoPesquisa/ListarTipoPesquisa");
        setTipos(res.data);
      } catch (err) {
        console.error("Erro ao buscar tipos:", err);
      }
    }
    carregarTipos();
  }, []);

  const handleConfirm = () => {
    const novosErros = {};
    if (!titulo) novosErros.titulo = "Título é obrigatório.";
    if (!descricao) novosErros.descricao = "Descrição é obrigatória.";
    if (!tipoSelecionado) novosErros.tipo = "Selecione o tipo de pesquisa.";
    if (!tempoExpiracao || tempoExpiracao <= 0) novosErros.tempoExpiracao = "Informe um tempo válido.";
    setErros(novosErros);
    if (Object.keys(novosErros).length > 0) return;

    onConfirm({
      titulo,
      descricao,
      tipo: tipoSelecionado,
      tempoExpiracao: parseInt(tempoExpiracao),
    });
  };

  return (
    <div className="modal-overlay">
      <div className="modal-criar">
        <h2>Nova Pesquisa</h2>

        <div className="form-group">
          <label htmlFor="titulo">Título</label>
          <input
            id="titulo"
            type="text"
            value={titulo}
            onChange={(e) => setTitulo(e.target.value)}
            placeholder="Digite o título da pesquisa"
            style={erros.titulo ? { borderColor: '#e74c3c' } : {}}
          />
          {erros.titulo && <span style={{ color: '#e74c3c', fontSize: '0.95rem' }}>{erros.titulo}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="descricao">Descrição</label>
          <textarea
            id="descricao"
            value={descricao}
            onChange={(e) => setDescricao(e.target.value)}
            placeholder="Descreva a pesquisa brevemente"
            style={erros.descricao ? { borderColor: '#e74c3c' } : {}}
          />
          {erros.descricao && <span style={{ color: '#e74c3c', fontSize: '0.95rem' }}>{erros.descricao}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="tipo">Tipo de Pesquisa</label>
          <select
            id="tipo"
            value={tipoSelecionado}
            onChange={(e) => setTipoSelecionado(e.target.value)}
            style={erros.tipo ? { borderColor: '#e74c3c' } : {}}
          >
            <option value="">Selecione um tipo</option>
            {tipos
              .filter((tipo) => tipo.desabilitado === false)
              .map((tipo) => (
                <option key={tipo.tipopesquisaid} value={tipo.tipopesquisaid}>
                  {tipo.tipopesquisa1}
                </option>
              ))}
          </select>
          {erros.tipo && <span style={{ color: '#e74c3c', fontSize: '0.95rem' }}>{erros.tipo}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="expiracao">Disponível por (horas)</label>
          <input
            id="expiracao"
            type="number"
            min="1"
            value={tempoExpiracao}
            onChange={(e) => setTempoExpiracao(e.target.value)}
            placeholder="Ex: 1"
            style={erros.tempoExpiracao ? { borderColor: '#e74c3c' } : {}}
          />
          {erros.tempoExpiracao && <span style={{ color: '#e74c3c', fontSize: '0.95rem' }}>{erros.tempoExpiracao}</span>}
        </div>

        <button onClick={handleConfirm}>Começar</button>
      </div>
    </div>
  );
};

export default ModalCriarPesquisa;
