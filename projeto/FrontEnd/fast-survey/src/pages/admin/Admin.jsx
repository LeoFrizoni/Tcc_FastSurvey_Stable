import React, { useEffect, useState, useRef } from 'react';
import styles from './admin.module.css';

const API = 'http://localhost:5062/api';

/** Lê a resposta em segurança:
 * - Se 204/205 ou body vazio => retorna valor padrão ([], {} ou null conforme passado)
 * - Se não for ok => lança erro com status
 * - Se vier texto mas não for JSON válido => lança erro claro
 */
async function fetchJSONSafe(input, init, { defaultValue = [], expect = 'array' } = {}) {
  const res = await fetch(input, init);

  // 204/205: sem conteúdo
  if (res.status === 204 || res.status === 205) return defaultValue;

  const text = await res.text();

  if (!res.ok) {
    // Se o servidor mandou um HTML/erro, mostramos um pedaço para debug
    throw new Error(
      `HTTP ${res.status} ${res.statusText} em ${input}\n` +
      (text?.slice(0, 200) || '(sem corpo)')
    );
  }

  // Corpo vazio: trate como default
  if (!text || text.trim() === '') return defaultValue;

  // Tenta parsear JSON
  let data;
  try {
    data = JSON.parse(text);
  } catch (e) {
    throw new Error(
      `Resposta não é JSON válido em ${input}:\n` + text.slice(0, 200)
    );
  }

  // Garantia de tipo esperado para evitar quebras no render
  if (expect === 'array' && !Array.isArray(data)) return defaultValue;
  if (expect === 'object' && (data === null || Array.isArray(data) || typeof data !== 'object')) {
    return defaultValue ?? {};
  }

  return data;
}

const AdminPage = () => {
  const [aba, setAba] = useState('usuarios');
  const [usuarios, setUsuarios] = useState([]);
  const [tiposUsuario, setTiposUsuario] = useState([]);
  const [tiposPesquisa, setTiposPesquisa] = useState([]);
  const [filtro, setFiltro] = useState('');
  const [edit, setEdit] = useState({});
  const [novo, setNovo] = useState({ usuario: '', email: '', senha: '', tipousuario: '', tipopesquisa: '' });
  const abortRef = useRef(null);

  useEffect(() => {
    // cancela requisições anteriores ao trocar de aba
    if (abortRef.current) abortRef.current.abort();
    const controller = new AbortController();
    abortRef.current = controller;

    (async () => {
      try {
        if (aba === 'usuarios') {
          const data = await fetchJSONSafe(`${API}/login`, { signal: controller.signal }, { defaultValue: [], expect: 'array' });
          setUsuarios(data);
        }
        if (aba === 'tipousuario') {
          const data = await fetchJSONSafe(`${API}/tipousuario`, { signal: controller.signal }, { defaultValue: [], expect: 'array' });
          setTiposUsuario(data);
        }
        if (aba === 'tipopesquisa') {
          const data = await fetchJSONSafe(`${API}/TipoPesquisa/ListarTipoPesquisa`, { signal: controller.signal }, { defaultValue: [], expect: 'array' });
          setTiposPesquisa(data);
        }
      } catch (err) {
        if (err.name === 'AbortError') return;
        console.error(err);
        alert(`Erro ao carregar dados: ${err.message}`);
      }
    })();

    return () => controller.abort();
  }, [aba]);

  // Não chamar diretamente res.json() em lugar nenhum daqui pra baixo.

  // ----- FETCHERS EXPLÍCITOS (reuso) -----
  async function fetchUsuarios() {
    try {
  const data = await fetchJSONSafe(`${API}/login`, {}, { defaultValue: [], expect: 'array' });
      setUsuarios(data);
    } catch (err) {
      console.error(err);
      alert(`Erro ao carregar usuários: ${err.message}`);
      setUsuarios([]); // evita .map em undefined
    }
  }
  async function fetchTiposUsuario() {
    try {
  const data = await fetchJSONSafe(`${API}/tipousuario/Listar`, {}, { defaultValue: [], expect: 'array' });
      setTiposUsuario(data);
    } catch (err) {
      console.error(err);
      alert(`Erro ao carregar tipos de usuário: ${err.message}`);
      setTiposUsuario([]);
    }
  }
  async function fetchTiposPesquisa() {
    try {
      const data = await fetchJSONSafe(`${API}/TipoPesquisa/ListarTipoPesquisa`, {}, { defaultValue: [], expect: 'array' });
      setTiposPesquisa(data);
    } catch (err) {
      console.error(err);
      alert(`Erro ao carregar tipos de pesquisa: ${err.message}`);
      setTiposPesquisa([]);
    }
  }

  // ----- CRUD Usuário -----
  function startEditUsuario(u) {
    setEdit({ ...u, type: 'usuario' });
  }
  async function saveUsuario() {
    try {
      await fetchJSONSafe(
        `${API}/login/alterarloginporid/${edit.LoginId}`,
        {
          method: 'PUT',
          body: JSON.stringify(edit),
          headers: { 'Content-Type': 'application/json' },
        },
        { defaultValue: null, expect: 'object' }
      );
      setEdit({});
      await fetchUsuarios();
    } catch (err) {
      console.error(err);
      alert(`Erro ao salvar usuário: ${err.message}`);
    }
  }
  async function deleteUsuario(id) {
    if (!window.confirm('Excluir usuário?')) return;
    try {
      await fetchJSONSafe(`${API}/login/excluirlogin/${id}`, { method: 'DELETE' }, { defaultValue: null, expect: 'object' });
      await fetchUsuarios();
    } catch (err) {
      console.error(err);
      alert(`Erro ao excluir usuário: ${err.message}`);
    }
  }
  async function addUsuario() {
    if (!novo.usuario || !novo.email || !novo.senha) {
      alert('Preencha usuário, email e senha.');
      return;
    }
    try {
      await fetchJSONSafe(
        `${API}/login/cadastrar`,
        {
          method: 'POST',
          body: JSON.stringify(novo),
          headers: { 'Content-Type': 'application/json' },
        },
        { defaultValue: null, expect: 'object' }
      );
      setNovo({ ...novo, usuario: '', email: '', senha: '' });
      await fetchUsuarios();
    } catch (err) {
      console.error(err);
      alert(`Erro ao adicionar usuário: ${err.message}`);
    }
  }

  // ----- CRUD TipoUsuario -----
  function startEditTipoUsuario(t) {
    setEdit({ ...t, type: 'tipousuario' });
  }
  async function saveTipoUsuario() {
    try {
      await fetchJSONSafe(
        `${API}/tipousuario/Alterar/${edit.TipoUsuarioId}`,
        {
          method: 'PUT',
          body: JSON.stringify({ tipousuario1: edit.tipousuario1 }),
          headers: { 'Content-Type': 'application/json' },
        },
        { defaultValue: null, expect: 'object' }
      );
      setEdit({});
      await fetchTiposUsuario();
    } catch (err) {
      console.error(err);
      alert(`Erro ao salvar tipo de usuário: ${err.message}`);
    }
  }
  async function deleteTipoUsuario(id) {
    if (!window.confirm('Excluir tipo de usuário?')) return;
    try {
      await fetchJSONSafe(`${API}/tipousuario/Excluir/${id}`, { method: 'DELETE' }, { defaultValue: null, expect: 'object' });
      await fetchTiposUsuario();
    } catch (err) {
      console.error(err);
      alert(`Erro ao excluir tipo de usuário: ${err.message}`);
    }
  }
  async function addTipoUsuario() {
    if (!novo.tipousuario) return;
    try {
      await fetchJSONSafe(
        `${API}/tipousuario/Cadastrar`,
        {
          method: 'POST',
          body: JSON.stringify({ tipousuario1: novo.tipousuario }),
          headers: { 'Content-Type': 'application/json' },
        },
        { defaultValue: null, expect: 'object' }
      );
      setNovo({ ...novo, tipousuario: '' });
      await fetchTiposUsuario();
    } catch (err) {
      console.error(err);
      alert(`Erro ao adicionar tipo de usuário: ${err.message}`);
    }
  }

  // ----- CRUD TipoPesquisa -----
  function startEditTipoPesquisa(t) {
    setEdit({ ...t, type: 'tipopesquisa' });
  }
  async function saveTipoPesquisa() {
    try {
      await fetchJSONSafe(
        `${API}/TipoPesquisa/AlterarTipoPesquisaPorId/${edit.TipoPesquisaId}`,
        {
          method: 'PUT',
          body: JSON.stringify({ tipopesquisa1: edit.tipopesquisa1 }),
          headers: { 'Content-Type': 'application/json' },
        },
        { defaultValue: null, expect: 'object' }
      );
      setEdit({});
      await fetchTiposPesquisa();
    } catch (err) {
      console.error(err);
      alert(`Erro ao salvar tipo de pesquisa: ${err.message}`);
    }
  }
  async function deleteTipoPesquisa(id) {
    if (!window.confirm('Excluir tipo de pesquisa?')) return;
    try {
      await fetchJSONSafe(`${API}/TipoPesquisa/ExcluirTipoPesquisa/${id}`, { method: 'DELETE' }, { defaultValue: null, expect: 'object' });
      await fetchTiposPesquisa();
    } catch (err) {
      console.error(err);
      alert(`Erro ao excluir tipo de pesquisa: ${err.message}`);
    }
  }
  async function addTipoPesquisa() {
    if (!novo.tipopesquisa) return;
    try {
      await fetchJSONSafe(
        `${API}/TipoPesquisa/CadastrarTipoPesquisa`,
        {
          method: 'POST',
          body: JSON.stringify({ tipopesquisa1: novo.tipopesquisa }),
          headers: { 'Content-Type': 'application/json' },
        },
        { defaultValue: null, expect: 'object' }
      );
      setNovo({ ...novo, tipopesquisa: '' });
      await fetchTiposPesquisa();
    } catch (err) {
      console.error(err);
      alert(`Erro ao adicionar tipo de pesquisa: ${err.message}`);
    }
  }

  // ----- Render helpers -----
  function renderUsuarios() {
    return (
      <div className={styles['admin-section']}>
        <h2>Usuários</h2>
        <div className={styles['admin-form']}>
          <input
            placeholder="Novo usuário"
            value={novo.usuario}
            onChange={e => setNovo({ ...novo, usuario: e.target.value })}
          />
          <input
            placeholder="Email"
            value={novo.email}
            onChange={e => setNovo({ ...novo, email: e.target.value })}
          />
          <input
            placeholder="Senha"
            type="password"
            value={novo.senha}
            onChange={e => setNovo({ ...novo, senha: e.target.value })}
          />
          <button onClick={addUsuario}>Adicionar</button>
        </div>
        <table>
          <thead>
            <tr><th>ID</th><th>Usuário</th><th>Email</th><th>Ações</th></tr>
          </thead>
          <tbody>
            {(usuarios || [])
              .filter(u => (u?.usuario || '').toLowerCase().includes(filtro.toLowerCase()))
              .map(u => (
                <tr key={u.LoginId}>
                  <td>{u.LoginId}</td>
                  <td>
                    {edit.LoginId === u.LoginId
                      ? <input value={edit.usuario ?? ''} onChange={e => setEdit({ ...edit, usuario: e.target.value })} />
                      : (u.usuario || '')}
                  </td>
                  <td>
                    {edit.LoginId === u.LoginId
                      ? <input value={edit.email ?? ''} onChange={e => setEdit({ ...edit, email: e.target.value })} />
                      : (u.email || '')}
                  </td>
                  <td>
                    {edit.LoginId === u.LoginId ? (
                      <>
                        <button onClick={saveUsuario}>Salvar</button>
                        <button onClick={() => setEdit({})}>Cancelar</button>
                      </>
                    ) : (
                      <>
                        <button onClick={() => startEditUsuario(u)}>Editar</button>
                        <button onClick={() => deleteUsuario(u.LoginId)}>Excluir</button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    );
  }

  function renderTiposUsuario() {
    return (
      <div className={styles['admin-section']}>
        <h2>Tipos de Usuário</h2>
        <div className={styles['admin-form']}>
          <input
            placeholder="Novo tipo de usuário"
            value={novo.tipousuario}
            onChange={e => setNovo({ ...novo, tipousuario: e.target.value })}
          />
          <button onClick={addTipoUsuario}>Adicionar</button>
        </div>
        <table>
          <thead>
            <tr><th>ID</th><th>Nome</th><th>Ações</th></tr>
          </thead>
          <tbody>
            {(tiposUsuario || [])
              .filter(t => (t?.tipousuario1 || '').toLowerCase().includes(filtro.toLowerCase()))
              .map(t => (
                <tr key={t.TipoUsuarioId}>
                  <td>{t.TipoUsuarioId}</td>
                  <td>
                    {edit.TipoUsuarioId === t.TipoUsuarioId
                      ? <input value={edit.tipousuario1 ?? ''} onChange={e => setEdit({ ...edit, tipousuario1: e.target.value })} />
                      : (t.tipousuario1 || '')}
                  </td>
                  <td>
                    {edit.TipoUsuarioId === t.TipoUsuarioId ? (
                      <>
                        <button onClick={saveTipoUsuario}>Salvar</button>
                        <button onClick={() => setEdit({})}>Cancelar</button>
                      </>
                    ) : (
                      <>
                        <button onClick={() => startEditTipoUsuario(t)}>Editar</button>
                        <button onClick={() => deleteTipoUsuario(t.TipoUsuarioId)}>Excluir</button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    );
  }

  function renderTiposPesquisa() {
    return (
      <div className={styles['admin-section']}>
        <h2>Tipos de Pesquisa</h2>
        <div className={styles['admin-form']}>
          <input
            placeholder="Novo tipo de pesquisa"
            value={novo.tipopesquisa}
            onChange={e => setNovo({ ...novo, tipopesquisa: e.target.value })}
          />
          <button onClick={addTipoPesquisa}>Adicionar</button>
        </div>
        <table>
          <thead>
            <tr><th>ID</th><th>Nome</th><th>Ações</th></tr>
          </thead>
          <tbody>
            {(tiposPesquisa || [])
              .filter(t => (t?.tipopesquisa1 || '').toLowerCase().includes(filtro.toLowerCase()))
              .map(t => (
                <tr key={t.TipoPesquisaId}>
                  <td>{t.TipoPesquisaId}</td>
                  <td>
                    {edit.TipoPesquisaId === t.TipoPesquisaId
                      ? <input value={edit.tipopesquisa1 ?? ''} onChange={e => setEdit({ ...edit, tipopesquisa1: e.target.value })} />
                      : (t.tipopesquisa1 || '')}
                  </td>
                  <td>
                    {edit.TipoPesquisaId === t.TipoPesquisaId ? (
                      <>
                        <button onClick={saveTipoPesquisa}>Salvar</button>
                        <button onClick={() => setEdit({})}>Cancelar</button>
                      </>
                    ) : (
                      <>
                        <button onClick={() => startEditTipoPesquisa(t)}>Editar</button>
                        <button onClick={() => deleteTipoPesquisa(t.TipoPesquisaId)}>Excluir</button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
          </tbody>
        </table>
      </div>
    );
  }

  return (
    <div className={styles['admin-main']}>
      <div className={styles['admin-navbar']}>
        <div className={styles['admin-navbar-title']}>Painel Administrativo</div>
        <div className={styles['admin-navbar-tabs']}>
          <button className={aba === 'usuarios' ? styles['active'] : ''} onClick={() => setAba('usuarios')}>Usuários</button>
          <button className={aba === 'tipousuario' ? styles['active'] : ''} onClick={() => setAba('tipousuario')}>Tipos de Usuário</button>
          <button className={aba === 'tipopesquisa' ? styles['active'] : ''} onClick={() => setAba('tipopesquisa')}>Tipos de Pesquisa</button>
        </div>
        <input
          className={styles['admin-search']}
          placeholder="Buscar..."
          value={filtro}
          onChange={e => setFiltro(e.target.value)}
        />
        <button
          className={styles['logout-btn']}
          onClick={() => { localStorage.clear(); window.location.href = '/login'; }}
        >
          Logout
        </button>
      </div>

      <div className={styles['admin-content']}>
        {aba === 'usuarios' && renderUsuarios()}
        {aba === 'tipousuario' && renderTiposUsuario()}
        {aba === 'tipopesquisa' && renderTiposPesquisa()}
      </div>
    </div>
  );
};

export default AdminPage;
