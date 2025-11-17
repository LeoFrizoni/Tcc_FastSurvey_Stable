import React, { useState, useRef, useEffect, useCallback, useMemo } from "react";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import TopNavbar from "../../components/layouts/TopNavBar";
import ModalCriarPesquisa from "../../components/layouts/ModalCriarPesquisa";
import ModalQRCode from "../../components/layouts/ModalQrCode";
import ModalPreviewPesquisa from "../../components/layouts/ModalPreviewPesquisa";
import Discursiva from "../../components/perguntas/discursiva";
import MultiplaEscolha from "../../components/perguntas/multiplaEscolha";
import Objetiva from "../../components/perguntas/objetiva";
import axios from "../../config/axios";
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
  Trash2,
} from "lucide-react";
import styles from "./createPesquisa.module.css";
import CabecalhoPesquisa from "../../components/layouts/CabecalhoPesquisa";
import InformacoesPesquisa from "../../components/layouts/InformacoesPesquisa";
import { API_BASE_URL } from "../../config";

/** Util: detectar se o alvo é um elemento editável */
const isEditableTarget = (el) =>
  !!el?.closest?.('input, textarea, select, [contenteditable="true"], label');

/** Botões/ícones não devem disparar foco/seleção do card */
const preventMouseDown = (e) => e.preventDefault();

const generateBlockId = () => {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return `bloco-${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`;
};

/** Memo helpers p/ reduzir re-render dos blocos */
const MemoDiscursiva = React.memo(Discursiva);
const MemoMultipla = React.memo(MultiplaEscolha);
const MemoObjetiva = React.memo(Objetiva);

/** Card “container” de cada bloco — memo + handlers estáveis */
const CardWrapper = React.memo(function CardWrapper({
  bloco,
  titulo,
  estilo,
  isSelected,
  onSelect,
  onMoverCima,
  onMoverBaixo,
  onAbrirFilePergunta,
  onAbrirImagemPergunta,
  onRemover,
  children,
  canMoveUp,
  canMoveDown,
}) {
  return (
    <div
      id={`bloco-${bloco.id}`}
      className={`${styles["bloco-pergunta"]} ${isSelected ? styles["selecionado"] : ""}`}
      style={estilo}
      onClick={onSelect}
      onMouseDown={(e) => {
        // 1) Se clicou em algo editável, deixa focar normalmente
        if (isEditableTarget(e.target)) return;
        // 2) Se clicou em botão/ícone, deixa o botão agir
        if (
          e.target instanceof HTMLButtonElement ||
          e.target instanceof SVGElement ||
          e.target.closest("button")
        )
          return;
        // 3) Se já existe um campo editável focado dentro do card, não permita que o "fundo" roube o foco
        const active = document.activeElement;
        const activeIsEditable =
          active && e.currentTarget.contains(active) && isEditableTarget(active);
        if (activeIsEditable) e.preventDefault();
      }}
    >
      <div className={styles["card-header"]}>
        <h4 className={styles["card-title"]}>{titulo}</h4>
        <div
          className={styles["toolbarRight"]}
          onClick={(e) => e.stopPropagation()}
          onMouseDown={preventMouseDown}
        >
          <button
            type="button"
            className={styles["iconBtn"]}
            title="Adicionar anexo à pergunta"
            onClick={() => onAbrirFilePergunta(bloco.id)}
            onMouseDown={preventMouseDown}
          >
            <Paperclip size={26} />
          </button>
          <button
            type="button"
            className={styles["iconBtn"]}
            title="Adicionar imagem à pergunta"
            onClick={() => onAbrirImagemPergunta(bloco.id)}
            onMouseDown={preventMouseDown}
          >
            <ImageIcon size={26} />
          </button>
          <button
            type="button"
            className={`${styles["iconBtn"]} ${styles["danger"]}`}
            title="Remover pergunta"
            onClick={() => onRemover(bloco.id)}
            onMouseDown={preventMouseDown}
          >
            <Trash2 size={26} />
          </button>
        </div>
      </div>

      <div
        className={styles["order-rail"]}
        onClick={(e) => e.stopPropagation()}
        onMouseDown={preventMouseDown}
      >
        <button
          type="button"
          className={styles["order-btn"]}
          title="Mover para cima"
          onClick={() => onMoverCima(bloco.id)}
          onMouseDown={preventMouseDown}
          disabled={!canMoveUp}
        >
          <ChevronUp size={18} />
        </button>
        <button
          type="button"
          className={styles["order-btn"]}
          title="Mover para baixo"
          onClick={() => onMoverBaixo(bloco.id)}
          onMouseDown={preventMouseDown}
          disabled={!canMoveDown}
        >
          <ChevronDown size={18} />
        </button>
      </div>

      {children}
    </div>
  );
});

const CriarPesquisa = () => {
  const [showModal, setShowModal] = useState(true);
  const [isPerguntasAberto, setIsPerguntasAberto] = useState(false);
  const [qrUrl, setQrUrl] = useState("");
  const [mostrarModalPreview, setMostrarModalPreview] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [selectedBlockId, setSelectedBlockId] = useState(null);
  const [mostrarModalQr, setMostrarModalQr] = useState(false);

  const scrollRef = useRef(null);
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
    const loginId = localStorage.getItem("userId") || 
                   localStorage.getItem("loginId") || 
                   sessionStorage.getItem("userId") || 
                   sessionStorage.getItem("loginId");
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");
    
    console.log('🔍 CreatePesquisa - Verificando autenticação');
    console.log('🔍 CreatePesquisa - loginId:', loginId);
    console.log('🔍 CreatePesquisa - token:', token ? 'existe' : 'não existe');
    
    if (loginId) {
      console.log('✅ CreatePesquisa - LoginId encontrado, tentando carregar perfil...');
      
      // Preparar headers de autenticação (opcional)
      const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
      
      axios
        .get(`${API_BASE_URL}/api/login/Perfil`, { headers: authHeaders })
        .then((res) => {
          console.log('✅ CreatePesquisa - Usuário carregado com sucesso');
          console.log('✅ CreatePesquisa - Dados do usuário:', res.data);
          setAutorNome(res.data.usuario || res.data.nome || "Usuário");
        })
        .catch((error) => {
          console.error("❌ CreatePesquisa - Erro ao buscar usuário:", error);
          console.error("❌ CreatePesquisa - Status:", error.response?.status);
          console.error("❌ CreatePesquisa - Data:", error.response?.data);
          
          // Se o usuário não existe ou não autorizado, usar nome padrão
          if (error.response?.status === 404 || error.response?.status === 401) {
            console.log('⚠️ CreatePesquisa - Usuário não encontrado ou não autorizado, usando nome padrão');
            setAutorNome("Usuário");
          } else {
            console.log('⚠️ CreatePesquisa - Erro não crítico, usando nome padrão');
            setAutorNome("Usuário");
          }
        });
    } else {
      console.log('❌ CreatePesquisa - Sem loginId, usando nome padrão');
      setAutorNome("Usuário");
    }
  }, []);

  useEffect(() => {
    console.log('🔍 CreatePesquisa - Carregando tipos de pesquisa...');
    
    // Verificar se há token disponível (opcional)
    const token = localStorage.getItem('token') ?? sessionStorage.getItem('token');
    const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
    
    axios
      .get(`${API_BASE_URL}/api/TipoPesquisa`, { headers: authHeaders })
      .then((res) => {
        console.log('✅ CreatePesquisa - Tipos carregados:', res.data);
        console.log('✅ CreatePesquisa - Estrutura do primeiro item:', res.data[0]);
        setTiposPesquisa(res.data);
      })
      .catch((error) => {
        console.error('❌ CreatePesquisa - Erro ao carregar tipos:', error);
        setTiposPesquisa([]);
      });
  }, []);

  // --- Handlers estáveis ---
  const adicionarBloco = useCallback((tipo) => {
    console.log('🔍 adicionarBloco - Chamado com tipo:', tipo);
    
    // Evitar cliques duplos
    if (tipo === "anexo") {
      console.log('🔍 adicionarBloco - Abrindo file picker para anexo');
      // Usar setTimeout para evitar cliques duplos
      setTimeout(() => {
        inputFileRef.current?.click();
      }, 0);
      return;
    }
    
    const base = {
      id: generateBlockId(),
      tipo,
      texto: "",
      opcoes: [],
      arquivo: null,
      attachments: [],
      imagens: [],
      estilo: { corFundo: "", corTexto: "", fonte: "" },
    };

    setBlocos((prev) => {
      if (tipo === "discursiva")
        return [...prev, { ...base, opcoes: [], respostaExemplo: "" }];
      if (tipo === "multipla")
        return [
          ...prev,
          {
            ...base,
            opcoes: [""],
            TemGabarito: false,
            permitirMultiplaSelecao: true,
            corretas: [],
          },
        ];
      if (tipo === "objetiva")
        return [
          ...prev,
          { ...base, opcoes: [""], TemGabarito: false, corretaIndex: null },
        ];
      return prev;
    });
  }, []);

  const handleArquivoSelecionado = useCallback((event) => {
    console.log('🔍 handleArquivoSelecionado - Chamado');
    console.log('🔍 handleArquivoSelecionado - Files:', event.target.files);
    
    const files = Array.from(event.target.files || []);
    if (files.length === 0) {
      console.log('🔍 handleArquivoSelecionado - Nenhum arquivo selecionado');
      return;
    }

    console.log('🔍 handleArquivoSelecionado - Processando', files.length, 'arquivos');
    
    const novos = files.map((file, i) => ({
      id: Date.now() + i,
      tipo: "anexo",
      arquivo: file,
      estilo: { corFundo: "", corTexto: "", fonte: "" },
    }));
    setBlocos((prev) => [...prev, ...novos]);

    event.target.value = "";
    console.log('🔍 handleArquivoSelecionado - Concluído');
  }, []);

  const abrirFilePickerPergunta = useCallback((blockId) => {
    setAttachToBlockId(blockId);
    inputFilePerguntaRef.current?.click();
  }, []);

  const handleArquivoPerguntaSelecionado = useCallback(
    (e) => {
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
    },
    [attachToBlockId]
  );

  const abrirImagemPickerPergunta = useCallback((blockId) => {
    setImageToBlockId(blockId);
    inputImagemPerguntaRef.current?.click();
  }, []);

  const handleImagemPerguntaSelecionada = useCallback(
    (e) => {
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
    },
    [imageToBlockId]
  );

  const atualizarBloco = useCallback((id, dados) => {
    setBlocos((prev) =>
      prev.map((bloco) => (bloco.id === id ? { ...bloco, ...dados } : bloco))
    );
  }, []);

  const atualizarTexto = useCallback((id, novoTexto) => {
    setBlocos((prev) =>
      prev.map((bloco) => (bloco.id === id ? { ...bloco, texto: novoTexto } : bloco))
    );
  }, []);

  const atualizarOpcoes = useCallback((id, novasOpcoes) => {
    setBlocos((prev) =>
      prev.map((bloco) => (bloco.id === id ? { ...bloco, opcoes: novasOpcoes } : bloco))
    );
  }, []);

  const toggleGabarito = useCallback(
    (id, val) => {
      atualizarBloco(id, { TemGabarito: !!val });
    },
    [atualizarBloco]
  );

  const togglePermiteMultipla = useCallback(
    (id, val) => {
      atualizarBloco(id, {
        permitirMultiplaSelecao: !!val,
        ...(val ? {} : { corretas: [] }),
      });
    },
    [atualizarBloco]
  );

  const setCorretaIndex = useCallback(
    (id, idx) => {
      atualizarBloco(id, { corretaIndex: idx });
    },
    [atualizarBloco]
  );

  const toggleCorretaMultipla = useCallback((blocoId, index) => {
    setBlocos((prev) =>
      prev.map((b) => {
        if (b.id === blocoId) {
          const corretas = Array.isArray(b.corretas) ? [...b.corretas] : [];
          
          if (b.permitirMultiplaSelecao) {
            // Para múltipla seleção: toggle do índice
            const indexExists = corretas.includes(index);
            if (indexExists) {
              return { ...b, corretas: corretas.filter(i => i !== index) };
            } else {
              return { ...b, corretas: [...corretas, index] };
            }
          } else {
            // Para seleção única: apenas um índice
            return { ...b, corretas: [index] };
          }
        }
        return b;
      })
    );
  }, []);

  const atualizarEstilo = useCallback(
    (campo, valor) => {
      setBlocos((prev) =>
        prev.map((bloco) =>
          bloco.id === selectedBlockId
            ? { ...bloco, estilo: { ...bloco.estilo, [campo]: valor } }
            : bloco
        )
      );
    },
    [selectedBlockId]
  );

  const removerBloco = useCallback((id) => {
    setBlocos((prev) => prev.filter((bloco) => bloco.id !== id));
    setSelectedBlockId((sel) => (sel === id ? null : sel));
  }, []);



  // ---------- uploads ----------
  const getAnexoBlocos = useCallback(
    () => blocos.filter((b) => b.tipo === "anexo" && b.arquivo instanceof File),
    [blocos]
  );

  // Corrigido para bater com o backend: POST /api/anexos + PesquisaId no FormData
  const uploadAnexosPesquisa = useCallback(
    async (pesquisaId) => {
      const anexos = getAnexoBlocos();
      if (anexos.length === 0) return { ok: 0, fail: 0 };

      const uploads = anexos.map((b) => {
        const fd = new FormData();
        fd.append("anexo", b.arquivo, b.arquivo.name);
        fd.append("PesquisaId", String(pesquisaId));
        // Verificar se há token disponível (opcional)
        const token = localStorage.getItem('token') || sessionStorage.getItem('token');
        const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
        
        return axios.post(`${API_BASE_URL}/api/anexos`, fd, { headers: authHeaders });
      });

      const results = await Promise.allSettled(uploads);
      const ok = results.filter((r) => r.status === "fulfilled").length;
      const fail = results.length - ok;
      return { ok, fail };
    },
    [getAnexoBlocos]
  );

  const fetchPerguntasDaPesquisa = useCallback(async (pesquisaId) => {
    // Verificar se há token disponível (opcional) - definido fora dos try/catch
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
    
    // Aguarda mais tempo para o backend processar a pesquisa recém-criada
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    // Tentar múltiplas vezes com intervalos
    for (let tentativa = 1; tentativa <= 3; tentativa++) {
      try {
        console.log(`🔍 Tentativa ${tentativa} de buscar perguntas da pesquisa ${pesquisaId}`);
        
        const resp = await axios.get(
          `${API_BASE_URL}/api/pesquisas/BuscarPesquisaPorId/${pesquisaId}`,
          { headers: authHeaders }
        );
        
        if (resp.data?.perguntas && resp.data.perguntas.length > 0) {
          console.log(`✅ Perguntas encontradas na tentativa ${tentativa}:`, resp.data.perguntas.length);
          return resp.data.perguntas;
        }
        
        // Se não encontrou perguntas, tenta endpoint alternativo
        const respAlt = await axios.get(
          `${API_BASE_URL}/api/pesquisas/${pesquisaId}`,
          { headers: authHeaders }
        );
        
        if (respAlt.data?.perguntas && respAlt.data.perguntas.length > 0) {
          console.log(`✅ Perguntas encontradas no endpoint alternativo (tentativa ${tentativa}):`, respAlt.data.perguntas.length);
          return respAlt.data.perguntas;
        }
        
        // Se ainda não encontrou, aguarda mais um pouco antes da próxima tentativa
        if (tentativa < 3) {
          console.log(`⏳ Aguardando mais 2 segundos antes da próxima tentativa...`);
          await new Promise(resolve => setTimeout(resolve, 2000));
        }
        
      } catch (error) {
        console.error(`❌ Erro na tentativa ${tentativa} ao buscar perguntas:`, error);
        
        // Se for a última tentativa, retorna array vazio
        if (tentativa === 3) {
          console.log("⚠️ Todas as tentativas falharam, retornando array vazio");
          return [];
        }
        
        // Aguarda antes da próxima tentativa
        await new Promise(resolve => setTimeout(resolve, 2000));
      }
    }
    
    return [];
  }, []);

  const fetchOpcoesDaPergunta = useCallback(async (perguntaId) => {
    try {
              // Verificar se há token disponível (opcional)
              const token = localStorage.getItem('token') || sessionStorage.getItem('token');
              const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
              
              const resp = await axios.get(
          `${API_BASE_URL}/api/OpcoesPergunta/Pergunta/${perguntaId}`,
          { headers: authHeaders }
        );
      return Array.isArray(resp.data) ? resp.data : [];
    } catch {
      return [];
    }
  }, []);

  const mapBlocosToPerguntaIds = useCallback(
    (perguntas) => {
      const blocosPerguntas = blocos.filter((b) =>
        ["discursiva", "objetiva", "multipla"].includes(b.tipo)
      );
      const map = new Map();
      blocosPerguntas.forEach((b, i) => {
        const pid = perguntas[i]?.perguntaid;
        if (pid) map.set(b.id, pid);
      });
      return map;
    },
    [blocos]
  );

  // Corrigido para bater com o backend: POST /api/anexos + PerguntaId (e PesquisaId opcional)
  const uploadAnexosPergunta = useCallback(
    async (pesquisaId, blocoToPerguntaId) => {
      const blocosPerguntas = blocos.filter((b) =>
        ["discursiva", "objetiva", "multipla"].includes(b.tipo)
      );
      const uploads = [];

      for (const b of blocosPerguntas) {
        const files = [
          ...(Array.isArray(b.attachments) ? b.attachments : []),
          ...(Array.isArray(b.imagens) ? b.imagens.map((i) => i.file) : []),
        ];
        if (files.length === 0) continue;

        const perguntaId = blocoToPerguntaId.get(b.id);
        if (!perguntaId) continue;

        for (const file of files) {
          console.log('🔍 UploadAnexosPergunta - Enviando arquivo:', file.name, 'para pergunta:', perguntaId);
          const fd = new FormData();
          fd.append("anexo", file, file.name);
          fd.append("PerguntaId", String(""));
          fd.append("PesquisaId", String(pesquisaId)); // opcional
          // Verificar se há token disponível (opcional)
          const token = localStorage.getItem('token') || sessionStorage.getItem('token');
          const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
          
          axios.post(`${API_BASE_URL}/api/anexos`, fd, { headers: authHeaders });
        }
      }

      if (uploads.length === 0) return { ok: 0, fail: 0 };

      const results = await Promise.allSettled(uploads);
      const ok = results.filter((r) => r.status === "fulfilled").length;
      const fail = results.length - ok;
      return { ok, fail };
    },
    [blocos]
  );

  // ---------- montar VM + template ----------
  const montarPesquisaVM = useCallback(() => {
    // Verificar loginId em ambos os storages
    const loginIdStr = localStorage.getItem("userId") || 
                      localStorage.getItem("loginId") || 
                      sessionStorage.getItem("userId") || 
                      sessionStorage.getItem("loginId");
    const loginId = loginIdStr ? parseInt(loginIdStr) : null;
    const dataCriacao = new Date();

    const limparOpcoes = (ops = []) =>
      ops
        .map((o) => (typeof o === "string" ? o.trim() : o))
        .filter((o) => Boolean(o) && (typeof o === "string" ? o.trim().length > 0 : true));



    const blocosTemplate = blocos
      .filter((b) => b.tipo !== "anexo")
      .map((b, i) => {
        const opcoesLimpa = limparOpcoes(b.opcoes);
        const base = {
          perguntaid: b.id ?? i,
          texto: b.texto,
          tipo: b.tipo,
          estilo: b.estilo || { corFundo: "", corTexto: "", fonte: "" },
          imagens: (b.imagens || []).map((img, idx) => ({ idx, nome: img.file?.name })),
        };

        if (b.tipo === "objetiva") {
          return {
            ...base,
            temGabarito: !!b.TemGabarito,
            corretaIndex: b.corretaIndex ?? null,
            opcoes: opcoesLimpa.map((texto, idx) => ({
              opcaoid: idx + 1,
              texto,
              correta: !!b.TemGabarito && b.corretaIndex === idx,
            })),
          };
        }

        if (b.tipo === "multipla") {
          const corretas = Array.isArray(b.corretas) ? b.corretas : [];
          return {
            ...base,
            temGabarito: !!b.TemGabarito,
            permitirMultiplaSelecao: !!b.permitirMultiplaSelecao,
            opcoes: opcoesLimpa.map((texto, idx) => ({
              opcaoid: idx + 1,
              texto,
              correta: !!b.TemGabarito && corretas.includes(idx),
            })),
          };
        }

        if (b.tipo === "discursiva") {
          return { ...base };
        }

        return { ...base, opcoes: [] };
      });

    return {
      loginid: loginId,
      tipopesquisaid: parseInt(dadosPesquisa.tipo),
      titulo: dadosPesquisa.titulo,
      descricao: dadosPesquisa.descricao,
      templateJson: JSON.stringify(blocosTemplate),
      datacriacao: dataCriacao.toISOString(),
      dataatualizacao: null,
      anexos: [],
    };
  }, [blocos, dadosPesquisa]);

  const aplicarGabaritosNoBackend = useCallback(
    async (perguntas, blocoToPerguntaId) => {
      const blocosPerguntas = blocos.filter((b) =>
        ["discursiva", "objetiva", "multipla"].includes(b.tipo)
      );

      for (const b of blocosPerguntas) {
        const perguntaId = blocoToPerguntaId.get(b.id);
        if (!perguntaId) continue;
        if (b.tipo !== "objetiva" && b.tipo !== "multipla") continue;

        const opcoes = await fetchOpcoesDaPergunta(perguntaId);
        if (!Array.isArray(opcoes) || opcoes.length === 0) continue;

        const dto = {
          temGabarito: !!b.TemGabarito,
          permiteMultiplaSelecao: b.tipo === "multipla" ? !!b.permitirMultiplaSelecao : false,
          opcoesCorretas: [],
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
          // Verificar se há token disponível (opcional)
          const token = localStorage.getItem('token') || sessionStorage.getItem('token');
          const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
          
          await axios.put(`${API_BASE_URL}/api/Perguntas/${perguntaId}/gabarito`, dto, { headers: authHeaders });
        } catch (e) {
          console.error("Falha ao definir gabarito da pergunta", perguntaId, e);
          toast.warn(`Não foi possível definir gabarito de uma pergunta (#${perguntaId}).`, {
            position: "top-center",
            autoClose: 4000,
          });
        }
      }
    },
    [blocos, fetchOpcoesDaPergunta]
  );

  // ---------- salvar ----------
  const salvarPesquisaComPerguntas = useCallback(
    async () => {
      if (isSaving) {
        console.log("Salvamento já em andamento, ignorando clique");
        return;
      }
      
      setIsSaving(true);
      try {
        const pesquisaVM = montarPesquisaVM();
        console.log("[LOG] Estado dos blocos antes do envio:", JSON.stringify(blocos, null, 2));
        console.log("[LOG] JSON da pesquisa a ser enviado:", JSON.stringify(pesquisaVM, null, 2));
        
        // Verificar se há token disponível (opcional)
        const token = localStorage.getItem('token') || sessionStorage.getItem('token');
        const authHeaders = token ? { Authorization: `Bearer ${token}` } : {};
        
        const response = await axios.post(`${API_BASE_URL}/api/pesquisas`, pesquisaVM, { headers: authHeaders });
        const id = response.data?.pesquisaid ?? response.data?.pesquisa?.pesquisaid ?? response.data?.id;
        if (!id) {
          toast.error("Não foi possível obter o ID da pesquisa criada.", {
            position: "top-center",
            autoClose: 5000,
          });
          return;
        }

        // 1) Uploads de anexos de nível de pesquisa
        const r1 = await uploadAnexosPesquisa(id);

        // 2) Buscar perguntas geradas e mapear IDs
        let perguntas = [];
        let mapa = new Map();
        try {
          perguntas = await fetchPerguntasDaPesquisa(id);
          mapa = mapBlocosToPerguntaIds(perguntas);
        } catch (error) {
          console.warn("Não foi possível buscar perguntas, mas a pesquisa foi salva:", error);
        }

        // 3) Aplicar gabaritos (se houver)
        try {
          await aplicarGabaritosNoBackend(perguntas, mapa);
        } catch (error) {
          console.warn("Não foi possível aplicar gabaritos:", error);
        }

        // 4) Uploads por pergunta
        let r2 = { ok: 0, fail: 0 };
        try {
          r2 = await uploadAnexosPergunta(id, mapa);
        } catch (error) {
          console.warn("Não foi possível fazer upload de anexos:", error);
        }

        const urlResposta = `${window.location.origin}/responder/${id}`;
        setQrUrl(urlResposta);

        const totalOk = (r1.ok || 0) + (r2.ok || 0);
        const totalFail = (r1.fail || 0) + (r2.fail || 0);

        if (totalFail === 0 && totalOk > 0) {
          toast.success(`Pesquisa e ${totalOk} anexo(s)/imagem(ns) enviados!`, {
            position: "top-center",
            autoClose: 3000,
          });
        } else if (totalOk > 0 && totalFail > 0) {
          toast.warn(`Pesquisa salva. ${totalOk} enviados, ${totalFail} falharam.`, {
            position: "top-center",
            autoClose: 5000,
          });
        } else if (totalOk === 0 && totalFail > 0) {
          toast.error(`Pesquisa salva, mas anexos/imagens falharam.`, {
            position: "top-center",
            autoClose: 5000,
          });
        } else {
          toast.success(`Pesquisa salva com sucesso! ID: ${id}`, {
            position: "top-center",
            autoClose: 3000,
          });
        }
      } catch (erro) {
        console.error("Erro ao salvar pesquisa:", erro);
        toast.error("Ocorreu um erro ao salvar. Verifique os dados e tente novamente.", {
          position: "top-center",
          autoClose: 4000,
        });
      } finally {
        setIsSaving(false);
      }
    },
    [
      aplicarGabaritosNoBackend,
      fetchPerguntasDaPesquisa,
      mapBlocosToPerguntaIds,
      montarPesquisaVM,
      uploadAnexosPergunta,
      uploadAnexosPesquisa,
      blocos,
      isSaving,
    ]
  );

  // ---------- mover com scroll estável ----------
  const moverBlocoBase = useCallback((id, direcao) => {
    const cardEl = document.getElementById(`bloco-${id}`);
    const container = scrollRef.current;
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

      if (beforeTop !== null && container) {
        requestAnimationFrame(() => {
          const afterEl = document.getElementById(`bloco-${id}`);
          if (!afterEl) return;
          const afterTop = afterEl.getBoundingClientRect().top;
          const delta = afterTop - beforeTop;

          if (document.activeElement && container.contains(document.activeElement)) return;
          container.scrollTop += delta;
        });
      }
      return novo;
    });
  }, []);

  const moverCima = useCallback((id) => moverBlocoBase(id, "up"), [moverBlocoBase]);
  const moverBaixo = useCallback((id) => moverBlocoBase(id, "down"), [moverBlocoBase]);

  // --- render helpers memoizados ---
  const nomeTipoPesquisa = useMemo(
    () => {
      console.log('🔍 nomeTipoPesquisa - Calculando...');
      console.log('🔍 nomeTipoPesquisa - tiposPesquisa:', tiposPesquisa);
      console.log('🔍 nomeTipoPesquisa - dadosPesquisa.tipo:', dadosPesquisa.tipo);
      
      if (!dadosPesquisa.tipo || !tiposPesquisa.length) {
        console.log('🔍 nomeTipoPesquisa - Sem tipo selecionado ou tipos não carregados');
        return "—";
      }
      
      const tipoId = parseInt(dadosPesquisa.tipo);
      console.log('🔍 nomeTipoPesquisa - tipoId:', tipoId);
      
      // Tentar diferentes propriedades possíveis
      const tipoEncontrado = tiposPesquisa.find((tp) => 
        tp.tipopesquisaid === tipoId || 
        tp.TipoPesquisaId === tipoId || 
        tp.tipoPesquisaId === tipoId ||
        tp.id === tipoId ||
        tp.Id === tipoId
      );
      
      console.log('🔍 nomeTipoPesquisa - tipoEncontrado:', tipoEncontrado);
      
      if (tipoEncontrado) {
        const nome = tipoEncontrado.tipopesquisa1 || 
                    tipoEncontrado.Tipopesquisa1 || 
                    tipoEncontrado.tipopesquisa || 
                    tipoEncontrado.tipoPesquisa ||
                    tipoEncontrado.nome ||
                    tipoEncontrado.Nome ||
                    "Nome não encontrado";
        console.log('🔍 nomeTipoPesquisa - nome encontrado:', nome);
        return nome;
      }
      
      console.log('🔍 nomeTipoPesquisa - Tipo não encontrado');
      return "—";
    },
    [tiposPesquisa, dadosPesquisa.tipo]
  );

  const renderizarBloco = useCallback(
    (bloco, index, arr) => {
      const estilo = {
        backgroundColor: bloco.estilo?.corFundo || "white",
        color: bloco.estilo?.corTexto || "inherit",
        fontFamily: bloco.estilo?.fonte || "inherit",
      };

      const isSelected = bloco.id === selectedBlockId;
      const canMoveUp = index > 0;
      const canMoveDown = index < arr.length - 1;

      const onSelect = () => setSelectedBlockId(bloco.id);

      const commonProps = {
        bloco: { ...bloco, selecionado: isSelected },
        onChangeTexto: atualizarTexto,
        onChangeOpcoes: atualizarOpcoes,
        onChangeBloco: atualizarBloco,
        onRemove: removerBloco,
        onRemoveImagem: (blockId, imgIndex) => {
          setBlocos((prev) =>
            prev.map((b) =>
              b.id === blockId
                ? { ...b, imagens: (b.imagens || []).filter((_, i) => i !== imgIndex) }
                : b
            )
          );
        },
        onClick: onSelect,
        style: estilo,
      };

      switch (bloco.tipo) {
        case "discursiva":
          return (
            <CardWrapper
              key={bloco.id}
              bloco={bloco}
              titulo="Pergunta Discursiva"
              estilo={estilo}
              isSelected={isSelected}
              onSelect={onSelect}
              onMoverCima={moverCima}
              onMoverBaixo={moverBaixo}
              onAbrirFilePergunta={abrirFilePickerPergunta}
              onAbrirImagemPergunta={abrirImagemPickerPergunta}
              onRemover={removerBloco}
              canMoveUp={canMoveUp}
              canMoveDown={canMoveDown}
            >
              <MemoDiscursiva
                {...commonProps}
                onChangeRespostaExemplo={(id, v) => atualizarBloco(id, { respostaExemplo: v })}
              />
            </CardWrapper>
          );
        case "multipla":
          return (
                         <CardWrapper
               key={bloco.id}
               bloco={bloco}
               titulo="Pergunta Múltipla Escolha"
               estilo={estilo}
               isSelected={isSelected}
               onSelect={onSelect}
               onMoverCima={moverCima}
               onMoverBaixo={moverBaixo}
               onAbrirFilePergunta={abrirFilePickerPergunta}
               onAbrirImagemPergunta={abrirImagemPickerPergunta}
               onRemover={removerBloco}
               canMoveUp={canMoveUp}
               canMoveDown={canMoveDown}
             >
              <MemoMultipla
                {...commonProps}
                onToggleGabarito={toggleGabarito}
                onTogglePermiteMultipla={togglePermiteMultipla}
                onToggleCorreta={toggleCorretaMultipla}
              />
            </CardWrapper>
          );
        case "objetiva":
          return (
            <CardWrapper
              key={bloco.id}
              bloco={bloco}
              titulo="Pergunta Objetiva (única)"
              estilo={estilo}
              isSelected={isSelected}
              onSelect={onSelect}
              onMoverCima={moverCima}
              onMoverBaixo={moverBaixo}
              onAbrirFilePergunta={abrirFilePickerPergunta}
              onAbrirImagemPergunta={abrirImagemPickerPergunta}
              onRemover={removerBloco}
              canMoveUp={canMoveUp}
              canMoveDown={canMoveDown}
            >
              <MemoObjetiva
                {...commonProps}
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
              className={`${styles["bloco-pergunta"]} ${isSelected ? styles["selecionado"] : ""}`}
              style={{
                ...estilo,
                padding: "1rem",
                border: "1px solid #eee",
                borderRadius: "10px",
                backgroundColor: "#fff",
              }}
              onClick={onSelect}
              onMouseDown={(e) => {
                if (isEditableTarget(e.target)) return;
                if (
                  e.target instanceof HTMLButtonElement ||
                  e.target instanceof SVGElement ||
                  e.target.closest("button")
                )
                  return;
                const active = document.activeElement;
                const activeIsEditable =
                  active && e.currentTarget.contains(active) && isEditableTarget(active);
                if (activeIsEditable) e.preventDefault();
              }}
            >
              <div className={styles["card-header"]}>
                <h4 className={styles["card-title"]}>Arquivo Anexo</h4>
                <div
                  className={styles["toolbarRight"]}
                  onClick={(e) => e.stopPropagation()}
                  onMouseDown={preventMouseDown}
                >
                  <button
                    type="button"
                    className={`${styles["iconBtn"]} ${styles["danger"]}`}
                    onClick={() => removerBloco(bloco.id)}
                    onMouseDown={preventMouseDown}
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
    },
    [
      selectedBlockId,
      atualizarTexto,
      atualizarOpcoes,
      atualizarBloco,
      removerBloco,
      moverCima,
      moverBaixo,
      abrirFilePickerPergunta,
      abrirImagemPickerPergunta,
      toggleGabarito,
      togglePermiteMultipla,
      toggleCorretaMultipla,
      setCorretaIndex,
    ]
  );

  return (
    <>
      <TopNavbar />
      <ToastContainer />
      <div className={styles["editor-container"]}>
        {/* inputs ocultos */}
        <input
          type="file"
          ref={inputFileRef}
          style={{ display: "none" }}
          multiple
          onChange={handleArquivoSelecionado}
        />
        <input
          type="file"
          ref={inputFilePerguntaRef}
          style={{ display: "none" }}
          multiple
          onChange={handleArquivoPerguntaSelecionado}
        />
        <input
          type="file"
          ref={inputImagemPerguntaRef}
          style={{ display: "none" }}
          multiple
          accept="image/*"
          onChange={handleImagemPerguntaSelecionada}
        />

        {showModal && (
          <ModalCriarPesquisa
            onConfirm={(dados) => {
              setDadosPesquisa(dados);
              setShowModal(false);
            }}
          />
        )}

        <aside className={styles["sidebar-esquerda"]}>
          <div className={styles["sidebar-scroll"]}>
            <h3>Componentes</h3>
            <ul>
              <li
                className={styles["item-clique"]}
                onClick={() => setIsPerguntasAberto((s) => !s)}
                onMouseDown={preventMouseDown}
              >
                {isPerguntasAberto ? <ChevronUp size={18} /> : <ChevronDown size={18} />} Perguntas
              </li>
              {isPerguntasAberto && (
                <ul className={styles["submenu"]}>
                  <li
                    className={styles["item-clique"]}
                    onClick={() => adicionarBloco("discursiva")}
                    onMouseDown={preventMouseDown}
                  >
                    <FileText size={18} /> Discursiva
                  </li>
                  <li
                    className={styles["item-clique"]}
                    onClick={() => adicionarBloco("multipla")}
                    onMouseDown={preventMouseDown}
                  >
                    <Circle size={18} /> Múltipla Escolha
                  </li>
                  <li
                    className={styles["item-clique"]}
                    onClick={() => adicionarBloco("objetiva")}
                    onMouseDown={preventMouseDown}
                  >
                    <Circle size={18} /> Objetiva (única)
                  </li>
                </ul>
              )}
              <li
                className={styles["item-clique"]}
                onClick={() => adicionarBloco("anexo")}
              >
                <Paperclip size={18} /> Anexo (da pesquisa)
              </li>
            </ul>
          </div>
          <div className={styles["salvar-wrapper"]}>
            <button
              className={styles["salvar-btn"]}
              style={{ marginBottom: "8px" }}
              onClick={() => setMostrarModalPreview(true)}
              onMouseDown={preventMouseDown}
              type="button"
            >
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
            <button
              className={styles["salvar-btn"]}
              onClick={salvarPesquisaComPerguntas}
              onMouseDown={preventMouseDown}
              type="button"
              disabled={isSaving}
            >
              <Save size={18} style={{ marginRight: "6px" }} /> 
              {isSaving ? "Salvando..." : "Salvar Pesquisa"}
            </button>
            {qrUrl && (
              <>
                <button
                  className={styles["salvar-btn"]}
                  onClick={() => setMostrarModalQr(true)}
                  onMouseDown={preventMouseDown}
                  type="button"
                >
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

        <main ref={scrollRef} className={styles["area-construcao"]} style={{ overflowAnchor: "none" }}>
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
              fontFamily: "'Segoe UI', 'Inter', Arial, sans-serif",
            }}
          />
          <InformacoesPesquisa
            titulo={dadosPesquisa.titulo}
            descricao={dadosPesquisa.descricao}
            tipoPesquisa={nomeTipoPesquisa}
          />

          <div
            className={`${styles["area-preview"]} ${
              blocos.length > 0 ? styles["invisivel"] : ""
            }`}
          >
            <p>Adicione blocos para montar sua pesquisa</p>
          </div>

          {blocos.length > 0 && blocos.map((b, i, arr) => renderizarBloco(b, i, arr))}
        </main>

        <aside className={styles["sidebar-direita"]}>
          <h3>Estilo</h3>
          <label>
            {" "}
            <Paintbrush2 size={18} /> Cor de Fundo
            <input
              type="color"
              onChange={(e) => atualizarEstilo("corFundo", e.target.value)}
              disabled={!selectedBlockId}
            />
          </label>
          <label>
            {" "}
            <Droplet size={18} /> Cor do Texto
            <input
              type="color"
              onChange={(e) => atualizarEstilo("corTexto", e.target.value)}
              disabled={!selectedBlockId}
            />
          </label>
          <label>
            {" "}
            <Type size={18} /> Fonte
            <select
              onChange={(e) => atualizarEstilo("fonte", e.target.value)}
              disabled={!selectedBlockId}
            >
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
              <option
                style={{ fontFamily: "Franklin Gothic Medium" }}
                value="Franklin Gothic Medium"
              >
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
