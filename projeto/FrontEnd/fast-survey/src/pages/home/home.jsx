import React, { useEffect, useMemo, useState, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Plus, User, LogOut, Info, Search, Filter, ChevronRight, Tag,
  FolderPlus, Folder, FolderOpen, X
} from 'lucide-react';
import axios from 'axios';
import styles from './home.module.css';

const API_BASE = 'http://localhost:5062';

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
/* =================================================== */

const HomePage = () => {
  const navigate = useNavigate();

  // Dados
  const [pesquisas, setPesquisas] = useState([]);
  const [pastas, setPastas] = useState([]); // [{ pastaId, nome }]
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

  const loginId = useMemo(() => parseInt(localStorage.getItem('userId')), []);
  const token = useMemo(() => localStorage.getItem('token'), []);

  const MAP_STORAGE_KEY = useMemo(
    () => `fs_pastas_map_user_${loginId || 'anon'}`,
    [loginId]
  );

  const ensureLocalMapSync = useCallback((remotePastas) => {
    const map = readLocalMap(MAP_STORAGE_KEY);
    if (Array.isArray(remotePastas)) {
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: remotePastas,
        vinculos: map.vinculos || {},
      });
    }
  }, [MAP_STORAGE_KEY]);

  /* === Carregamento inicial === */
  useEffect(() => {
    const bootstrap = async () => {
      if (!loginId || !token) {
        navigate('/login');
        return;
      }
      try {
        // Pesquisas do usuário
        const { data: pesquisasData } = await axios.get(
          `${API_BASE}/api/pesquisas/usuario/${loginId}`,
          { headers: { Authorization: `Bearer ${token}` } }
        );
        setPesquisas(Array.isArray(pesquisasData) ? pesquisasData : []);
      } catch (err) {
        console.error('Erro ao buscar pesquisas:', err);
        setPesquisas([]);
      }

      try {
        // Pastas do usuário (API oficial)
        const { data: pastasData } = await axios.get(
          `${API_BASE}/api/pastas`,
          { params: { loginid: loginId }, headers: { Authorization: `Bearer ${token}` } }
        );
        // API já devolve [{ pastaId, nome }]
        setPastas(Array.isArray(pastasData) ? pastasData : []);
        ensureLocalMapSync(Array.isArray(pastasData) ? pastasData : []);
      } catch (err) {
        console.warn('Falha ao carregar pastas do backend. Usando cache local.', err);
        const map = readLocalMap(MAP_STORAGE_KEY);
        setPastas(map.pastas || []);
      } finally {
        setLoading(false);
      }
    };
    bootstrap();
  }, [loginId, token, navigate, ensureLocalMapSync, MAP_STORAGE_KEY]);

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
      // OBS: backend espera { nome, loginid }
      const { data } = await axios.post(
        `${API_BASE}/api/pastas`,
        { nome, loginid: loginId },
        { headers: { Authorization: `Bearer ${token}` } }
      ); // retorna { pastaId, nome }

      const nova = data;
      setPastas((prev) => [...prev, nova]);

      const map = readLocalMap(MAP_STORAGE_KEY);
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: [...(map.pastas || []), nova],
        vinculos: map.vinculos || {},
      });
    } catch (err) {
      console.error('Falha ao criar pasta no backend. Caindo para cache local.', err);
      const nova = { pastaId: crypto.randomUUID(), nome }; // fallback local
      setPastas((prev) => [...prev, nova]);
      const map = readLocalMap(MAP_STORAGE_KEY);
      writeLocalMap(MAP_STORAGE_KEY, {
        pastas: [...(map.pastas || []), nova],
        vinculos: map.vinculos || {},
      });
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
        `${API_BASE}/api/pesquisas/${pesquisaId}/mover-pasta`,
        { pastaId: destinoId },
        { headers: { Authorization: `Bearer ${token}` } }
      );

      setPesquisas((prev) =>
        prev.map((p) => (p.pesquisaid === pesquisaId ? { ...p, pastaId: destinoId } : p))
      );

      const map = readLocalMap(MAP_STORAGE_KEY);
      const vinc = { ...(map.vinculos || {}) };
      if (destinoId) vinc[pesquisaId] = destinoId; else delete vinc[pesquisaId];
      writeLocalMap(MAP_STORAGE_KEY, { pastas: map.pastas || [], vinculos: vinc });
    } catch (err) {
      // fallback local
      const map = readLocalMap(MAP_STORAGE_KEY);
      const vinc = { ...(map.vinculos || {}) };
      if (destinoId) vinc[pesquisaId] = destinoId; else delete vinc[pesquisaId];
      writeLocalMap(MAP_STORAGE_KEY, { pastas: map.pastas || [], vinculos: vinc });

      setPesquisas((prev) =>
        prev.map((p) => (p.pesquisaid === pesquisaId ? { ...p, pastaId: destinoId } : p))
      );
    } finally {
      setPastaPulse(pastaIdOrSem);
      setTimeout(() => setPastaPulse(null), 900);
    }
  };

  /* === Aplica vínculo local quando API não traz pastaId === */
  const pesquisasComPastaVinculada = useMemo(() => {
    const map = readLocalMap(MAP_STORAGE_KEY);
    const vinc = map.vinculos || {};
    return (pesquisas || []).map((p) => {
      const pastaId = typeof p.pastaId !== 'undefined' ? p.pastaId : vinc[p.pesquisaid] || null;
      return { ...p, pastaId };
    });
  }, [pesquisas, MAP_STORAGE_KEY]);

  /* === Filtros === */
  const tiposDisponiveis = useMemo(() => {
    const set = new Set(
      pesquisasComPastaVinculada
        .map((p) => p?.tipoPesquisa?.descricao)
        .filter((t) => t && typeof t === 'string')
    );
    return ['todos', ...Array.from(set)];
  }, [pesquisasComPastaVinculada]);

  const listaFiltrada = useMemo(() => {
    let base = [...pesquisasComPastaVinculada];

    if (query.trim()) {
      const q = query.trim().toLowerCase();
      base = base.filter(
        (p) =>
          p?.titulo?.toLowerCase().includes(q) ||
          p?.descricao?.toLowerCase().includes(q) ||
          p?.tipoPesquisa?.descricao?.toLowerCase().includes(q)
      );
    }

    if (filtroTipo !== 'todos') {
      base = base.filter((p) => p?.tipoPesquisa?.descricao === filtroTipo);
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
      base.sort(
        (a, b) =>
          new Date(b.dataCriacao || b.criadoEm || 0) -
          new Date(a.dataCriacao || a.criadoEm || 0)
      );
    }

    return base;
  }, [pesquisasComPastaVinculada, query, filtroTipo, ordenarPor, pastaSelecionada]);

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
  const onPastaDragLeave = (key) => { if (dragOverPastaRef.current === key) dragOverPastaRef.current = null; setPastaHover((prev) => (prev === key ? null : prev)); };
  const onPastaDrop = (e, key) => { e.preventDefault(); const id = parseInt(e.dataTransfer.getData('text/plain')); if (!id) return; movePesquisaToPasta(id, key); dragOverPastaRef.current = null; setPastaHover(null); };

  const labelPasta = useCallback((pastaId) => {
    if (!pastaId) return 'Sem pasta';
    const p = pastas.find((x) => x.pastaId === pastaId);
    return p ? p.nome : 'Pasta';
  }, [pastas]);

  const handleLogout = () => { localStorage.clear(); navigate('/login'); };

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
              {listaFiltrada.map((p) => (
                <article
                  key={p.pesquisaid}
                  className={styles.card}
                  draggable
                  onDragStart={(e) => onDragStart(e, p.pesquisaid)}
                  onDragEnd={onDragEnd}
                  onClick={() => {
                    if (draggingId) return;
                    navigate(`/minhas-pesquisas/resultado/${p.pesquisaid}`);
                  }}
                >
                  <div className={styles.cardHead}>
                    <span className={styles.badge}>
                      {p?.tipoPesquisa?.descricao || 'Sem tipo'}
                    </span>
                    <span className={styles.folderTag} title={p.pastaId ? labelPasta(p.pastaId) : 'Sem pasta'}>
                      <Folder size={14} />
                      <i>{p.pastaId ? labelPasta(p.pastaId) : 'Sem pasta'}</i>
                    </span>
                  </div>

                  <h3 className={styles.cardTitle}>{p.titulo || 'Sem título'}</h3>
                  {p.descricao && <p className={styles.cardDesc}>{p.descricao}</p>}

                  <div className={styles.cardFoot}>
                    <span className={styles.meta}>
                      {new Date(p.dataCriacao || p.criadoEm || Date.now()).toLocaleDateString('pt-BR')}
                    </span>
                    <span className={styles.more}>
                      Abrir <ChevronRight size={16} />
                    </span>
                  </div>
                </article>
              ))}
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
    </div>
  );
};

export default HomePage;
