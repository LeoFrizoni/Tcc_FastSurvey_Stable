// src/pages/home/Home.jsx
import React, { useEffect, useMemo, useState, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus, User, LogOut, Info, Search, Filter, ChevronRight, Tag,
  FolderPlus, Folder, FolderOpen, X, Pencil, Trash2
} from 'lucide-react';
import axios from 'axios';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import styles from './home.module.css';
import { API_BASE_URL } from '../../config';
import { logout } from '../../utils/auth';

/* ===================== Helpers ===================== */
const readLocalMap = (key) => {
  try {
    const raw = localStorage.getItem(key);
    return raw ? JSON.parse(raw) : { pastas: [], vinculos: {} };
  } catch {
    return { pastas: [], vinculos: {} };
  }
};
const writeLocalMap = (key, data) => {
  localStorage.setItem(key, JSON.stringify(data));
};

// id seguro independente do casing que veio do back
const getPid = (p) => p?.pesquisaid ?? p?.pesquisaId ?? p?.PesquisaId ?? null;

// tenta ler o TipoPesquisaId em diferentes formatos
const getTipoId = (p) =>
  p?.tipoPesquisaId ??
  p?.TipoPesquisaId ??
  p?.tipopesquisaid ??
  p?.TipopesquisaId ??
  p?.TipoPesquisaID ??
  p?.TipoPesquisaid ??
  p?.TipoPesquisa ??
  p?.tipopesquisa ??
  null;

// normaliza PASTA vinda do back/cache para { pastaId, nome }
const normalizePasta = (it) => ({
  pastaId: it?.pastaId ?? it?.PastaId ?? it?.pastaid ?? it?.Pastaid,
  nome: it?.nome ?? it?.Nome ?? it?.nomePasta ?? ''
});

// normaliza PESQUISA para o shape que o UI espera
const normalizePesquisa = (it) => {
  const tipoDesc =
    it?.tipoPesquisa?.descricao ??
    it?.TipoPesquisaDescricao ??
    it?.tipopesquisa?.descricao ??
    it?.Tipopesquisa?.Descricao ??
    null;

  return {
    pesquisaid: it.pesquisaid ?? it.pesquisaId ?? it.PesquisaId,
    titulo: it.titulo ?? it.Titulo ?? '',
    descricao: it.descricao ?? it.Descricao ?? '',
    pastaId: it.pastaId ?? it.PastaId ?? null,
    dataCriacao: it.dataCriacao ?? it.criadoEm ?? it.Datacriacao ?? it.data_criacao,
    tipoPesquisa: tipoDesc ? { descricao: tipoDesc } : it.tipoPesquisa,
    ...it,
  };
};

// normaliza string para comparação case/acentos-insensitive
const keyOf = (s) =>
  (s ?? '')
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .toLowerCase()
    .trim();

/* =================================================== */

const HomePage = () => {
  const navigate = useNavigate();

  // Dados
  const [pesquisas, setPesquisas] = useState([]);
  const [pastas, setPastas] = useState([]); // [{ pastaId, nome }]
  const [tipoLookup, setTipoLookup] = useState(new Map()); // id -> descricao
  const [loading, setLoading] = useState(true);

  // UI
  const [query, setQuery] = useState('');
  const [filtroTipo, setFiltroTipo] = useState('todos');
  const [ordenarPor, setOrdenarPor] = useState('recente');

  // Filtro por pasta
  const [pastaSelecionada, setPastaSelecionada] = useState('todas'); // 'todas' | 'sem' | pastaId

  // Drag state
  const [draggingId, setDraggingId] = useState(null);
  const [pastaHover, setPastaHover] = useState(null);
  const [pastaPulse, setPastaPulse] = useState(null);

  const dragOverPastaRef = useRef(null);

  // Modal: Nova pasta
  const [isNovaPastaOpen, setIsNovaPastaOpen] = useState(false);
  const [novoNomePasta, setNovoNomePasta] = useState('');
  const inputRef = useRef(null);

  // Modal: Renomear pasta
  const [isRenomearOpen, setIsRenomearOpen] = useState(false);
  const [pastaAlvo, setPastaAlvo] = useState(null); // { pastaId, nome }
  const [novoNomeEdicao, setNovoNomeEdicao] = useState('');

  // Modal: Excluir pasta
  const [isExcluirOpen, setIsExcluirOpen] = useState(false);

  // Modal: Excluir pesquisa
  const [isExcluirPesquisaOpen, setIsExcluirPesquisaOpen] = useState(false);
  const [pesquisaAlvo, setPesquisaAlvo] = useState(null);

  // loginId seguro (aceita 'loginId' ou 'userId')
  const loginId = useMemo(() => {
    const v = localStorage.getItem('loginId') ?? localStorage.getItem('userId');
    const n = v ? parseInt(v, 10) : NaN;
    return Number.isFinite(n) ? n : null;
  }, []);
  const token = useMemo(() => localStorage.getItem('token'), []);

  const MAP_STORAGE_KEY = useMemo(
    () => `fs_pastas_map_user_${loginId || 'anon'}`,
    [loginId]
  );

  const ensureLocalMapSync = useCallback((remotePastas) => {
    const map = readLocalMap(MAP_STORAGE_KEY);
    if (Array.isArray(remotePastas)) {
      const norm = remotePastas.map(normalizePasta);
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: norm,
        vinculos: map.vinculos || {},
      });
    }
  }, [MAP_STORAGE_KEY]);

  // Aplica descricao do tipo a cada pesquisa usando lookup
  const attachTipoDescricao = useCallback(
    (arr) => {
      if (!arr || !arr.length) return arr || [];
      if (!tipoLookup || tipoLookup.size === 0) return arr;

      return arr.map((p) => {
        const hasDesc = p?.tipoPesquisa?.descricao;
        if (hasDesc) return p;

        const tid = getTipoId(p);
        if (!tid) return p;

        const desc = tipoLookup.get(Number(tid));
        if (!desc) return p;

        return { ...p, tipoPesquisa: { descricao: desc } };
      });
    },
    [tipoLookup]
  );

  /* === Carregamento inicial === */
  useEffect(() => {
    const bootstrap = async () => {
      if (!loginId || !token) {
        toast.error('Você precisa estar logado para acessar esta página.');
        navigate('/login');
        return;
      }

      try {
        // Buscar tipos + pesquisas + pastas em paralelo
        const [tiposRes, pesqRes, pastasRes] = await Promise.allSettled([
          axios.get(`${API_BASE_URL}/api/TipoPesquisa`, { headers: { Authorization: `Bearer ${token}` } }),
          axios.get(`${API_BASE_URL}/api/pesquisas/usuario/${loginId}`, { headers: { Authorization: `Bearer ${token}` } }),
          axios.get(`${API_BASE_URL}/api/pastas`, { params: { loginid: loginId }, headers: { Authorization: `Bearer ${token}` } })
        ]);

        // Tipos
        if (tiposRes.status === 'fulfilled' && Array.isArray(tiposRes.value.data)) {
          const map = new Map();
          for (const t of tiposRes.value.data) {
            const id = t?.tipoPesquisaId ?? t?.TipoPesquisaId ?? t?.tipopesquisaid ?? t?.id;
            const desc = t?.descricao ?? t?.Descricao ?? t?.nome ?? '';
            if (id) map.set(Number(id), String(desc || '').trim());
          }
          setTipoLookup(map);
        } else {
          setTipoLookup(new Map());
        }

        // Pesquisas
        if (pesqRes.status === 'fulfilled') {
          const arr = Array.isArray(pesqRes.value.data) ? pesqRes.value.data.map(normalizePesquisa) : [];
          setPesquisas(arr);
        } else {
          setPesquisas([]);
          toast.error('Falha ao carregar pesquisas.');
        }

        // Pastas
        if (pastasRes.status === 'fulfilled') {
          const norm = Array.isArray(pastasRes.value.data) ? pastasRes.value.data.map(normalizePasta) : [];
          setPastas(norm);
          ensureLocalMapSync(norm);
        } else {
          const map = readLocalMap(MAP_STORAGE_KEY);
          const norm = Array.isArray(map.pastas) ? map.pastas.map(normalizePasta) : [];
          setPastas(norm);
          toast.warn('Não consegui carregar as pastas do servidor. Usei o cache local.');
        }
      } catch (err) {
        console.error(err);
        toast.error('Falha ao carregar dados.');
      } finally {
        setLoading(false);
      }
    };
    bootstrap();
  }, [loginId, token, navigate, ensureLocalMapSync, MAP_STORAGE_KEY]);

  // Reaplica descricao quando o lookup chegar depois das pesquisas
  const pesquisasEnriquecidas = useMemo(
    () => attachTipoDescricao(pesquisas),
    [pesquisas, attachTipoDescricao]
  );

  /* === Modal Nova Pasta === */
  const abrirModalPasta = () => {
    setNovoNomePasta('');
    setIsNovaPastaOpen(true);
  };
  useEffect(() => {
    if (isNovaPastaOpen && inputRef.current) inputRef.current.focus();
  }, [isNovaPastaOpen]);

  const criarPastaConfirm = useCallback(async () => {
    const nome = (novoNomePasta || '').trim();
    if (!nome) return;

    try {
      const { data } = await axios.post(
        `${API_BASE_URL}/api/pastas`,
        { nome, loginid: loginId },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      const nova = normalizePasta(data);
      setPastas((prev) => [...prev, nova]);

      const map = readLocalMap(MAP_STORAGE_KEY);
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: [...(map.pastas || []), nova],
        vinculos: map.vinculos || {},
      });

      toast.success(`Pasta "${nome}" criada!`);
    } catch (err) {
      console.error('Falha ao criar pasta no backend. Caindo para cache local.', err);
      const nova = { pastaId: (crypto?.randomUUID?.() ?? `local-${Date.now()}`), nome }; // fallback local
      setPastas((prev) => [...prev, nova]);
      const map = readLocalMap(MAP_STORAGE_KEY);
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: [...(map.pastas || []), nova],
        vinculos: map.vinculos || {},
      });
      toast.info(`Pasta "${nome}" criada localmente (offline).`);
    } finally {
      setIsNovaPastaOpen(false);
    }
  }, [novoNomePasta, loginId, token, MAP_STORAGE_KEY]);

  // Atalhos teclado no modal
  useEffect(() => {
    const onKey = (e) => {
      if (!isNovaPastaOpen) return;
      if (e.key === 'Escape') setIsNovaPastaOpen(false);
      if (e.key === 'Enter') criarPastaConfirm();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [isNovaPastaOpen, criarPastaConfirm]);

  /* === Mover pesquisa para pasta === */
  const movePesquisaToPasta = async (pesquisaId, pastaIdOrSem) => {
    const destinoId = pastaIdOrSem === 'sem' ? null : pastaIdOrSem;

    try {
      await axios.patch(
        `${API_BASE_URL}/api/pesquisas/${pesquisaId}/mover-pasta`,
        { pastaId: destinoId },
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setPesquisas((prev) =>
        prev.map((p) => (getPid(p) === pesquisaId ? { ...p, pastaId: destinoId } : p))
      );

      const map = readLocalMap(MAP_STORAGE_KEY);
      const vinc = { ...(map.vinculos || {}) };
      if (destinoId) vinc[pesquisaId] = destinoId; else delete vinc[pesquisaId];
      writeLocalMap(MAP_STORAGE_KEY, { pastas: map.pastas || [], vinculos: vinc });
    } catch (err) {
      const map = readLocalMap(MAP_STORAGE_KEY);
      const vinc = { ...(map.vinculos || {}) };
      if (destinoId) vinc[pesquisaId] = destinoId; else delete vinc[pesquisaId];
      writeLocalMap(MAP_STORAGE_KEY, { pastas: map.pastas || [], vinculos: vinc });

      setPesquisas((prev) =>
        prev.map((p) => (getPid(p) === pesquisaId ? { ...p, pastaId: destinoId } : p))
      );
      toast.warn('Não consegui atualizar no servidor. Alteração aplicada localmente.');
    } finally {
      setPastaPulse(pastaIdOrSem);
      setTimeout(() => setPastaPulse(null), 900);
    }
  };

  /* === Aplica vínculo local quando API não traz pastaId === */
  const pesquisasComPastaVinculada = useMemo(() => {
    const map = readLocalMap(MAP_STORAGE_KEY);
    const vinc = map.vinculos || {};
    return (pesquisasEnriquecidas || []).map((p) => {
      const pid = getPid(p);
      const pastaId =
        typeof p.pastaId !== 'undefined'
          ? p.pastaId
          : (pid ? vinc[pid] : null);
      return { ...p, pastaId };
    });
  }, [pesquisasEnriquecidas, MAP_STORAGE_KEY]);

  /* === Filtros === */
  const tiposDisponiveis = useMemo(() => {
    // nomes vindos do p.tipoPesquisa.descricao (já enriquecido) ou do lookup usando TipoPesquisaId
    const set = new Set(
      pesquisasComPastaVinculada
        .map((p) => {
          const desc = p?.tipoPesquisa?.descricao;
          if (desc) return desc;
          const tid = getTipoId(p);
          return tid && tipoLookup.get(Number(tid));
        })
        .filter((t) => t && typeof t === 'string')
    );
    return ['todos', ...Array.from(set)];
  }, [pesquisasComPastaVinculada, tipoLookup]);

  const listaFiltrada = useMemo(() => {
    let base = [...pesquisasComPastaVinculada];

    if (query.trim()) {
      const q = keyOf(query);
      base = base.filter(
        (p) =>
          keyOf(p?.titulo).includes(q) ||
          keyOf(p?.descricao).includes(q) ||
          keyOf(p?.tipoPesquisa?.descricao ?? (tipoLookup.get(Number(getTipoId(p))) || '')).includes(q)
      );
    }

    if (filtroTipo !== 'todos') {
      const k = keyOf(filtroTipo);
      base = base.filter((p) => {
        const desc = p?.tipoPesquisa?.descricao ?? (tipoLookup.get(Number(getTipoId(p))) || '');
        return keyOf(desc) === k;
      });
    }

    if (pastaSelecionada !== 'todas') {
      base = pastaSelecionada === 'sem'
        ? base.filter((p) => !p.pastaId)
        : base.filter((p) => p.pastaId === pastaSelecionada);
    }

    if (ordenarPor === 'a-z') {
      base.sort((a, b) => (a.titulo || '').localeCompare(b.titulo || ''));
    } else if (ordenarPor === 'z-a') {
      base.sort((a, b) => (b.titulo || '').localeCompare(a.titulo || ''));
    } else {
      const getDate = (x) => x?.dataCriacao || x?.criadoEm || x?.datacriacao || 0;
      base.sort((a, b) => new Date(getDate(b)) - new Date(getDate(a)));
    }

    return base;
  }, [pesquisasComPastaVinculada, query, filtroTipo, ordenarPor, pastaSelecionada, tipoLookup]);

  /* === Drag & Drop === */
  const onDragStart = (e, pesquisaId) => {
    e.dataTransfer.setData('text/plain', String(pesquisaId));
    setDraggingId(pesquisaId);
    e.currentTarget.classList.add(styles.cardDragging);
    document.body.classList.add(styles.draggingBody);
  };
  const onDragEnd = (e) => {
    setDraggingId(null);
    e.currentTarget.classList.remove(styles.cardDragging);
    dragOverPastaRef.current = null;
    setPastaHover(null);
    document.body.classList.remove(styles.draggingBody);
  };
  const allowDrop = (e) => e.preventDefault();
  const onPastaDragEnter = (key) => { dragOverPastaRef.current = key; setPastaHover(key); };
  const onPastaDragLeave = (key) => {
    if (dragOverPastaRef.current === key) dragOverPastaRef.current = null;
    setPastaHover((prev) => (prev === key ? null : prev));
  };
  const onPastaDrop = (e, key) => {
    e.preventDefault();
    const id = parseInt(e.dataTransfer.getData('text/plain'));
    if (!id) return;
    movePesquisaToPasta(id, key);
    dragOverPastaRef.current = null;
    setPastaHover(null);
  };

  const labelPasta = useCallback((pastaId) => {
    if (!pastaId) return 'Sem pasta';
    const p = pastas.find((x) => x.pastaId === pastaId);
    return p ? (p.nome || 'Pasta') : 'Pasta';
  }, [pastas]);

  // Usar função centralizada de logout
  const handleLogout = () => {
    logout();
  };

  /* ===================== AÇÕES: Renomear / Excluir ===================== */
  const abrirRenomear = (p) => {
    setPastaAlvo(p);
    setNovoNomeEdicao(p?.nome || '');
    setIsRenomearOpen(true);
  };

  const confirmarRenomear = async () => {
    const novoNome = (novoNomeEdicao || '').trim();
    if (!pastaAlvo || !novoNome) return;

    try {
      await axios.put(
        `${API_BASE_URL}/api/pastas/${pastaAlvo.pastaId}/nome`,
        { novoNome },
        { params: { loginid: loginId }, headers: { Authorization: `Bearer ${token}` } }
      );

      setPastas((prev) =>
        prev.map((x) => (x.pastaId === pastaAlvo.pastaId ? { ...x, nome: novoNome } : x))
      );
      const map = readLocalMap(MAP_STORAGE_KEY);
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: (map.pastas || []).map((x) => (x.pastaId === pastaAlvo.pastaId ? { ...x, nome: novoNome } : x)),
        vinculos: map.vinculos || {},
      });

      toast.success('Pasta renomeada!');
    } catch (err) {
      const status = err?.response?.status;
      if (status === 409) toast.warn('Já existe uma pasta com esse nome.');
      else toast.error('Falha ao renomear a pasta.');
      console.error('Falha ao renomear pasta:', err);
    } finally {
      setIsRenomearOpen(false);
      setPastaAlvo(null);
      setNovoNomeEdicao('');
    }
  };

  const abrirExcluir = (p) => {
    setPastaAlvo(p);
    setIsExcluirOpen(true);
  };

  const confirmarExcluir = async () => {
    if (!pastaAlvo) return;
    try {
      await axios.delete(
        `${API_BASE_URL}/api/pastas/${pastaAlvo.pastaId}`,
        { params: { loginid: loginId }, headers: { Authorization: `Bearer ${token}` } }
      );
      setPastas((prev) => prev.filter((x) => x.pastaId !== pastaAlvo.pastaId));
      const map = readLocalMap(MAP_STORAGE_KEY);
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: (map.pastas || []).filter((x) => x.pastaId !== pastaAlvo.pastaId),
        vinculos: map.vinculos || {},
      });

      setPastaSelecionada((cur) => (cur === pastaAlvo.pastaId ? 'todas' : cur));
      setPesquisas((prev) => prev.map(p => (p.pastaId === pastaAlvo.pastaId ? { ...p, pastaId: null } : p)));

      toast.success('Pasta excluída!');
    } catch (err) {
      console.error('Falha ao excluir pasta:', err);
      toast.error('Falha ao excluir a pasta.');
    } finally {
      setIsExcluirOpen(false);
      setPastaAlvo(null);
    }
  };

  // Funções para exclusão de pesquisa
  const abrirExcluirPesquisa = (pesquisa) => {
    setPesquisaAlvo(pesquisa);
    setIsExcluirPesquisaOpen(true);
  };

  const confirmarExcluirPesquisa = async () => {
    if (!pesquisaAlvo) return;
    try {
      const pesquisaId = getPid(pesquisaAlvo);
      await axios.delete(
        `${API_BASE_URL}/api/pesquisas/${pesquisaId}`,
        { headers: { Authorization: `Bearer ${token}` } }
      );
      
      setPesquisas((prev) => prev.filter((p) => getPid(p) !== pesquisaId));
      toast.success('Pesquisa excluída com sucesso!');
    } catch (err) {
      console.error('Falha ao excluir pesquisa:', err);
      toast.error('Falha ao excluir a pesquisa.');
    } finally {
      setIsExcluirPesquisaOpen(false);
      setPesquisaAlvo(null);
    }
  };

  console.log('DEBUG - Todas as variáveis REACT_APP:', process.env);

  /* ============================== RENDER ============================== */
  return (
    <div className={styles.wrapper}>
      {/* Sidebar */}
      <aside className={styles.sidebar}>
        <div className={styles.brand}>FastSurvey</div>
        <nav className={styles.nav}>
          <button type="button" className={`${styles.navItem} ${styles.navWhite}`} onClick={() => navigate('/perfil')}>
            <User size={18} /> <span>Perfil</span>
          </button>
          <button type="button" className={`${styles.navItem} ${styles.navWhite}`} onClick={() => navigate('/sobre-nos')}>
            <Info size={18} /> <span>Sobre Nós</span>
          </button>
          <div className={styles.navSpacer} />
          <button type="button" className={`${styles.navItem} ${styles.navWhite} ${styles.navDanger}`} onClick={handleLogout} aria-label="Sair">
            <LogOut size={18} /> <span>Sair</span>
          </button>
        </nav>
        <footer className={styles.copy}>© {new Date().getFullYear()} FastSurvey</footer>
      </aside>

      {/* Main */}
      <main className={styles.main}>
        <header className={styles.header}>
          <div className={styles.headerLeft}>
            <h1 className={styles.title}>Minhas Pesquisas</h1>
            <p className={styles.subtitle}>Crie, gerencie e acompanhe seus formulários.</p>
          </div>
          <div className={styles.headerRight}>
            <button className={styles.cta} onClick={() => navigate('/criar')}>
              <Plus size={18} /> Criar Pesquisa
            </button>
            <button className={styles.ctaSecondary} onClick={abrirModalPasta} title="Nova pasta">
              <FolderPlus size={18} /> Nova Pasta
            </button>
          </div>
        </header>

        {/* Toolbar */}
        <section className={styles.toolbar}>
          <div className={styles.searchBox}>
            <Search size={16} />
            <input
              type="text"
              placeholder="Pesquisar por título, descrição ou tipo…"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
          </div>

          <div className={styles.filters}>
            <div className={styles.selectWrap}>
              <Filter size={16} />
              <select value={filtroTipo} onChange={(e) => setFiltroTipo(e.target.value)}>
                {tiposDisponiveis.map((t) => (
                  <option key={t} value={t}>
                    {t === 'todos' ? 'Todos os tipos' : t}
                  </option>
                ))}
              </select>
            </div>

            <div className={styles.selectWrap}>
              <Tag size={16} />
              <select value={ordenarPor} onChange={(e) => setOrdenarPor(e.target.value)}>
                <option value="recente">Mais recentes</option>
                <option value="a-z">Título A → Z</option>
                <option value="z-a">Título Z → A</option>
              </select>
            </div>
          </div>
        </section>

        {/* Pastas */}
        <section className={styles.pastasBar}>
          <div
            className={[
              styles.pastaChip,
              pastaSelecionada === 'todas' ? styles.pastaChipActive : '',
              pastaHover === 'todas' ? styles.pastaChipHover : '',
              pastaPulse === 'todas' ? styles.pastaChipPulse : ''
            ].join(' ')}
            onClick={() => setPastaSelecionada('todas')}
            onDragOver={allowDrop}
            onDragEnter={() => onPastaDragEnter('todas')}
            onDragLeave={() => onPastaDragLeave('todas')}
            onDrop={(e) => onPastaDrop(e, 'todas')}
            title="Ver todas as pesquisas"
          >
            <FolderOpen size={16} />
            <span>Todas</span>
          </div>

          <div
            className={[
              styles.pastaChip,
              pastaSelecionada === 'sem' ? styles.pastaChipActive : '',
              pastaHover === 'sem' ? styles.pastaChipHover : '',
              pastaPulse === 'sem' ? styles.pastaChipPulse : ''
            ].join(' ')}
            onClick={() => setPastaSelecionada('sem')}
            onDragOver={allowDrop}
            onDragEnter={() => onPastaDragEnter('sem')}
            onDragLeave={() => onPastaDragLeave('sem')}
            onDrop={(e) => onPastaDrop(e, 'sem')}
            title="Pesquisas sem pasta"
          >
            <Folder size={16} />
            <span>Sem pasta</span>
          </div>

          {pastas.map((p) => (
            <div
              key={p.pastaId}
              className={[
                styles.pastaChip,
                styles.pastaChipRow,
                pastaSelecionada === p.pastaId ? styles.pastaChipActive : '',
                pastaHover === p.pastaId ? styles.pastaChipHover : '',
                pastaPulse === p.pastaId ? styles.pastaChipPulse : ''
              ].join(' ')}
              onClick={() => setPastaSelecionada(p.pastaId)}
              onDragOver={allowDrop}
              onDragEnter={() => onPastaDragEnter(p.pastaId)}
              onDragLeave={() => onPastaDragLeave(p.pastaId)}
              onDrop={(e) => onPastaDrop(e, p.pastaId)}
              title={`Solte aqui para mover para "${p.nome}"`}
            >
              <Folder size={16} />
              <span>{p.nome}</span>

              <span className={styles.pastaActions}>
                <button
                  className={styles.iconBtn}
                  title="Renomear pasta"
                  onClick={(e) => { e.stopPropagation(); abrirRenomear(p); }}
                >
                  <Pencil size={14} />
                </button>
                <button
                  className={`${styles.iconBtn} ${styles.navDanger}`}
                  title="Excluir pasta"
                  onClick={(e) => { e.stopPropagation(); abrirExcluir(p); }}
                >
                  <Trash2 size={14} />
                </button>
              </span>
            </div>
          ))}
        </section>

        {/* Dica de arraste */}
        {draggingId && <div className={styles.dragHint}>Arraste e solte em uma pasta para organizar</div>}

        {/* Grid */}
        <section className={styles.gridArea}>
          {loading ? (
            <div className={styles.loading}>Carregando suas pesquisas…</div>
          ) : listaFiltrada.length === 0 ? (
            <div className={styles.empty}>
              <div className={styles.emptyCard}>
                <h3>Nada por aqui ainda</h3>
                <p>Crie sua primeira pesquisa, ajuste os filtros ou selecione outra pasta.</p>
                <div className={styles.emptyActions}>
                  <button className={styles.ctaGhost} onClick={() => navigate('/criar')}>
                    <Plus size={16} /> Criar pesquisa
                  </button>
                  <button className={styles.ctaGhost} onClick={abrirModalPasta}>
                    <FolderPlus size={16} /> Nova pasta
                  </button>
                </div>
              </div>
            </div>
          ) : (
            <div className={styles.grid}>
              {listaFiltrada.map((p) => {
                const pid = getPid(p);
                const descTipo = p?.tipoPesquisa?.descricao ?? (tipoLookup.get(Number(getTipoId(p))) || 'Sem tipo');
                return (
                  <article
                    key={pid}
                    className={styles.card}
                    draggable
                    onDragStart={(e) => pid && onDragStart(e, pid)}
                    onDragEnd={onDragEnd}
                    onClick={() => {
                      if (draggingId) return;
                      if (!pid) return;
                      navigate(`/minhas-pesquisas/resultado/${pid}`);
                    }}
                  >
                    <div className={styles.cardHead}>
                      <span className={styles.badge}>{descTipo || 'Sem tipo'}</span>
                      <span className={styles.folderTag} title={p.pastaId ? labelPasta(p.pastaId) : 'Sem pasta'}>
                        <Folder size={14} />
                        <i>{p.pastaId ? labelPasta(p.pastaId) : 'Sem pasta'}</i>
                      </span>
                    </div>

                    <h3 className={styles.cardTitle}>{p.titulo || 'Sem título'}</h3>
                    {p.descricao && <p className={styles.cardDesc}>{p.descricao}</p>}

                    <div className={styles.cardFoot}>
                      <span className={styles.meta}>
                        {new Date(p.dataCriacao || p.criadoEm || p.datacriacao || Date.now()).toLocaleDateString('pt-BR')}
                      </span>
                      <div className={styles.cardActions}>
                        <button
                          className={`${styles.iconBtn} ${styles.navDanger}`}
                          title="Excluir pesquisa"
                          onClick={(e) => { e.stopPropagation(); abrirExcluirPesquisa(p); }}
                        >
                          <Trash2 size={14} />
                        </button>
                        <span className={styles.more}>
                          Abrir <ChevronRight size={16} />
                        </span>
                      </div>
                    </div>
                  </article>
                );
              })}
            </div>
          )}
        </section>
      </main>

      {/* Modal: Nova Pasta */}
      {isNovaPastaOpen && (
        <div className={styles.modalOverlay} onMouseDown={() => setIsNovaPastaOpen(false)}>
          <div
            className={styles.modalCard}
            role="dialog"
            aria-modal="true"
            aria-labelledby="nova-pasta-title"
            onMouseDown={(e) => e.stopPropagation()}
          >
            <div className={styles.modalHeader}>
              <h3 id="nova-pasta-title">Nova pasta</h3>
              <button aria-label="Fechar" className={styles.iconBtn} onClick={() => setIsNovaPastaOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <div className={styles.modalBody}>
              <label className={styles.fieldLabel}>Nome da pasta</label>
              <input
                ref={inputRef}
                type="text"
                maxLength={60}
                placeholder="Ex.: Relatórios, Provas, Clientes..."
                value={novoNomePasta}
                onChange={(e) => setNovoNomePasta(e.target.value)}
                className={styles.textInput}
              />
              <small className={styles.helpText}>
                Dica: crie pastas por equipe, cliente ou período para manter tudo organizado.
              </small>
            </div>

            <div className={styles.modalFooter}>
              <button className={styles.btnGhost} onClick={() => setIsNovaPastaOpen(false)}>
                Cancelar
              </button>
              <button
                className={styles.btnPrimary}
                onClick={criarPastaConfirm}
                disabled={!novoNomePasta.trim()}
                title={!novoNomePasta.trim() ? 'Digite um nome para criar' : 'Criar pasta'}
              >
                Criar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Renomear Pasta */}
      {isRenomearOpen && (
        <div className={styles.modalOverlay} onMouseDown={() => setIsRenomearOpen(false)}>
          <div
            className={styles.modalCard}
            role="dialog"
            aria-modal="true"
            aria-labelledby="renomear-pasta-title"
            onMouseDown={(e) => e.stopPropagation()}
          >
            <div className={styles.modalHeader}>
              <h3 id="renomear-pasta-title">Renomear pasta</h3>
              <button aria-label="Fechar" className={styles.iconBtn} onClick={() => setIsRenomearOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <div className={styles.modalBody}>
              <label className={styles.fieldLabel}>Novo nome</label>
              <input
                type="text"
                maxLength={60}
                value={novoNomeEdicao}
                onChange={(e) => setNovoNomeEdicao(e.target.value)}
                className={styles.textInput}
              />
              <small className={styles.helpText}>
                Renomeando: <b>{pastaAlvo?.nome}</b>
              </small>
            </div>

            <div className={styles.modalFooter}>
              <button className={styles.btnGhost} onClick={() => setIsRenomearOpen(false)}>
                Cancelar
              </button>
              <button
                className={styles.btnPrimary}
                onClick={confirmarRenomear}
                disabled={!novoNomeEdicao.trim()}
                title={!novoNomeEdicao.trim() ? 'Digite um nome' : 'Salvar'}
              >
                Salvar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Excluir Pasta */}
      {isExcluirOpen && (
        <div className={styles.modalOverlay} onMouseDown={() => setIsExcluirOpen(false)}>
          <div
            className={styles.modalCard}
            role="dialog"
            aria-modal="true"
            aria-labelledby="excluir-pasta-title"
            onMouseDown={(e) => e.stopPropagation()}
          >
            <div className={styles.modalHeader}>
              <h3 id="excluir-pasta-title">Excluir pasta</h3>
              <button aria-label="Fechar" className={styles.iconBtn} onClick={() => setIsExcluirOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <div className={styles.modalBody}>
              <p>Tem certeza que deseja excluir a pasta <b>{pastaAlvo?.nome}</b>?</p>
              <small className={styles.helpText}>
                As pesquisas não serão apagadas — elas apenas ficarão “Sem pasta”.
              </small>
            </div>

            <div className={styles.modalFooter}>
              <button className={styles.btnGhost} onClick={() => setIsExcluirOpen(false)}>
                Cancelar
              </button>
              <button className={`${styles.btnPrimary} ${styles.navDanger}`} onClick={confirmarExcluir}>
                Excluir
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: Excluir Pesquisa */}
      {isExcluirPesquisaOpen && (
        <div className={styles.modalOverlay} onMouseDown={() => setIsExcluirPesquisaOpen(false)}>
          <div
            className={styles.modalCard}
            role="dialog"
            aria-modal="true"
            aria-labelledby="excluir-pesquisa-title"
            onMouseDown={(e) => e.stopPropagation()}
          >
            <div className={styles.modalHeader}>
              <h3 id="excluir-pesquisa-title">Excluir pesquisa</h3>
              <button aria-label="Fechar" className={styles.iconBtn} onClick={() => setIsExcluirPesquisaOpen(false)}>
                <X size={18} />
              </button>
            </div>

            <div className={styles.modalBody}>
              <p>Tem certeza que deseja excluir a pesquisa <b>"{pesquisaAlvo?.titulo}"</b>?</p>
              <small className={styles.helpText}>
                Esta ação não pode ser desfeita. Todas as respostas e dados da pesquisa serão perdidos permanentemente.
              </small>
            </div>

            <div className={styles.modalFooter}>
              <button className={styles.btnGhost} onClick={() => setIsExcluirPesquisaOpen(false)}>
                Cancelar
              </button>
              <button className={`${styles.btnPrimary} ${styles.navDanger}`} onClick={confirmarExcluirPesquisa}>
                Excluir permanentemente
              </button>
            </div>
          </div>
        </div>
      )}

      <ToastContainer
        position="top-right"
        autoClose={2200}
        hideProgressBar={false}
        newestOnTop
        closeOnClick
        pauseOnFocusLoss={false}
        draggable
        pauseOnHover
        theme="dark"
      />
    </div>
  );
};

export default HomePage;