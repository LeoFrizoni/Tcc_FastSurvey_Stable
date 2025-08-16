import React, { useState, useRef, useEffect } from "react";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import TopNavbar from "../../components/layouts/TopNavBar";
import ModalCriarPesquisa from "../../components/layouts/ModalCriarPesquisa";
import ModalQRCode from "../../components/layouts/ModalQrCode";
import ModalPreviewPesquisa from "../../components/layouts/ModalPreviewPesquisa";
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
import styles from "./createPesquisa.module.css";
import CabecalhoPesquisa from "../../components/layouts/CabecalhoPesquisa";
import InformacoesPesquisa from "../../components/layouts/InformacoesPesquisa";

const API_BASE = "http://localhost:5062";

const CriarPesquisa = () => {
  const [showModal, setShowModal] = useState(true);
  const [isPerguntasAberto, setIsPerguntasAberto] = useState(false);
  const [qrUrl, setQrUrl] = useState("");
  const [mostrarModalPreview, setMostrarModalPreview] = useState(false);
  const [selectedBlockId, setSelectedBlockId] = useState(null);
  const [mostrarModalQr, setMostrarModalQr] = useState(false);

  const inputFileRef = useRef();

  const [dadosPesquisa, setDadosPesquisa] = useState({
    titulo: "",
    descricao: "",
    tipo: ""
  });

  const [blocos, setBlocos] = useState([]);
  const [autorNome, setAutorNome] = useState("");
  const [tiposPesquisa, setTiposPesquisa] = useState([]);

  useEffect(() => {
    const loginId = localStorage.getItem("userId");
    if (loginId) {
      axios.get(`http://localhost:5062/api/login/SelecionarLoginPorId/${loginId}`)
        .then(res => {
          setAutorNome(res.data.usuario || "Usuário");
        })
        .catch(() => setAutorNome("Usuário"));
    }
  }, []);

  useEffect(() => {
    axios.get("http://localhost:5062/api/tipopesquisa/ListarTipoPesquisa")
      .then(res => setTiposPesquisa(res.data))
      .catch(() => setTiposPesquisa([]));
  }, []);

  const adicionarBloco = (tipo) => {
    const novo = {
      id: Date.now(),
      tipo,
      texto: "",
      opcoes: tipo === "multipla" || tipo === "objetiva" ? [""] : [],
      arquivo: null,
      estilo: { corFundo: "", corTexto: "", fonte: "" }
    };

    if (tipo === "anexo") {
      inputFileRef.current?.click();
      novo.pendente = true;
    } else {
      setBlocos(prev => [...prev, novo]);
    }
  };

  // agora aceita múltiplos arquivos
  const handleArquivoSelecionado = (event) => {
    const files = Array.from(event.target.files || []);
    if (files.length === 0) return;

    const novos = files.map((file, i) => ({
      id: Date.now() + i,
      tipo: "anexo",
      arquivo: file,
      estilo: { corFundo: "", corTexto: "", fonte: "" }
    }));
    setBlocos(prev => [...prev, ...novos]);

    // reseta input pra poder escolher o mesmo arquivo de novo se precisar
    event.target.value = "";
  };

  const atualizarBloco = (id, dados) => {
    setBlocos(prev => prev.map(bloco => (bloco.id === id ? { ...bloco, ...dados } : bloco)));
  };

  const atualizarTexto = (id, novoTexto) => atualizarBloco(id, { texto: novoTexto });
  const atualizarOpcoes = (id, novasOpcoes) => atualizarBloco(id, { opcoes: novasOpcoes });

  const atualizarEstilo = (campo, valor) => {
    setBlocos(prev =>
      prev.map(bloco =>
        bloco.id === selectedBlockId ? { ...bloco, estilo: { ...bloco.estilo, [campo]: valor } } : bloco
      )
    );
  };

  const removerBloco = (id) => {
    setBlocos(prev => prev.filter(bloco => bloco.id !== id));
    if (selectedBlockId === id) setSelectedBlockId(null);
  };

  // ---- helpers anexos ----
  const getAnexoBlocos = () =>
    blocos.filter(b => b.tipo === "anexo" && b.arquivo instanceof File);

  const uploadAnexos = async (pesquisaId) => {
    const anexos = getAnexoBlocos();
    if (anexos.length === 0) return { ok: 0, fail: 0 };

    const uploads = anexos.map(b => {
      const fd = new FormData();
      fd.append("anexo", b.arquivo, b.arquivo.name);
      return axios.post(`${API_BASE}/api/anexos/${pesquisaId}`, fd, {
        headers: { "Content-Type": "multipart/form-data" }
      });
    });

    const results = await Promise.allSettled(uploads);
    const ok = results.filter(r => r.status === "fulfilled").length;
    const fail = results.length - ok;
    return { ok, fail };
  };

  // ---------- montar VM + template ----------
  const montarPesquisaVM = () => {
    const loginId = parseInt(localStorage.getItem("userId"));
    const autor = autorNome;
    const dataCriacao = new Date().toLocaleDateString('pt-BR');

    const limparOpcoes = (ops = []) =>
      ops
        .map(o => (typeof o === 'string' ? o.trim() : o))
        .filter(o => Boolean(o) && (typeof o === 'string' ? o.trim().length > 0 : true));

    const perguntasDiscursivas = blocos
      .filter(b => b.tipo === "discursiva")
      .map(b => ({ titulo: b.texto, resposta: "" }));

    const perguntasObjetivas = blocos
      .filter(b => b.tipo === "objetiva")
      .map(b => ({
        titulo: b.texto,
        tipo: "objetiva",
        opcoes: limparOpcoes(b.opcoes).map(o => ({ opcao: o, respostaCerta: null }))
      }));

    const perguntasMultiplaEscolha = blocos
      .filter(b => b.tipo === "multipla")
      .map(b => ({
        titulo: b.texto,
        tipo: "multipla",
        opcoes: limparOpcoes(b.opcoes).map(o => ({ opcao: o, respostaCerta: null }))
      }));

    const blocosTemplate = blocos
      .filter(b => b.tipo !== "anexo")
      .map((b, i) => {
        const opcoesLimpa = limparOpcoes(b.opcoes);
        return {
          perguntaid: b.id ?? i,
          texto: b.texto,
          tipo: b.tipo, // "discursiva" | "objetiva" | "multipla"
          opcoes:
            b.tipo === "objetiva" || b.tipo === "multipla"
              ? opcoesLimpa.map((texto, idx) => ({ opcaoid: idx + 1, texto }))
              : [],
          estilo: b.estilo || { corFundo: "", corTexto: "", fonte: "" }
        };
      });

    return {
      titulo: dadosPesquisa.titulo,
      descricao: dadosPesquisa.descricao,
      tipoPesquisaId: parseInt(dadosPesquisa.tipo),
      codigoPesquisa: 0,
      loginId,
      autor,
      dataCriacao,
      perguntasDiscursivas,
      perguntasObjetivas,
      PerguntasMultiplaEscolha: perguntasMultiplaEscolha,
      templateJson: JSON.stringify(blocosTemplate)
    };
  };

  const salvarPesquisaComPerguntas = async () => {
    try {
      const pesquisaVM = montarPesquisaVM();
      const response = await axios.post(`${API_BASE}/api/pesquisas`, pesquisaVM);
      console.log('Resposta do backend ao salvar pesquisa:', response.data);
      // Tenta pegar o id de diferentes formas
      const id = response.data.pesquisaid || response.data.id || response.data.pesquisaid || (response.data.pesquisa && response.data.pesquisa.pesquisaid);
      if (!id) {
        toast.error("Não foi possível obter o ID da pesquisa criada. Verifique o backend.", { position: 'top-center', autoClose: 5000 });
        return;
      }
      // sobe anexos AGORA, vinculando à pesquisa criada
      const { ok, fail } = await uploadAnexos(id);

      const urlResposta = `${window.location.origin}/responder/${id}`;
      setQrUrl(urlResposta);

      if (fail === 0 && ok > 0) {
        toast.success(`Pesquisa e ${ok} anexo(s) enviados!`, { position: 'top-center', autoClose: 3000 });
      } else if (ok > 0 && fail > 0) {
        toast.warn(`Pesquisa salva. ${ok} anexo(s) enviados, ${fail} falharam.`, { position: 'top-center', autoClose: 5000 });
      } else if (ok === 0 && fail > 0) {
        toast.error(`Pesquisa salva, mas anexos falharam.`, { position: 'top-center', autoClose: 5000 });
      } else {
        toast.success("Pesquisa salva com sucesso!", { position: 'top-center', autoClose: 3000 });
      }
    } catch (erro) {
      console.error("Erro ao salvar pesquisa:", erro);
      toast.error("Ocorreu um erro ao salvar. Verifique os dados e tente novamente.", { position: 'top-center', autoClose: 4000 });
    }
  };
  // Função para mover bloco para cima ou para baixo
  const moverBloco = (id, direcao) => {
    setBlocos(prev => {
      const idx = prev.findIndex(b => b.id === id);
      if (idx === -1) return prev;
      // Só permite mover perguntas (discursiva, multipla, objetiva)
      const tipo = prev[idx].tipo;
      if (!["discursiva", "multipla", "objetiva"].includes(tipo)) return prev;
      const novo = [...prev];
      const novoIdx = direcao === "up" ? idx - 1 : idx + 1;
      // Só troca se o bloco alvo for também uma pergunta
      if (novoIdx < 0 || novoIdx >= novo.length) return prev;
      if (!["discursiva", "multipla", "objetiva"].includes(novo[novoIdx].tipo)) return prev;
      // Troca os blocos
      [novo[idx], novo[novoIdx]] = [novo[novoIdx], novo[idx]];
      return novo;
    });
  };


  const renderizarBloco = (bloco) => {
    const estilo = {
      backgroundColor: bloco.estilo.corFundo || 'white',
      color: bloco.estilo.corTexto || 'inherit',
      fontFamily: bloco.estilo.fonte || 'inherit'
    };

    const propsComuns = {
      bloco: { ...bloco, selecionado: bloco.id === selectedBlockId },
      onChangeTexto: atualizarTexto,
      onChangeOpcoes: atualizarOpcoes,
      onRemove: removerBloco,
      onClick: () => setSelectedBlockId(bloco.id),
      style: estilo
    };

    // Botões de mover para cima/baixo alinhados à direita
    const moveButtons = ["discursiva", "multipla", "objetiva"].includes(bloco.tipo) ? (
      <div style={{ display: "flex", flexDirection: "column", alignItems: "flex-end", gap: "0.5rem", marginLeft: "auto" }}>
        <button
          className={styles['btn-mover']}
          title="Mover para cima"
          onClick={(e) => { e.stopPropagation(); moverBloco(bloco.id, "up"); }}
          disabled={blocos.findIndex(b => b.id === bloco.id) === 0}
        >↑</button>
        <button
          className={styles['btn-mover']}
          title="Mover para baixo"
          onClick={(e) => { e.stopPropagation(); moverBloco(bloco.id, "down"); }}
          disabled={blocos.findIndex(b => b.id === bloco.id) === blocos.length - 1}
        >↓</button>
      </div>
    ) : null;

    switch (bloco.tipo) {
      case "discursiva":
        return (
          <div key={bloco.id} style={{}} onClick={() => setSelectedBlockId(bloco.id)}>
            <div style={{ display: "flex", alignItems: "flex-start" }}>
              <div style={{ flex: 1 }}>
                <Discursiva {...propsComuns} style={estilo} />
              </div>
              {moveButtons}
            </div>
          </div>
        );
      case "multipla":
        return (
          <div key={bloco.id} style={{}} onClick={() => setSelectedBlockId(bloco.id)}>
            <div style={{ display: "flex", alignItems: "flex-start" }}>
              <div style={{ flex: 1 }}>
                <MultiplaEscolha {...propsComuns} style={estilo} />
              </div>
              {moveButtons}
            </div>
          </div>
        );
      case "objetiva":
        return (
          <div key={bloco.id} style={{}} onClick={() => setSelectedBlockId(bloco.id)}>
            <div style={{ display: "flex", alignItems: "flex-start" }}>
              <div style={{ flex: 1 }}>
                <Objetiva {...propsComuns} style={estilo} />
              </div>
              {moveButtons}
            </div>
          </div>
        );
      case "anexo":
        return (
          <div
            key={bloco.id}
            className={`${styles['bloco-pergunta']} ${bloco.selecionado ? styles['selecionado'] : ''}`}
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
              <button className={styles['btn-remover']} onClick={() => removerBloco(bloco.id)} title="Remover anexo">
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

  const nomeTipoPesquisa = tiposPesquisa.find(tp => tp.tipopesquisaid === parseInt(dadosPesquisa.tipo))?.tipopesquisa1 || "—";

  return (
    <>
      <TopNavbar />
      <ToastContainer />
      <div className={styles['editor-container']}>
        <input
          type="file"
          ref={inputFileRef}
          style={{ display: "none" }}
          multiple
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

        <aside className={styles['sidebar-esquerda']}>
          <div className={styles['sidebar-scroll']}>
            <h3>Componentes</h3>
            <ul>
              <li className={styles['item-clique']} onClick={() => setIsPerguntasAberto(!isPerguntasAberto)}>
                {isPerguntasAberto ? <ChevronUp size={18} /> : <ChevronDown size={18} />} Perguntas
              </li>
              {isPerguntasAberto && (
                <ul className={styles['submenu']}>
                  <li className={styles['item-clique']} onClick={() => adicionarBloco("discursiva")}> <FileText size={18} /> Discursiva </li>
                  <li className={styles['item-clique']} onClick={() => adicionarBloco("multipla")}> <Circle size={18} /> Múltipla Escolha </li>
                  <li className={styles['item-clique']} onClick={() => adicionarBloco("objetiva")}> <Circle size={18} /> Objetiva (única) </li>
                </ul>
              )}
              <li className={styles['item-clique']} onClick={() => adicionarBloco("anexo")}> <Paperclip size={18} /> Anexo </li>
            </ul>
          </div>
          <div className={styles['salvar-wrapper']}>
            <button className={styles['salvar-btn']} style={{ marginBottom: '8px' }} onClick={() => setMostrarModalPreview(true)}>
              Preview da Pesquisa
            </button>
            <ModalPreviewPesquisa
              isOpen={mostrarModalPreview}
              onClose={() => setMostrarModalPreview(false)}
              dadosPesquisa={dadosPesquisa}
              blocos={blocos}
              nomeTipoPesquisa={nomeTipoPesquisa}
              autorNome={autorNome}
            />
            <button className={styles['salvar-btn']} onClick={salvarPesquisaComPerguntas}>
              <Save size={18} style={{ marginRight: "6px" }} /> Salvar Pesquisa
            </button>
            {qrUrl && (
              <>
                <button className={styles['salvar-btn']} onClick={() => setMostrarModalQr(true)}>
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

        <main className={styles['area-construcao']}>
          <CabecalhoPesquisa
            autor={autorNome}
            data={(() => {
              if (dadosPesquisa && dadosPesquisa.dataCriacao) return dadosPesquisa.dataCriacao;
              const hoje = new Date();
              return hoje.toLocaleDateString('pt-BR');
            })()}
            selecionado={false}
            style={{
              background: "linear-gradient(90deg, #f8f9fd 0%, #eef2fa 100%)",
              border: "1.5px solid #e0e7ef",
              borderRadius: "14px",
              boxShadow: "0 2px 12px rgba(44, 62, 80, 0.06)",
              marginBottom: "2rem",
              padding: "1.8rem 2.2rem",
              fontFamily: "'Segoe UI', 'Inter', Arial, sans-serif"
            }}
          />
          <InformacoesPesquisa
            titulo={dadosPesquisa.titulo}
            descricao={dadosPesquisa.descricao}
            tipoPesquisa={nomeTipoPesquisa}
          />

          <div className={`${styles['area-preview']} ${blocos.length > 0 ? styles['invisivel'] : ''}`}>
            <p>Adicione blocos para montar sua pesquisa</p>
          </div>

          {blocos.length > 0 && blocos.map(renderizarBloco)}
        </main>

        <aside className={styles['sidebar-direita']}>
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