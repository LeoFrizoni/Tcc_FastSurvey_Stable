import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, User, LogOut, Info, Search, Filter, ChevronRight, Tag } from 'lucide-react';
import axios from 'axios';
import styles from './home.module.css';

const HomePage = () => {
  const navigate = useNavigate();
  const [pesquisas, setPesquisas] = useState([]);
  const [loading, setLoading] = useState(true);

  // UI state
  const [query, setQuery] = useState('');
  const [filtroTipo, setFiltroTipo] = useState('todos');
  const [ordenarPor, setOrdenarPor] = useState('recente');

  useEffect(() => {
    const buscarPesquisas = async () => {
      const loginId = parseInt(localStorage.getItem('userId'));
      const token = localStorage.getItem('token');
      if (!loginId || !token) {
        navigate('/login');
        return;
      }
      try {
        const { data } = await axios.get(`http://localhost:5062/api/pesquisas/usuario/${loginId}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        setPesquisas(Array.isArray(data) ? data : []);
      } catch (err) {
        console.error('Erro ao buscar pesquisas:', err);
      } finally {
        setLoading(false);
      }
    };
    buscarPesquisas();
  }, [navigate]);

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  const tiposDisponiveis = useMemo(() => {
    const set = new Set(
      pesquisas
        .map((p) => p?.tipoPesquisa?.descricao)
        .filter((t) => t && typeof t === 'string')
    );
    return ['todos', ...Array.from(set)];
  }, [pesquisas]);

  const listaFiltrada = useMemo(() => {
    let base = [...pesquisas];

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

    if (ordenarPor === 'a-z') {
      base.sort((a, b) => (a.titulo || '').localeCompare(b.titulo || ''));
    } else if (ordenarPor === 'z-a') {
      base.sort((a, b) => (b.titulo || '').localeCompare(a.titulo || ''));
    } else {
      base.sort((a, b) => new Date(b.dataCriacao || b.criadoEm || 0) - new Date(a.dataCriacao || a.criadoEm || 0));
    }

    return base;
  }, [pesquisas, query, filtroTipo, ordenarPor]);

  return (
    <div className={styles.wrapper}>
      {/* Sidebar */}
      <aside className={styles.sidebar}>
        <div className={styles.brand}>FastSurvey</div>
        <nav className={styles.nav}>
          <button
            type="button"
            className={`${styles.navItem} ${styles.navWhite}`}
            onClick={() => navigate('/perfil')}
          >
            <User size={18} /> <span>Perfil</span>
          </button>

          <button
            type="button"
            className={`${styles.navItem} ${styles.navWhite}`}
            onClick={() => navigate('/sobre-nos')}
          >
            <Info size={18} /> <span>Sobre Nós</span>
          </button>

          <div className={styles.navSpacer} />

          <button
            type="button"
            className={`${styles.navItem} ${styles.navWhite} ${styles.navDanger}`}
            onClick={handleLogout}
            aria-label="Sair"
          >
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

        {/* Grid */}
        <section className={styles.gridArea}>
          {loading ? (
            <div className={styles.loading}>Carregando suas pesquisas…</div>
          ) : listaFiltrada.length === 0 ? (
            <div className={styles.empty}>
              <div className={styles.emptyCard}>
                <h3>Nada por aqui ainda</h3>
                <p>Crie sua primeira pesquisa ou ajuste seus filtros e busca.</p>
                <button className={styles.ctaGhost} onClick={() => navigate('/criar')}>
                  <Plus size={16} /> Criar pesquisa
                </button>
              </div>
            </div>
          ) : (
            <div className={styles.grid}>
              {listaFiltrada.map((p) => (
                <article
                  key={p.pesquisaid}
                  className={styles.card}
                  onClick={() => navigate(`/minhas-pesquisas/resultado/${p.pesquisaid}`)}
                >
                  <div className={styles.cardHead}>
                    <span className={styles.badge}>
                      {p?.tipoPesquisa?.descricao || 'Sem tipo'}
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
    </div>
  );
};

export default HomePage;
