import React, { useState, useRef, useEffect } from "react";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
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
  Image as ImageIcon,
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

  // input oculto para anexos "da pesquisa" (bloco Anexo)
  const inputFileRef = useRef();

  // inputs ocultos para anexos/IMAGENS por PERGUNTA
  const inputFilePerguntaRef = useRef(null);
  const inputImagemPerguntaRef = useRef(null);
  const [attachToBlockId, setAttachToBlockId] = useState(null);
  const [imageToBlockId, setImageToBlockId] = useState(null);

  const [dadosPesquisa, setDadosPesquisa] = useState({ titulo: "", descricao: "", tipo: "" });
  const [blocos, setBlocos] = useState([]);
  const [autorNome, setAutorNome] = useState("");
  const [tiposPesquisa, setTiposPesquisa] = useState([]);

  useEffect(() => {
    const loginId = localStorage.getItem("userId");
    if (loginId) {
      axios
        .get(`${API_BASE}/api/login/SelecionarLoginPorId/${loginId}`)
        .then((res) => setAutorNome(res.data.usuario || "Usuário"))
        .catch(() => setAutorNome("Usuário"));
    }
  }, []);

  useEffect(() => {
    axios
      .get(`${API_BASE}/api/tipopesquisa/ListarTipoPesquisa`)
      .then((res) => setTiposPesquisa(res.data))
      .catch(() => setTiposPesquisa([]));
  }, []);

  const adicionarBloco = (tipo) => {
    const novo = {
      id: Date.now(),
      tipo,
      texto: "",
      opcoes: tipo === "multipla" || tipo === "objetiva" ? [""] : [],
      arquivo: null, // usado no bloco "anexo" da PESQUISA
      attachments: [], // arquivos anexados à PERGUNTA (pdf, doc, etc)
      imagens: [], // { file, previewUrl } por PERGUNTA
      estilo: { corFundo: "", corTexto: "", fonte: "" }
    };

    if (tipo === "anexo") {
      inputFileRef.current?.click();
    } else {
      setBlocos((prev) => [...prev, novo]);
    }
  };

  // anexos da PESQUISA (bloco "anexo")
  const handleArquivoSelecionado = (event) => {
    const files = Array.from(event.target.files || []);
    if (files.length === 0) return;

    const novos = files.map((file, i) => ({
      id: Date.now() + i,
      tipo: "anexo",
      arquivo: file,
      estilo: { corFundo: "", corTexto: "", fonte: "" }
    }));
    setBlocos((prev) => [...prev, ...novos]);

    event.target.value = "";
  };

  // anexos por PERGUNTA (qualquer arquivo)
  const abrirFilePickerPergunta = (blockId) => {
    setAttachToBlockId(blockId);
    inputFilePerguntaRef.current?.click();
  };
  const handleArquivoPerguntaSelecionado = (e) => {
    const files = Array.from(e.target.files || []);
    if (attachToBlockId && files.length > 0) {
      setBlocos((prev) =>
        prev.map((b) => (b.id === attachToBlockId ? { ...b, attachments: [...(b.attachments || []), ...files] } : b))
      );
    }
    setAttachToBlockId(null);
    e.target.value = "";
  };

  // imagens por PERGUNTA (com preview)
  const abrirImagemPickerPergunta = (blockId) => {
    setImageToBlockId(blockId);
    inputImagemPerguntaRef.current?.click();
  };
  const handleImagemPerguntaSelecionada = (e) => {
    const files = Array.from(e.target.files || []);
    if (imageToBlockId && files.length > 0) {
      const imagens = files
        .filter((f) => f.type?.startsWith("image/"))
        .map((f) => ({ file: f, previewUrl: URL.createObjectURL(f) }));

      if (imagens.length > 0) {
        setBlocos((prev) =>
          prev.map((b) => (b.id === imageToBlockId ? { ...b, imagens: [...(b.imagens || []), ...imagens] } : b))
        );
      }
    }
    setImageToBlockId(null);
    e.target.value = "";
  };

  const atualizarBloco = (id, dados) => {
    setBlocos((prev) => prev.map((bloco) => (bloco.id === id ? { ...bloco, ...dados } : bloco)));
  };
  const atualizarTexto = (id, novoTexto) => atualizarBloco(id, { texto: novoTexto });
  const atualizarOpcoes = (id, novasOpcoes) => atualizarBloco(id, { opcoes: novasOpcoes });

  const atualizarEstilo = (campo, valor) => {
    setBlocos((prev) =>
      prev.map((bloco) =>
        bloco.id === selectedBlockId ? { ...bloco, estilo: { ...bloco.estilo, [campo]: valor } } : bloco
      )
    );
  };

  const removerBloco = (id) => {
    setBlocos((prev) => prev.filter((bloco) => bloco.id !== id));
    if (selectedBlockId === id) setSelectedBlockId(null);
  };

  // remove 1 imagem de uma pergunta pelo índice
  const removerImagemPergunta = (blockId, imgIndex) => {
    setBlocos((prev) =>
      prev.map((b) => (b.id === blockId ? { ...b, imagens: (b.imagens || []).filter((_, i) => i !== imgIndex) } : b))
    );
  };

  // ---- uploads ----
  const getAnexoBlocos = () => blocos.filter((b) => b.tipo === "anexo" && b.arquivo instanceof File);

  const uploadAnexosPesquisa = async (pesquisaId) => {
    const anexos = getAnexoBlocos();
    if (anexos.length === 0) return { ok: 0, fail: 0 };

    const uploads = anexos.map((b) => {
      const fd = new FormData();
      fd.append("anexo", b.arquivo, b.arquivo.name);
      return axios.post(`${API_BASE}/api/Anexos/${pesquisaId}`, fd);
    });

    const results = await Promise.allSettled(uploads);
    const ok = results.filter((r) => r.status === "fulfilled").length;
    const fail = results.length - ok;
    return { ok, fail };
  };

  // pega perguntas da pesquisa (ajuste a rota se a sua for diferente)
  const fetchPerguntasDaPesquisa = async (pesquisaId) => {
    const resp = await axios.get(`${API_BASE}/api/pesquisas/BuscarPesquisaPorId/${pesquisaId}`);
    return resp.data?.perguntas ?? [];
  };

  const mapBlocosToPerguntaIds = (perguntas) => {
    const blocosPerguntas = blocos.filter((b) => ["discursiva", "objetiva", "multipla"].includes(b.tipo));
    const map = new Map();
    blocosPerguntas.forEach((b, i) => {
      const pid = perguntas[i]?.perguntaid;
      if (pid) map.set(b.id, pid);
    });
    return map;
  };

  const uploadAnexosPergunta = async (pesquisaId, blocoToPerguntaId) => {
    const blocosPerguntas = blocos.filter((b) => ["discursiva", "objetiva", "multipla"].includes(b.tipo));
    const uploads = [];

    for (const b of blocosPerguntas) {
      const files = [
        ...(Array.isArray(b.attachments) ? b.attachments : []),
        ...(Array.isArray(b.imagens) ? b.imagens.map((i) => i.file) : [])
      ];
      if (files.length === 0) continue;

      const perguntaId = blocoToPerguntaId.get(b.id);
      if (!perguntaId) continue;

      for (const file of files) {
        const fd = new FormData();
        fd.append("anexo", file, file.name);
        fd.append("PerguntaId", String(perguntaId));
        uploads.push(axios.post(`${API_BASE}/api/Anexos/${pesquisaId}`, fd));
      }
    }

    if (uploads.length === 0) return { ok: 0, fail: 0 };

    const results = await Promise.allSettled(uploads);
    const ok = results.filter((r) => r.status === "fulfilled").length;
    const fail = results.length - ok;
    return { ok, fail };
  };

  // ---------- montar VM + template ----------
  const montarPesquisaVM = () => {
    const loginId = parseInt(localStorage.getItem("userId"));
    const autor = autorNome;
    const dataCriacao = new Date().toLocaleDateString("pt-BR");

    const limparOpcoes = (ops = []) =>
      ops
        .map((o) => (typeof o === "string" ? o.trim() : o))
        .filter((o) => Boolean(o) && (typeof o === "string" ? o.trim().length > 0 : true));

    const perguntasDiscursivas = blocos
      .filter((b) => b.tipo === "discursiva")
      .map((b) => ({ titulo: b.texto, resposta: "" }));

    const perguntasObjetivas = blocos
      .filter((b) => b.tipo === "objetiva")
      .map((b) => ({
        titulo: b.texto,
        tipo: "objetiva",
        opcoes: limparOpcoes(b.opcoes).map((o) => ({ opcao: o, respostaCerta: null }))
      }));

    const perguntasMultiplaEscolha = blocos
      .filter((b) => b.tipo === "multipla")
      .map((b) => ({
        titulo: b.texto,
        tipo: "multipla",
        opcoes: limparOpcoes(b.opcoes).map((o) => ({ opcao: o, respostaCerta: null }))
      }));

    const blocosTemplate = blocos
      .filter((b) => b.tipo !== "anexo")
      .map((b, i) => {
        const opcoesLimpa = limparOpcoes(b.opcoes);
        return {
          perguntaid: b.id ?? i,
          texto: b.texto,
          tipo: b.tipo,
          opcoes:
            b.tipo === "objetiva" || b.tipo === "multipla"
              ? opcoesLimpa.map((texto, idx) => ({ opcaoid: idx + 1, texto }))
              : [],
          estilo: b.estilo || { corFundo: "", corTexto: "", fonte: "" },
          imagens: (b.imagens || []).map((img, idx) => ({
            idx,
            nome: img.file?.name
          }))
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
      const id = response.data?.pesquisaid ?? response.data?.pesquisa?.pesquisaid;
      if (!id) {
        toast.error("Não foi possível obter o ID da pesquisa criada.", { position: "top-center", autoClose: 5000 });
        return;
      }

      const r1 = await uploadAnexosPesquisa(id);

      const perguntas = await fetchPerguntasDaPesquisa(id);
      const mapa = mapBlocosToPerguntaIds(perguntas);
      const r2 = await uploadAnexosPergunta(id, mapa);

      const urlResposta = `${window.location.origin}/responder/${id}`;
      setQrUrl(urlResposta);

      const totalOk = (r1.ok || 0) + (r2.ok || 0);
      const totalFail = (r1.fail || 0) + (r2.fail || 0);

      if (totalFail === 0 && totalOk > 0) {
        toast.success(`Pesquisa e ${totalOk} anexo(s)/imagem(ns) enviados!`, {
          position: "top-center",
          autoClose: 3000
        });
      } else if (totalOk > 0 && totalFail > 0) {
        toast.warn(`Pesquisa salva. ${totalOk} enviados, ${totalFail} falharam.`, {
          position: "top-center",
          autoClose: 5000
        });
      } else if (totalOk === 0 && totalFail > 0) {
        toast.error(`Pesquisa salva, mas anexos/imagens falharam.`, { position: "top-center", autoClose: 5000 });
      } else {
        toast.success("Pesquisa salva com sucesso!", { position: "top-center", autoClose: 3000 });
      }
    } catch (erro) {
      console.error("Erro ao salvar pesquisa:", erro);
      toast.error("Ocorreu um erro ao salvar. Verifique os dados e tente novamente.", {
        position: "top-center",
        autoClose: 4000
      });
    }
  };

  const moverBloco = (id, direcao) => {
    setBlocos((prev) => {
      const idx = prev.findIndex((b) => b.id === id);
      if (idx === -1) return prev;
      const tipo = prev[idx].tipo;
      if (!["discursiva", "multipla", "objetiva"].includes(tipo)) return prev;
      const novo = [...prev];
      const novoIdx = direcao === "up" ? idx - 1 : idx + 1;
      if (novoIdx < 0 || novoIdx >= novo.length) return prev;
      if (!["discursiva", "multipla", "objetiva"].includes(novo[novoIdx].tipo)) return prev;
      [novo[idx], novo[novoIdx]] = [novo[novoIdx], novo[idx]];
      return novo;
    });
  };

  // header de cada pergunta com botões clip (anexo) + imagem + lixeira
  const renderHeaderPergunta = (titulo, bloco) => (
    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 8 }}>
      <h4 style={{ margin: 0 }}>{titulo}</h4>
      <div style={{ display: "flex", gap: 8 }}>
        <button
          type="button"
          className={styles["btn-remover"]}
          title="Adicionar anexo à pergunta"
          onClick={(e) => {
            e.stopPropagation();
            abrirFilePickerPergunta(bloco.id);
          }}
        >
          <Paperclip size={18} />
        </button>
        <button
          type="button"
          className={styles["btn-remover"]}
          title="Adicionar imagem à pergunta"
          onClick={(e) => {
            e.stopPropagation();
            abrirImagemPickerPergunta(bloco.id);
          }}
        >
          <ImageIcon size={18} />
        </button>
        <button
          type="button"
          className={styles["btn-remover"]}
          onClick={(e) => {
            e.stopPropagation();
            removerBloco(bloco.id);
          }}
          title="Remover pergunta"
        >
          <Trash2 size={18} />
        </button>
      </div>
    </div>
  );

  const renderPreviewAnexosPergunta = (bloco) =>
    Array.isArray(bloco.attachments) &&
    bloco.attachments.length > 0 && (
      <ul style={{ marginTop: 8 }}>
        {bloco.attachments.map((f, i) => (
          <li key={i} style={{ fontSize: 12, opacity: 0.8 }}>
            {f.name}
          </li>
        ))}
      </ul>
    );

  const renderizarBloco = (bloco) => {
    const estilo = {
      backgroundColor: bloco.estilo.corFundo || "white",
      color: bloco.estilo.corTexto || "inherit",
      fontFamily: bloco.estilo.fonte || "inherit"
    };

    const propsComuns = {
      bloco: { ...bloco, selecionado: bloco.id === selectedBlockId, imagens: bloco.imagens },
      onChangeTexto: atualizarTexto,
      onChangeOpcoes: atualizarOpcoes,
      onRemove: removerBloco,
      onRemoveImagem: removerImagemPergunta, // NOVO
      onClick: () => setSelectedBlockId(bloco.id),
      style: estilo
    };

    const moveButtons = ["discursiva", "multipla", "objetiva"].includes(bloco.tipo) ? (
      <div style={{ display: "flex", flexDirection: "column", alignItems: "flex-end", gap: "0.5rem", marginLeft: "auto" }}>
        <button
          className={styles["btn-mover"]}
          title="Mover para cima"
          onClick={(e) => {
            e.stopPropagation();
            moverBloco(bloco.id, "up");
          }}
          disabled={blocos.findIndex((b) => b.id === bloco.id) === 0}
        >
          ↑
        </button>
        <button
          className={styles["btn-mover"]}
          title="Mover para baixo"
          onClick={(e) => {
            e.stopPropagation();
            moverBloco(bloco.id, "down");
          }}
          disabled={blocos.findIndex((b) => b.id === bloco.id) === blocos.length - 1}
        >
          ↓
        </button>
      </div>
    ) : null;

    switch (bloco.tipo) {
      case "discursiva":
        return (
          <div key={bloco.id} onClick={() => setSelectedBlockId(bloco.id)}>
            <div style={{ display: "flex", alignItems: "flex-start" }}>
              <div style={{ flex: 1 }}>
                {renderHeaderPergunta("Pergunta Discursiva", bloco)}
                <Discursiva {...propsComuns} style={estilo} />
                {renderPreviewAnexosPergunta(bloco)}
              </div>
              {moveButtons}
            </div>
          </div>
        );
      case "multipla":
        return (
          <div key={bloco.id} onClick={() => setSelectedBlockId(bloco.id)}>
            <div style={{ display: "flex", alignItems: "flex-start" }}>
              <div style={{ flex: 1 }}>
                {renderHeaderPergunta("Pergunta Múltipla Escolha", bloco)}
                <MultiplaEscolha {...propsComuns} style={estilo} />
                {renderPreviewAnexosPergunta(bloco)}
              </div>
              {moveButtons}
            </div>
          </div>
        );
      case "objetiva":
        return (
          <div key={bloco.id} onClick={() => setSelectedBlockId(bloco.id)}>
            <div style={{ display: "flex", alignItems: "flex-start" }}>
              <div style={{ flex: 1 }}>
                {renderHeaderPergunta("Pergunta Objetiva (única)", bloco)}
                <Objetiva {...propsComuns} style={estilo} />
                {renderPreviewAnexosPergunta(bloco)}
              </div>
              {moveButtons}
            </div>
          </div>
        );
      case "anexo":
        return (
          <div
            key={bloco.id}
            className={`${styles["bloco-pergunta"]} ${bloco.selecionado ? styles["selecionado"] : ""}`}
            style={{
              ...estilo,
              padding: "1rem",
              border: "1px solid #eee",
              borderRadius: "10px",
              backgroundColor: "#fff",
              marginBottom: "1rem"
            }}
            onClick={() => setSelectedBlockId(bloco.id)}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <h4 style={{ margin: 0 }}>Arquivo Anexo</h4>
              <button className={styles["btn-remover"]} onClick={() => removerBloco(bloco.id)} title="Remover anexo">
                <Trash2 size={18} />
              </button>
            </div>
            <p>
              <strong>Arquivo:</strong> {bloco.arquivo?.name || "Nenhum arquivo selecionado"}
            </p>
          </div>
        );
      default:
        return null;
    }
  };

  const nomeTipoPesquisa =
    tiposPesquisa.find((tp) => tp.tipopesquisaid === parseInt(dadosPesquisa.tipo))?.tipopesquisa1 ||
    tiposPesquisa.find((tp) => tp.tipopesquisaid === parseInt(dadosPesquisa.tipo))?.tipopesquisa ||
    "—";

  return (
    <>
      <TopNavbar />
      <ToastContainer />
      <div className={styles["editor-container"]}>
        {/* inputs ocultos */}
        <input type="file" ref={inputFileRef} style={{ display: "none" }} multiple onChange={handleArquivoSelecionado} />
        <input type="file" ref={inputFilePerguntaRef} style={{ display: "none" }} multiple onChange={handleArquivoPerguntaSelecionado} />
        <input type="file" ref={inputImagemPerguntaRef} style={{ display: "none" }} multiple accept="image/*" onChange={handleImagemPerguntaSelecionada} />

        {showModal && <ModalCriarPesquisa onConfirm={(dados) => { setDadosPesquisa(dados); setShowModal(false); }} />}

        <aside className={styles["sidebar-esquerda"]}>
          <div className={styles["sidebar-scroll"]}>
            <h3>Componentes</h3>
            <ul>
              <li className={styles["item-clique"]} onClick={() => setIsPerguntasAberto(!isPerguntasAberto)}>
                {isPerguntasAberto ? <ChevronUp size={18} /> : <ChevronDown size={18} />} Perguntas
              </li>
              {isPerguntasAberto && (
                <ul className={styles["submenu"]}>
                  <li className={styles["item-clique"]} onClick={() => adicionarBloco("discursiva")}>
                    <FileText size={18} /> Discursiva
                  </li>
                  <li className={styles["item-clique"]} onClick={() => adicionarBloco("multipla")}>
                    <Circle size={18} /> Múltipla Escolha
                  </li>
                  <li className={styles["item-clique"]} onClick={() => adicionarBloco("objetiva")}>
                    <Circle size={18} /> Objetiva (única)
                  </li>
                </ul>
              )}
              <li className={styles["item-clique"]} onClick={() => adicionarBloco("anexo")}>
                <Paperclip size={18} /> Anexo (da pesquisa)
              </li>
            </ul>
          </div>
          <div className={styles["salvar-wrapper"]}>
            <button className={styles["salvar-btn"]} style={{ marginBottom: "8px" }} onClick={() => setMostrarModalPreview(true)}>
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
            <button className={styles["salvar-btn"]} onClick={salvarPesquisaComPerguntas}>
              <Save size={18} style={{ marginRight: "6px" }} /> Salvar Pesquisa
            </button>
            {qrUrl && (
              <>
                <button className={styles["salvar-btn"]} onClick={() => setMostrarModalQr(true)}>
                  Visualizar QR Code
                </button>
                <ModalQRCode isOpen={mostrarModalQr} onClose={() => setMostrarModalQr(false)} qrUrl={qrUrl} />
              </>
            )}
          </div>
        </aside>

        <main className={styles["area-construcao"]}>
          <CabecalhoPesquisa
            autor={autorNome}
            data={(() => {
              if (dadosPesquisa && dadosPesquisa.dataCriacao) return dadosPesquisa.dataCriacao;
              const hoje = new Date();
              return hoje.toLocaleDateString("pt-BR");
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

          <div className={`${styles["area-preview"]} ${blocos.length > 0 ? styles["invisivel"] : ""}`}>
            <p>Adicione blocos para montar sua pesquisa</p>
          </div>

          {blocos.length > 0 && blocos.map(renderizarBloco)}
        </main>

        <aside className={styles["sidebar-direita"]}>
          <h3>Estilo</h3>
          <label>
            {" "}
            <Paintbrush2 size={18} /> Cor de Fundo
            <input type="color" onChange={(e) => atualizarEstilo("corFundo", e.target.value)} disabled={!selectedBlockId} />
          </label>
          <label>
            {" "}
            <Droplet size={18} /> Cor do Texto
            <input type="color" onChange={(e) => atualizarEstilo("corTexto", e.target.value)} disabled={!selectedBlockId} />
          </label>
          <label>
            {" "}
            <Type size={18} /> Fonte
            <select onChange={(e) => atualizarEstilo("fonte", e.target.value)} disabled={!selectedBlockId}>
              <option value="">Padrão</option>
              <option style={{ fontFamily: "Arial" }} value="Arial">
                Arial
              </option>
              <option style={{ fontFamily: "Helvetica" }} value="Helvetica">
                Helvetica
              </option>
              <option style={{ fontFamily: "Verdana" }} value="Verdana">
                Verdana
              </option>
              <option style={{ fontFamily: "Tahoma" }} value="Tahoma">
                Tahoma
              </option>
              <option style={{ fontFamily: "Trebuchet MS" }} value="Trebuchet MS">
                Trebuchet MS
              </option>
              <option style={{ fontFamily: "Georgia" }} value="Georgia">
                Georgia
              </option>
              <option style={{ fontFamily: "Times New Roman" }} value="Times New Roman">
                Times New Roman
              </option>
              <option style={{ fontFamily: "Courier New" }} value="Courier New">
                Courier New
              </option>
              <option style={{ fontFamily: "Lucida Console" }} value="Lucida Console">
                Lucida Console
              </option>
              <option style={{ fontFamily: "Impact" }} value="Impact">
                Impact
              </option>
              <option style={{ fontFamily: "Palatino Linotype" }} value="Palatino Linotype">
                Palatino Linotype
              </option>
              <option style={{ fontFamily: "Segoe UI" }} value="Segoe UI">
                Segoe UI
              </option>
              <option style={{ fontFamily: "Cambria" }} value="Cambria">
                Cambria
              </option>
              <option style={{ fontFamily: "Garamond" }} value="Garamond">
                Garamond
              </option>
              <option style={{ fontFamily: "Franklin Gothic Medium" }} value="Franklin Gothic Medium">
                Franklin Gothic Medium
              </option>
              <option style={{ fontFamily: "Brush Script MT" }} value="Brush Script MT">
                Brush Script MT
              </option>
              <option style={{ fontFamily: "Comic Sans MS" }} value="Comic Sans MS">
                Comic Sans MS
              </option>
              <option style={{ fontFamily: "Copperplate" }} value="Copperplate">
                Copperplate
              </option>
              <option style={{ fontFamily: "Fira Sans" }} value="Fira Sans">
                Fira Sans
              </option>
            </select>
          </label>
        </aside>
      </div>
    </>
  );
};

export default CriarPesquisa;
