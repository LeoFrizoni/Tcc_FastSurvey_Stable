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

  // área com scroll (fix do “salto”)
  const scrollRef = useRef(null);

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
        .get(`${API_BASE}/api/login/${loginId}`)
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
    const base = {
      id: Date.now(),
      tipo,
      texto: "",
      opcoes: [],
      arquivo: null,
      attachments: [],
      imagens: [],
      estilo: { corFundo: "", corTexto: "", fonte: "" },
    };

    if (tipo === "discursiva") {
      setBlocos((prev) => [...prev, { ...base, opcoes: [], respostaExemplo: "" }]);
      return;
    }
    if (tipo === "multipla") {
      setBlocos((prev) => [
        ...prev,
        { ...base, opcoes: [""], temGabarito: false, permitirMultiplaSelecao: true, corretas: [] },
      ]);
      return;
    }
    if (tipo === "objetiva") {
      setBlocos((prev) => [...prev, { ...base, opcoes: [""], temGabarito: false, corretaIndex: null }]);
      return;
    }
    if (tipo === "anexo") {
      inputFileRef.current?.click();
      return;
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
        prev.map((b) =>
          b.id === attachToBlockId
            ? { ...b, attachments: [...(b.attachments || []), ...files] }
            : b
        )
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
          prev.map((b) =>
            b.id === imageToBlockId
              ? { ...b, imagens: [...(b.imagens || []), ...imagens] }
              : b
          )
        );
      }
    }
    setImageToBlockId(null);
    e.target.value = "";
  };

  const atualizarBloco = (id, dados) => {
    setBlocos((prev) =>
      prev.map((bloco) => (bloco.id === id ? { ...bloco, ...dados } : bloco))
    );
  };
  const atualizarTexto = (id, novoTexto) => atualizarBloco(id, { texto: novoTexto });
  const atualizarOpcoes = (id, novasOpcoes) => atualizarBloco(id, { opcoes: novasOpcoes });

  // NOVOS: flags e gabaritos
  const toggleGabarito = (id, val) => atualizarBloco(id, { temGabarito: !!val });
  const togglePermiteMultipla = (id, val) =>
    atualizarBloco(id, { permitirMultiplaSelecao: !!val, ...(val ? {} : { corretas: [] }) });

  const setCorretaIndex = (id, idx) => atualizarBloco(id, { corretaIndex: idx });

  const toggleCorretaMultipla = (id, idx) => {
    setBlocos((prev) =>
      prev.map((b) => {
        if (b.id !== id) return b;
        const atual = Array.isArray(b.corretas) ? [...b.corretas] : [];
        if (b.permitirMultiplaSelecao) {
          const tem = atual.includes(idx);
          const novo = tem ? atual.filter((i) => i !== idx) : [...atual, idx];
          return { ...b, corretas: novo };
        } else {
          // se não permite múltipla, fica somente uma correta
          return { ...b, corretas: [idx] };
        }
      })
    );
  };

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
      prev.map((b) =>
        b.id === blockId
          ? { ...b, imagens: (b.imagens || []).filter((_, i) => i !== imgIndex) }
          : b
      )
    );
  };

  // ---------- uploads ----------
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

  const fetchPerguntasDaPesquisa = async (pesquisaId) => {
    const resp = await axios.get(`${API_BASE}/api/pesquisas/BuscarPesquisaPorId/${pesquisaId}`);
    // Espera-se algo como { perguntas: [...] }
    return resp.data?.perguntas ?? [];
  };

  const fetchOpcoesDaPergunta = async (perguntaId) => {
    try {
      const resp = await axios.get(`${API_BASE}/api/OpcoesPergunta/Pergunta/${perguntaId}`);
      return Array.isArray(resp.data) ? resp.data : [];
    } catch {
      return [];
    }
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
  const dataCriacao = new Date();

    const limparOpcoes = (ops = []) =>
      ops
        .map((o) => (typeof o === "string" ? o.trim() : o))
        .filter((o) => Boolean(o) && (typeof o === "string" ? o.trim().length > 0 : true));

    const perguntasDiscursivas = blocos
      .filter((b) => b.tipo === "discursiva")
      .map((b) => ({ titulo: b.texto, resposta: "" }));

    const perguntasObjetivas = blocos
      .filter((b) => b.tipo === "objetiva")
      .map((b) => {
        const ops = limparOpcoes(b.opcoes);
        return {
          titulo: b.texto,
          tipo: "objetiva",
          temGabarito: !!b.temGabarito,
          permitirMultiplaSelecao: false,
          opcoes: ops.map((o, idx) => ({
            opcao: o,
            correta: !!b.temGabarito && b.corretaIndex === idx
          }))
        };
      });

    const perguntasMultiplaEscolha = blocos
      .filter((b) => b.tipo === "multipla")
      .map((b) => {
        const ops = limparOpcoes(b.opcoes);
        const corretas = Array.isArray(b.corretas) ? b.corretas : [];
        return {
          titulo: b.texto,
          tipo: "multipla",
          temGabarito: !!b.temGabarito,
          permitirMultiplaSelecao: !!b.permitirMultiplaSelecao,
          opcoes: ops.map((o, idx) => ({
            opcao: o,
            correta: !!b.temGabarito && corretas.includes(idx)
          }))
        };
      });

    // JSON de template para refletir no front (Responder/Preview)
    const blocosTemplate = blocos
      .filter((b) => b.tipo !== "anexo")
      .map((b, i) => {
        const opcoesLimpa = limparOpcoes(b.opcoes);
        const base = {
          perguntaid: b.id ?? i,
          texto: b.texto,
          tipo: b.tipo,
          estilo: b.estilo || { corFundo: "", corTexto: "", fonte: "" },
          imagens: (b.imagens || []).map((img, idx) => ({
            idx,
            nome: img.file?.name
          }))
        };

        if (b.tipo === "objetiva") {
          return {
            ...base,
            temGabarito: !!b.temGabarito,
            corretaIndex: b.corretaIndex ?? null,
            opcoes: opcoesLimpa.map((texto, idx) => ({
              opcaoid: idx + 1,
              texto,
              correta: !!b.temGabarito && b.corretaIndex === idx
            }))
          };
        }

        if (b.tipo === "multipla") {
          const corretas = Array.isArray(b.corretas) ? b.corretas : [];
          return {
            ...base,
            temGabarito: !!b.temGabarito,
            permitirMultiplaSelecao: !!b.permitirMultiplaSelecao,
            opcoes: opcoesLimpa.map((texto, idx) => ({
              opcaoid: idx + 1,
              texto,
              correta: !!b.temGabarito && corretas.includes(idx)
            }))
          };
        }

        if (b.tipo === "discursiva") {
          return {
            ...base,
            respostaExemplo: b.respostaExemplo || ""
          };
        }

        return { ...base, opcoes: [] };
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

  // ---------- GABARITO: aplicar no backend após criar a pesquisa ----------
  const aplicarGabaritosNoBackend = async (perguntas, blocoToPerguntaId) => {
    // Vamos iterar somente blocos de pergunta
    const blocosPerguntas = blocos.filter((b) =>
      ["discursiva", "objetiva", "multipla"].includes(b.tipo)
    );

    for (const b of blocosPerguntas) {
      const perguntaId = blocoToPerguntaId.get(b.id);
      if (!perguntaId) continue;

      // Apenas objetiva e múltipla possuem gabarito
      if (b.tipo !== "objetiva" && b.tipo !== "multipla") continue;

      // Buscar opções da pergunta para mapear índices → ids
      const opcoes = await fetchOpcoesDaPergunta(perguntaId);
      if (!Array.isArray(opcoes) || opcoes.length === 0) continue;

      // Montar DTO a partir do bloco
      const dto = {
        temGabarito: !!b.temGabarito,
        permiteMultiplaSelecao: b.tipo === "multipla" ? !!b.permitirMultiplaSelecao : false,
        opcoesCorretas: []
      };

      if (dto.temGabarito) {
        if (b.tipo === "objetiva" && typeof b.corretaIndex === "number") {
          const alvo = opcoes[b.corretaIndex];
          if (alvo?.opcaoid) dto.opcoesCorretas = [alvo.opcaoid];
        }
        if (b.tipo === "multipla" && Array.isArray(b.corretas)) {
          dto.opcoesCorretas = b.corretas
            .map((idx) => opcoes[idx]?.opcaoid)
            .filter(Boolean);
        }
      }

      try {
        await axios.put(`${API_BASE}/api/Perguntas/${perguntaId}/gabarito`, dto);
      } catch (e) {
        console.error("Falha ao definir gabarito da pergunta", perguntaId, e);
        toast.warn(`Não foi possível definir gabarito de uma pergunta (#${perguntaId}).`, {
          position: "top-center",
          autoClose: 4000
        });
      }
    }
  };

  // ---------- salvar ----------
  const salvarPesquisaComPerguntas = async () => {
    try {
      const pesquisaVM = montarPesquisaVM();
      const response = await axios.post(`${API_BASE}/api/pesquisas`, pesquisaVM);
      const id = response.data?.pesquisaid ?? response.data?.pesquisa?.pesquisaid;
      if (!id) {
        toast.error("Não foi possível obter o ID da pesquisa criada.", { position: "top-center", autoClose: 5000 });
        return;
      }

      // 1) Upload dos anexos da pesquisa
      const r1 = await uploadAnexosPesquisa(id);

      // 2) Buscar perguntas criadas pelo backend
      const perguntas = await fetchPerguntasDaPesquisa(id);

      // 3) Mapear blocos → perguntaId pela ordem
      const mapa = mapBlocosToPerguntaIds(perguntas);

      // 4) Aplicar gabaritos (PUT Perguntas/{id}/gabarito)
      await aplicarGabaritosNoBackend(perguntas, mapa);

      // 5) Upload de anexos/imagens por pergunta
      const r2 = await uploadAnexosPergunta(id, mapa);

      // QR + feedback
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

  // ---------- mover com scroll estável ----------
  const moverBloco = (id, direcao) => {
    // mede posição visual do card ANTES da troca
    const cardEl = document.getElementById(`bloco-${id}`);
    const container = scrollRef.current; // <main> com overflow
    const beforeTop = cardEl ? cardEl.getBoundingClientRect().top : null;

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

      // compensa o scroll no próximo frame
      if (beforeTop !== null && container) {
        requestAnimationFrame(() => {
          const afterEl = document.getElementById(`bloco-${id}`);
          if (!afterEl) return;
          const afterTop = afterEl.getBoundingClientRect().top;
          const delta = afterTop - beforeTop;
          container.scrollTop += delta;
        });
      }

      return novo;
    });
  };

  // header de cada pergunta com botões (anexo + imagem + lixeira)
  const renderHeaderPergunta = (titulo, bloco) => (
    <div className={styles["card-header"]}>
      <h4 className={styles["card-title"]}>{titulo}</h4>
      <div className={styles["toolbarRight"]} onClick={(e) => e.stopPropagation()}>
        <button
          type="button"
          className={styles["iconBtn"]}
          title="Adicionar anexo à pergunta"
          onClick={() => abrirFilePickerPergunta(bloco.id)}
        >
          <Paperclip size={26} />
        </button>
        <button
          type="button"
          className={styles["iconBtn"]}
          title="Adicionar imagem à pergunta"
          onClick={() => abrirImagemPickerPergunta(bloco.id)}
        >
          <ImageIcon size={26} />
        </button>
        <button
          type="button"
          className={`${styles["iconBtn"]} ${styles["danger"]}`}
          title="Remover pergunta"
          onClick={() => removerBloco(bloco.id)}
        >
          <Trash2 size={26} />
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

  // régua de ordenação dentro do card
  const renderOrderRail = (bloco) => {
    const idx = blocos.findIndex((b) => b.id === bloco.id);
    const isFirst = idx === 0;
    const isLast = idx === blocos.length - 1;

    return (
      <div className={styles["order-rail"]} onClick={(e) => e.stopPropagation()}>
        <button
          type="button"
          className={styles["order-btn"]}
          title="Mover para cima"
          onClick={() => moverBloco(bloco.id, "up")}
          disabled={isFirst}
        >
          <ChevronUp size={18} />
        </button>
        <button
          type="button"
          className={styles["order-btn"]}
          title="Mover para baixo"
          onClick={() => moverBloco(bloco.id, "down")}
          disabled={isLast}
        >
          <ChevronDown size={18} />
        </button>
      </div>
    );
  };

  // helper para reduzir repetição
  const CardWrapper = ({ bloco, children, titulo, estilo }) => (
    <div
      id={`bloco-${bloco.id}`}
      className={`${styles["bloco-pergunta"]} ${bloco.id === selectedBlockId ? styles["selecionado"] : ""}`}
      style={estilo}
      onClick={() => setSelectedBlockId(bloco.id)}
    >
      {renderHeaderPergunta(titulo, bloco)}
      {renderOrderRail(bloco)}
      {children}
      {renderPreviewAnexosPergunta(bloco)}
    </div>
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
      onRemoveImagem: removerImagemPergunta,
      onClick: () => setSelectedBlockId(bloco.id),
      style: estilo
    };

    switch (bloco.tipo) {
      case "discursiva":
        return (
          <CardWrapper key={bloco.id} bloco={bloco} titulo="Pergunta Discursiva" estilo={estilo}>
            <Discursiva
              {...propsComuns}
              onChangeRespostaExemplo={(id, v) => atualizarBloco(id, { respostaExemplo: v })}
            />
          </CardWrapper>
        );
      case "multipla":
        return (
          <CardWrapper key={bloco.id} bloco={bloco} titulo="Pergunta Múltipla Escolha" estilo={estilo}>
            <MultiplaEscolha
              {...propsComuns}
              onToggleGabarito={toggleGabarito}
              onTogglePermiteMultipla={togglePermiteMultipla}
              onToggleCorreta={toggleCorretaMultipla}
            />
          </CardWrapper>
        );
      case "objetiva":
        return (
          <CardWrapper key={bloco.id} bloco={bloco} titulo="Pergunta Objetiva (única)" estilo={estilo}>
            <Objetiva
              {...propsComuns}
              onToggleGabarito={toggleGabarito}
              onSetCorretaIndex={setCorretaIndex}
            />
          </CardWrapper>
        );
      case "anexo":
        return (
          <div
            key={bloco.id}
            id={`bloco-${bloco.id}`}
            className={`${styles["bloco-pergunta"]} ${bloco.selecionado ? styles["selecionado"] : ""}`}
            style={{ ...estilo, padding: "1rem", border: "1px solid #eee", borderRadius: "10px", backgroundColor: "#fff" }}
            onClick={() => setSelectedBlockId(bloco.id)}
          >
            <div className={styles["card-header"]}>
              <h4 className={styles["card-title"]}>Arquivo Anexo</h4>
              <div className={styles["toolbarRight"]}>
                <button
                  type="button"
                  className={`${styles["iconBtn"]} ${styles["danger"]}`}
                  onClick={() => removerBloco(bloco.id)}
                  title="Remover anexo"
                >
                  <Trash2 size={26} />
                </button>
              </div>
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

        <main ref={scrollRef} className={styles["area-construcao"]}>
          <CabecalhoPesquisa
            autor={autorNome}
            data={dadosPesquisa.dataCriacao || new Date()}
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
