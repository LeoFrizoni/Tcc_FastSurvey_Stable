import React, { useState, useRef } from "react";
import TopNavbar from "../../components/layouts/TopNavBar";
import ModalCriarPesquisa from "../../components/layouts/ModalCriarPesquisa";
import ModalQRCode from "../../components/layouts/ModalQrCode"
import Discursiva from "../../components/perguntas/discursiva";
import MultiplaEscolha from "../../components/perguntas/multiplaEscolha";
import Objetiva from "../../components/perguntas/objetiva";
import axios from "axios";
import {
  FileText,
  Circle,
  Paperclip,
  Save,
  ChevronDown,
  ChevronUp,
  Paintbrush2,
  Type,
  Droplet,
  Trash2
} from "lucide-react";
import "./createPesquisa.css";

const CriarPesquisa = () => {
  const [showModal, setShowModal] = useState(true);
  const [isPerguntasAberto, setIsPerguntasAberto] = useState(false);
  const [qrUrl, setQrUrl] = useState("");
  const [selectedBlockId, setSelectedBlockId] = useState(null);
  const [mostrarModalQr, setMostrarModalQr] = useState(false);

  const inputFileRef = useRef();

  const [dadosPesquisa, setDadosPesquisa] = useState({
    titulo: "",
    descricao: "",
    tipo: ""
  });

  const [blocos, setBlocos] = useState([]);

  const adicionarBloco = (tipo) => {
    const novo = {
      id: Date.now(),
      tipo,
      texto: "",
      opcoes: tipo === "multipla" || tipo === "objetiva" ? [""] : [],
      arquivo: null,
      estilo: {
        corFundo: "",
        corTexto: "",
        fonte: ""
      }
    };

    if (tipo === "anexo") {
      inputFileRef.current.click();
      novo.pendente = true;
    } else {
      setBlocos([...blocos, novo]);
    }
  };

  const handleArquivoSelecionado = (event) => {
    const file = event.target.files[0];
    if (file) {
      const novo = {
        id: Date.now(),
        tipo: "anexo",
        arquivo: file,
        estilo: {
          corFundo: "",
          corTexto: "",
          fonte: ""
        }
      };
      setBlocos((prev) => [...prev, novo]);
    }
  };

  const atualizarBloco = (id, dados) => {
    const atualizados = blocos.map((bloco) =>
      bloco.id === id ? { ...bloco, ...dados } : bloco
    );
    setBlocos(atualizados);
  };

  const atualizarTexto = (id, novoTexto) => {
    atualizarBloco(id, { texto: novoTexto });
  };

  const atualizarOpcoes = (id, novasOpcoes) => {
    atualizarBloco(id, { opcoes: novasOpcoes });
  };

  const atualizarEstilo = (campo, valor) => {
    setBlocos((prev) =>
      prev.map((bloco) =>
        bloco.id === selectedBlockId
          ? { ...bloco, estilo: { ...bloco.estilo, [campo]: valor } }
          : bloco
      )
    );
  };

  const removerBloco = (id) => {
    setBlocos(blocos.filter((bloco) => bloco.id !== id));
    if (selectedBlockId === id) setSelectedBlockId(null);
  };

  const montarPesquisaVM = () => {
    const loginId = parseInt(localStorage.getItem("userId"));

    return {
      titulo: dadosPesquisa.titulo,
      tipoPesquisaId: parseInt(dadosPesquisa.tipo),
      codigoPesquisa: 0,
      loginId,
      perguntasDiscursivas: blocos
        .filter((b) => b.tipo === "discursiva")
        .map((b) => ({ titulo: b.texto, resposta: "" })),
      perguntasObjetivas: blocos
        .filter((b) => b.tipo === "multipla" || b.tipo === "objetiva")
        .map((b) => ({
          titulo: b.texto,
          tipo: b.tipo,
          opcoes: b.opcoes.filter((o) => o.trim()).map((o) => ({ opcao: o, respostaCerta: null }))
        })),
      templateJson: JSON.stringify(blocos) // ✅ inclusão do campo TemplateJson
    };
  };

  const salvarPesquisaComPerguntas = async () => {
    try {
      const pesquisaVM = montarPesquisaVM();
      const response = await axios.post("http://localhost:5062/api/pesquisas", pesquisaVM);

      const id = response.data.pesquisaid;
      const urlResposta = `${window.location.origin}/responder/${id}`;
      setQrUrl(urlResposta);
      alert("Pesquisa salva com sucesso!");
    } catch (erro) {
      console.error("Erro ao salvar pesquisa:", erro);
      alert("Ocorreu um erro ao salvar. Verifique os dados e tente novamente.");
    }
  };

  const renderizarBloco = (bloco) => {
    const estilo = {
      backgroundColor: bloco.estilo.corFundo || "transparent",
      color: bloco.estilo.corTexto || "inherit",
      fontFamily: bloco.estilo.fonte || "inherit"
    };

    const propsComuns = {
      bloco: {
        ...bloco,
        selecionado: bloco.id === selectedBlockId
      },
      onChangeTexto: atualizarTexto,
      onChangeOpcoes: atualizarOpcoes,
      onRemove: removerBloco,
      onClick: () => setSelectedBlockId(bloco.id),
      style: estilo
    };

    switch (bloco.tipo) {
      case "discursiva":
        return <Discursiva key={bloco.id} {...propsComuns} />;
      case "multipla":
        return <MultiplaEscolha key={bloco.id} {...propsComuns} />;
      case "objetiva":
        return <Objetiva key={bloco.id} {...propsComuns} />;
      case "anexo":
        return (
          <div
            key={bloco.id}
            className={`bloco-pergunta ${bloco.selecionado ? "selecionado" : ""}`}
            style={{
              ...estilo,
              padding: '1rem',
              border: '1px solid #eee',
              borderRadius: '10px',
              backgroundColor: '#fff',
              marginBottom: '1rem'
            }}
            onClick={() => setSelectedBlockId(bloco.id)}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <h4 style={{ margin: 0 }}>Arquivo Anexo</h4>
              <button className="btn-remover" onClick={() => removerBloco(bloco.id)} title="Remover anexo">
                <Trash2 size={18} />
              </button>
            </div>
            <p><strong>Arquivo:</strong> {bloco.arquivo?.name || "Nenhum arquivo selecionado"}</p>
          </div>
        );
      default:
        return null;
    }
  };
  return (
    <>
      <TopNavbar />
      <div className="editor-container">
        <input
          type="file"
          ref={inputFileRef}
          style={{ display: "none" }}
          onChange={handleArquivoSelecionado}
        />

        {showModal && (
          <ModalCriarPesquisa
            onConfirm={(dados) => {
              setDadosPesquisa(dados);
              setShowModal(false);
            }}
          />
        )}

        <aside className="sidebar-esquerda">
          <div className="sidebar-scroll">
            <h3>Componentes</h3>
            <ul>
              <li className="item-clique" onClick={() => setIsPerguntasAberto(!isPerguntasAberto)}>
                {isPerguntasAberto ? <ChevronUp size={18} /> : <ChevronDown size={18} />} Perguntas
              </li>
              {isPerguntasAberto && (
                <ul className="submenu">
                  <li className="item-clique" onClick={() => adicionarBloco("discursiva")}> <FileText size={18} /> Discursiva </li>
                  <li className="item-clique" onClick={() => adicionarBloco("multipla")}> <Circle size={18} /> Múltipla Escolha </li>
                  <li className="item-clique" onClick={() => adicionarBloco("objetiva")}> <Circle size={18} /> Objetiva (única) </li>
                </ul>
              )}
              <li className="item-clique" onClick={() => adicionarBloco("anexo")}> <Paperclip size={18} /> Anexo </li>
            </ul>
          </div>

          <div className="salvar-wrapper">
            <button className="salvar-btn" onClick={salvarPesquisaComPerguntas}>
              <Save size={18} style={{ marginRight: "6px" }} /> Salvar Pesquisa
            </button>

            {qrUrl && (
              <>
                <button className="salvar-btn" onClick={() => setMostrarModalQr(true)}>
                  Visualizar QR Code
                </button>
                <ModalQRCode
                  isOpen={mostrarModalQr}
                  onClose={() => setMostrarModalQr(false)}
                  qrUrl={qrUrl}
                />
              </>
            )}
          </div>
        </aside>

        <main className="area-construcao">
          <h2>{dadosPesquisa.titulo}</h2>
          <p>{dadosPesquisa.descricao}</p>
          <p><em>Tipo: {dadosPesquisa.tipo}</em></p>

          <div className={`area-preview ${blocos.length > 0 ? "invisivel" : ""}`}>
            <p>Adicione blocos para montar sua pesquisa</p>
          </div>

          {blocos.length > 0 && blocos.map(renderizarBloco)}
        </main>
        <aside className="sidebar-direita">
          <h3>Estilo</h3>
          <label> <Paintbrush2 size={18} /> Cor de Fundo
            <input type="color" onChange={(e) => atualizarEstilo("corFundo", e.target.value)} disabled={!selectedBlockId} />
          </label>
          <label> <Droplet size={18} /> Cor do Texto
            <input type="color" onChange={(e) => atualizarEstilo("corTexto", e.target.value)} disabled={!selectedBlockId} />
          </label>
          <label> <Type size={18} /> Fonte
            <select onChange={(e) => atualizarEstilo("fonte", e.target.value)} disabled={!selectedBlockId}>
              <option value="">Padrão</option>
              <option style={{ fontFamily: "Arial" }} value="Arial">Arial</option>
              <option style={{ fontFamily: "Helvetica" }} value="Helvetica">Helvetica</option>
              <option style={{ fontFamily: "Verdana" }} value="Verdana">Verdana</option>
              <option style={{ fontFamily: "Tahoma" }} value="Tahoma">Tahoma</option>
              <option style={{ fontFamily: "Trebuchet MS" }} value="Trebuchet MS">Trebuchet MS</option>
              <option style={{ fontFamily: "Georgia" }} value="Georgia">Georgia</option>
              <option style={{ fontFamily: "Times New Roman" }} value="Times New Roman">Times New Roman</option>
              <option style={{ fontFamily: "Courier New" }} value="Courier New">Courier New</option>
              <option style={{ fontFamily: "Lucida Console" }} value="Lucida Console">Lucida Console</option>
              <option style={{ fontFamily: "Impact" }} value="Impact">Impact</option>
              <option style={{ fontFamily: "Palatino Linotype" }} value="Palatino Linotype">Palatino Linotype</option>
              <option style={{ fontFamily: "Segoe UI" }} value="Segoe UI">Segoe UI</option>
              <option style={{ fontFamily: "Cambria" }} value="Cambria">Cambria</option>
              <option style={{ fontFamily: "Garamond" }} value="Garamond">Garamond</option>
              <option style={{ fontFamily: "Franklin Gothic Medium" }} value="Franklin Gothic Medium">Franklin Gothic Medium</option>
              <option style={{ fontFamily: "Brush Script MT" }} value="Brush Script MT">Brush Script MT</option>
              <option style={{ fontFamily: "Comic Sans MS" }} value="Comic Sans MS">Comic Sans MS</option>
              <option style={{ fontFamily: "Copperplate" }} value="Copperplate">Copperplate</option>
              <option style={{ fontFamily: "Fira Sans" }} value="Fira Sans">Fira Sans</option>
            </select>
          </label>
        </aside>
      </div>
    </>
  );
};

export default CriarPesquisa;
