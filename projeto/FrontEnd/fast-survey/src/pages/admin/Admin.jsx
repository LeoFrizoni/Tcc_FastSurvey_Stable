
import React, { useEffect, useState } from 'react';
import styles from './admin.module.css';

const API = 'http://localhost:5062/api';

const AdminPage = () => {
  const [aba, setAba] = useState('usuarios');
  const [usuarios, setUsuarios] = useState([]);
  const [tiposUsuario, setTiposUsuario] = useState([]);
  const [tiposPesquisa, setTiposPesquisa] = useState([]);
  const [filtro, setFiltro] = useState('');
  const [edit, setEdit] = useState({});
  const [novo, setNovo] = useState({ usuario: '', email: '', senha: '', tipousuario: '', tipopesquisa: '' });

  useEffect(() => {
    if (aba === 'usuarios') fetchUsuarios();
    if (aba === 'tipousuario') fetchTiposUsuario();
    if (aba === 'tipopesquisa') fetchTiposPesquisa();
  }, [aba]);

  async function fetchUsuarios() {
    const res = await fetch(`${API}/login/listarlogins`);
    setUsuarios(await res.json());
  }
  async function fetchTiposUsuario() {
    const res = await fetch(`${API}/tipousuario/Listar`);
    setTiposUsuario(await res.json());
  }
  async function fetchTiposPesquisa() {
    const res = await fetch(`${API}/tipopesquisa/ListarTipoPesquisa`);
    setTiposPesquisa(await res.json());
  }

  // CRUD Usuário
  function startEditUsuario(u) {
    setEdit({ ...u, type: 'usuario' });
  }
  async function saveUsuario() {
    await fetch(`${API}/login/alterarloginporid/${edit.loginid}`, {
      method: 'PUT',
      body: JSON.stringify(edit),
      headers: { 'Content-Type': 'application/json' },
    });
    setEdit({});
    fetchUsuarios();
  }
  async function deleteUsuario(id) {
    if (!window.confirm('Excluir usuário?')) return;
    await fetch(`${API}/login/excluirlogin/${id}`, { method: 'DELETE' });
    fetchUsuarios();
  }
  async function addUsuario() {
    if (!novo.usuario || !novo.email || !novo.senha) return;
    await fetch(`${API}/login/cadastrar`, {
      method: 'POST',
      body: JSON.stringify(novo),
      headers: { 'Content-Type': 'application/json' },
    });
    setNovo({ ...novo, usuario: '', email: '', senha: '' });
    fetchUsuarios();
  }

  // CRUD TipoUsuario
  function startEditTipoUsuario(t) {
    setEdit({ ...t, type: 'tipousuario' });
  }
  async function saveTipoUsuario() {
    await fetch(`${API}/tipousuario/Alterar/${edit.usuarioid}`, {
      method: 'PUT',
      body: JSON.stringify({ tipousuario1: edit.tipousuario1 }),
      headers: { 'Content-Type': 'application/json' },
    });
    setEdit({});
    fetchTiposUsuario();
  }
  async function deleteTipoUsuario(id) {
    if (!window.confirm('Excluir tipo de usuário?')) return;
    await fetch(`${API}/tipousuario/Excluir/${id}`, { method: 'DELETE' });
    fetchTiposUsuario();
  }
  async function addTipoUsuario() {
    if (!novo.tipousuario) return;
    await fetch(`${API}/tipousuario/Cadastrar`, {
      method: 'POST',
      body: JSON.stringify({ tipousuario1: novo.tipousuario }),
      headers: { 'Content-Type': 'application/json' },
    });
    setNovo({ ...novo, tipousuario: '' });
    fetchTiposUsuario();
  }

  // CRUD TipoPesquisa
  function startEditTipoPesquisa(t) {
    setEdit({ ...t, type: 'tipopesquisa' });
  }
  async function saveTipoPesquisa() {
    await fetch(`${API}/tipopesquisa/AlterarTipoPesquisaPorId/${edit.tipopesquisaid}`, {
      method: 'PUT',
      body: JSON.stringify({ tipopesquisa1: edit.tipopesquisa1 }),
      headers: { 'Content-Type': 'application/json' },
    });
    setEdit({});
    fetchTiposPesquisa();
  }
  async function deleteTipoPesquisa(id) {
    if (!window.confirm('Excluir tipo de pesquisa?')) return;
    await fetch(`${API}/tipopesquisa/ExcluirTipoPesquisa/${id}`, { method: 'DELETE' });
    fetchTiposPesquisa();
  }
  async function addTipoPesquisa() {
    if (!novo.tipopesquisa) return;
    await fetch(`${API}/tipopesquisa/CadastrarTipoPesquisa`, {
      method: 'POST',
      body: JSON.stringify({ tipopesquisa1: novo.tipopesquisa }),
      headers: { 'Content-Type': 'application/json' },
    });
    setNovo({ ...novo, tipopesquisa: '' });
    fetchTiposPesquisa();
  }

  // Render helpers
  function renderUsuarios() {
    return (
  <div className={styles['admin-section']}>
        <h2>Usuários</h2>
  <div className={styles['admin-form']}>
          <input placeholder="Usuário" value={novo.usuario} onChange={e => setNovo({ ...novo, usuario: e.target.value })} />
          <input placeholder="Email" value={novo.email} onChange={e => setNovo({ ...novo, email: e.target.value })} />
          <input placeholder="Senha" type="password" value={novo.senha} onChange={e => setNovo({ ...novo, senha: e.target.value })} />
          <button onClick={addUsuario}>Adicionar</button>
        </div>
        <table>
          <thead>
            <tr><th>ID</th><th>Usuário</th><th>Email</th><th>Ações</th></tr>
          </thead>
          <tbody>
            {usuarios.filter(u => u.usuario?.toLowerCase().includes(filtro.toLowerCase())).map(u => (
              <tr key={u.loginid}>
                <td>{u.loginid}</td>
                <td>{edit.loginid === u.loginid ? <input value={edit.usuario} onChange={e => setEdit({ ...edit, usuario: e.target.value })} /> : u.usuario}</td>
                <td>{edit.loginid === u.loginid ? <input value={edit.email} onChange={e => setEdit({ ...edit, email: e.target.value })} /> : u.email}</td>
                <td>
                  {edit.loginid === u.loginid ? (
                    <>
                      <button onClick={saveUsuario}>Salvar</button>
                      <button onClick={() => setEdit({})}>Cancelar</button>
                    </>
                  ) : (
                    <>
                      <button onClick={() => startEditUsuario(u)}>Editar</button>
                      <button onClick={() => deleteUsuario(u.loginid)}>Excluir</button>
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
          <input placeholder="Novo tipo de usuário" value={novo.tipousuario} onChange={e => setNovo({ ...novo, tipousuario: e.target.value })} />
          <button onClick={addTipoUsuario}>Adicionar</button>
        </div>
        <table>
          <thead>
            <tr><th>ID</th><th>Nome</th><th>Ações</th></tr>
          </thead>
          <tbody>
            {tiposUsuario.filter(t => t.tipousuario1?.toLowerCase().includes(filtro.toLowerCase())).map(t => (
              <tr key={t.usuarioid}>
                <td>{t.usuarioid}</td>
                <td>{edit.usuarioid === t.usuarioid ? <input value={edit.tipousuario1} onChange={e => setEdit({ ...edit, tipousuario1: e.target.value })} /> : t.tipousuario1}</td>
                <td>
                  {edit.usuarioid === t.usuarioid ? (
                    <>
                      <button onClick={saveTipoUsuario}>Salvar</button>
                      <button onClick={() => setEdit({})}>Cancelar</button>
                    </>
                  ) : (
                    <>
                      <button onClick={() => startEditTipoUsuario(t)}>Editar</button>
                      <button onClick={() => deleteTipoUsuario(t.usuarioid)}>Excluir</button>
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
          <input placeholder="Novo tipo de pesquisa" value={novo.tipopesquisa} onChange={e => setNovo({ ...novo, tipopesquisa: e.target.value })} />
          <button onClick={addTipoPesquisa}>Adicionar</button>
        </div>
        <table>
          <thead>
            <tr><th>ID</th><th>Nome</th><th>Ações</th></tr>
          </thead>
          <tbody>
            {tiposPesquisa.filter(t => t.tipopesquisa1?.toLowerCase().includes(filtro.toLowerCase())).map(t => (
              <tr key={t.tipopesquisaid}>
                <td>{t.tipopesquisaid}</td>
                <td>{edit.tipopesquisaid === t.tipopesquisaid ? <input value={edit.tipopesquisa1} onChange={e => setEdit({ ...edit, tipopesquisa1: e.target.value })} /> : t.tipopesquisa1}</td>
                <td>
                  {edit.tipopesquisaid === t.tipopesquisaid ? (
                    <>
                      <button onClick={saveTipoPesquisa}>Salvar</button>
                      <button onClick={() => setEdit({})}>Cancelar</button>
                    </>
                  ) : (
                    <>
                      <button onClick={() => startEditTipoPesquisa(t)}>Editar</button>
                      <button onClick={() => deleteTipoPesquisa(t.tipopesquisaid)}>Excluir</button>
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
  <input className={styles['admin-search']} placeholder="Buscar..." value={filtro} onChange={e => setFiltro(e.target.value)} />
  <button className={styles['logout-btn']} onClick={() => { localStorage.clear(); window.location.href = '/login'; }}>Logout</button>
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