import React, { useEffect, useState } from 'react';
import axios from 'axios';
import './admin.css';
import { useNavigate } from 'react-router-dom';
import { FiLogOut } from 'react-icons/fi';

const AdminPage = () => {
  const [abaAtiva, setAbaAtiva] = useState('usuarios');

  const [usuarios, setUsuarios] = useState([]);
  const [editandoUsuario, setEditandoUsuario] = useState(null);
  const [usuarioEditado, setUsuarioEditado] = useState({ usuario: '', email: '', senha: '' });

  const [tiposUsuario, setTiposUsuario] = useState([]);
  const [editandoTipoUsuario, setEditandoTipoUsuario] = useState(null);
  const [tipoUsuarioEditado, setTipoUsuarioEditado] = useState('');
  const [novoTipoUsuario, setNovoTipoUsuario] = useState('');

  const [tiposPesquisa, setTiposPesquisa] = useState([]);
  const [editandoTipoPesquisa, setEditandoTipoPesquisa] = useState(null);
  const [tipoPesquisaEditado, setTipoPesquisaEditado] = useState('');
  const [novoTipoPesquisa, setNovoTipoPesquisa] = useState('');

  const [filtro, setFiltro] = useState('');

  const navigate = useNavigate();

  useEffect(() => {
    if (abaAtiva === 'usuarios') buscarUsuarios();
    if (abaAtiva === 'tipousuario') buscarTiposUsuario();
    if (abaAtiva === 'tipopesquisa') buscarTiposPesquisa();
  }, [abaAtiva]);

  const buscarUsuarios = async () => {
    try {
      const resposta = await axios.get('http://localhost:5062/api/login/listarlogins');
      setUsuarios(resposta.data);
    } catch (erro) {
      console.error('Erro ao buscar usuários:', erro);
    }
  };

  const buscarTiposUsuario = async () => {
    try {
      const resposta = await axios.get('http://localhost:5062/api/tipousuario/Listar');
      setTiposUsuario(resposta.data);
    } catch (erro) {
      console.error('Erro ao buscar tipos de usuário:', erro);
    }
  };

  const buscarTiposPesquisa = async () => {
    try {
      const resposta = await axios.get('http://localhost:5062/api/tipopesquisa/ListarTipoPesquisa');
      setTiposPesquisa(resposta.data);
    } catch (erro) {
      console.error('Erro ao buscar tipos de pesquisa:', erro);
    }
  };

  const excluirItem = async (id, tipo) => {
    const confirm = window.confirm('Tem certeza que deseja excluir este item?');
    if (!confirm) return;

    try {
      if (tipo === 'usuario') {
        await axios.delete(`http://localhost:5062/api/login/excluirlogin/${id}`);
        buscarUsuarios();
      } else if (tipo === 'tipousuario') {
        await axios.delete(`http://localhost:5062/api/tipousuario/Excluir/${id}`);
        buscarTiposUsuario();
      } else if (tipo === 'tipopesquisa') {
        await axios.delete(`http://localhost:5062/api/tipopesquisa/ExcluirTipoPesquisa/${id}`);
        buscarTiposPesquisa();
      }
    } catch (erro) {
      console.error(`Erro ao excluir ${tipo}:`, erro);
    }
  };

  const salvarUsuario = async (id) => {
    try {
      const formData = new FormData();
      formData.append('loginid', id);
      formData.append('usuario', usuarioEditado.usuario);
      formData.append('email', usuarioEditado.email);
      formData.append('senha', usuarioEditado.senha);
      await axios.put(`http://localhost:5062/api/login/alterarloginporid/${id}`, formData);
      setEditandoUsuario(null);
      buscarUsuarios();
    } catch (erro) {
      console.error('Erro ao editar usuário:', erro);
    }
  };

  const salvarTipoUsuario = async (id) => {
    try {
      await axios.put(`http://localhost:5062/api/tipousuario/Alterar/${id}`, {
        tipousuario1: tipoUsuarioEditado
      });
      setEditandoTipoUsuario(null);
      buscarTiposUsuario();
    } catch (erro) {
      console.error('Erro ao editar tipo de usuário:', erro);
    }
  };

  const salvarTipoPesquisa = async (id) => {
    try {
      const formData = new FormData();
      formData.append('tipopesquisaid', id);
      formData.append('tipopesquisa1', tipoPesquisaEditado);
      await axios.put(`http://localhost:5062/api/tipopesquisa/AlterarTipoPesquisaPorId/${id}`, formData);
      setEditandoTipoPesquisa(null);
      buscarTiposPesquisa();
    } catch (erro) {
      console.error('Erro ao editar tipo de pesquisa:', erro);
    }
  };

  const adicionarTipoUsuario = async () => {
    if (!novoTipoUsuario.trim()) return;
    try {
      await axios.post('http://localhost:5062/api/tipousuario/Cadastrar', { tipousuario1: novoTipoUsuario });
      setNovoTipoUsuario('');
      buscarTiposUsuario();
    } catch (erro) {
      console.error('Erro ao adicionar tipo de usuário:', erro);
    }
  };

  const adicionarTipoPesquisa = async () => {
    if (!novoTipoPesquisa.trim()) return;
    try {
      const formData = new FormData();
      formData.append('tipopesquisa1', novoTipoPesquisa);
      await axios.post('http://localhost:5062/api/tipopesquisa/CadastrarTipoPesquisa', formData);
      setNovoTipoPesquisa('');
      buscarTiposPesquisa();
    } catch (erro) {
      console.error('Erro ao adicionar tipo de pesquisa:', erro);
    }
  };

  return (
    <div className="admin-wrapper">
      <header className="admin-header">
        <h2>FastSurvey | Painel Admin</h2>
        <button className="btn-sair" aria-label="Sair" onClick={() => { localStorage.clear(); navigate('/login'); }}>
          <FiLogOut /> Sair
        </button>
      </header>

      <div className="admin-container">
        <div className="aba-selector">
          <button onClick={() => setAbaAtiva('usuarios')} className={abaAtiva === 'usuarios' ? 'ativo' : ''}>Usuários</button>
          <button onClick={() => setAbaAtiva('tipousuario')} className={abaAtiva === 'tipousuario' ? 'ativo' : ''}>Tipo Usuário</button>
          <button onClick={() => setAbaAtiva('tipopesquisa')} className={abaAtiva === 'tipopesquisa' ? 'ativo' : ''}>Tipo Pesquisa</button>
        </div>

        {abaAtiva === 'usuarios' && (
          <>
            <div className="admin-toolbar">
              <h1>Gerenciamento de Usuários</h1>
              <p>Total: {usuarios.length}</p>
              <input
                type="text"
                placeholder="Buscar por nome..."
                value={filtro}
                onChange={(e) => setFiltro(e.target.value)}
              />
            </div>

            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Usuário</th>
                  <th>Email</th>
                  <th>Senha</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {usuarios.filter(u => u.usuario?.toLowerCase().includes(filtro.toLowerCase())).map((u, index) => (
                  <tr key={u.loginid}>
                    <td>{index + 1}</td>
                    <td>
                      {editandoUsuario === u.loginid ? (
                        <input
                          value={usuarioEditado.usuario}
                          onChange={(e) => setUsuarioEditado({ ...usuarioEditado, usuario: e.target.value })}
                        />
                      ) : u.usuario}
                    </td>
                    <td>
                      {editandoUsuario === u.loginid ? (
                        <input
                          value={usuarioEditado.email}
                          onChange={(e) => setUsuarioEditado({ ...usuarioEditado, email: e.target.value })}
                        />
                      ) : u.email}
                    </td>
                    <td>
                      {editandoUsuario === u.loginid ? (
                        <input
                          type="password"
                          placeholder="Nova senha"
                          value={usuarioEditado.senha}
                          onChange={(e) => setUsuarioEditado({ ...usuarioEditado, senha: e.target.value })}
                        />
                      ) : '******'}
                    </td>
                    <td>
                      {editandoUsuario === u.loginid ? (
                        <>
                          <button onClick={() => salvarUsuario(u.loginid)}>Salvar</button>
                          <button onClick={() => setEditandoUsuario(null)}>Cancelar</button>
                          <button type="button" aria-label="Salvar usuário" onClick={() => salvarUsuario(u.loginid)}>Salvar</button>
                          <button type="button" aria-label="Cancelar edição" onClick={() => setEditandoUsuario(null)}>Cancelar</button>
                        </>
                      ) : (
                        <>
                          <button onClick={() => {
                            setEditandoUsuario(u.loginid);
                            setUsuarioEditado({ usuario: u.usuario, email: u.email, senha: '' });
                          }}>Editar</button>
                          <button onClick={() => excluirItem(u.loginid, 'usuario')}>Excluir</button>
                          <button type="button" aria-label="Editar usuário" onClick={() => {
                            setEditandoUsuario(u.loginid);
                            setUsuarioEditado({ usuario: u.usuario, email: u.email, senha: '' });
                          }}>Editar</button>
                          <button type="button" aria-label="Excluir usuário" onClick={() => excluirItem(u.loginid, 'usuario')}>Excluir</button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {abaAtiva === 'tipousuario' && (
          <>
            <div className="admin-toolbar">
              <h1>Gerenciamento de Tipo de Usuário</h1>
              <p>Total: {tiposUsuario.length}</p>
              <input
                type="text"
                placeholder="Buscar por nome..."
                value={filtro}
                onChange={(e) => setFiltro(e.target.value)}
              />
            </div>

            <div className="admin-add-form">
              <input
                type="text"
                placeholder="Novo Tipo de Usuário"
                value={novoTipoUsuario}
                onChange={(e) => setNovoTipoUsuario(e.target.value)}
              />
              <button onClick={adicionarTipoUsuario}>Adicionar</button>
            </div>

            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {tiposUsuario.filter(tipo => tipo.tipousuario1?.toLowerCase().includes(filtro.toLowerCase())).map((tipo, index) => (
                  <tr key={tipo.usuarioid}>
                    <td>{index + 1}</td>
                    <td>
                      {editandoTipoUsuario === tipo.usuarioid ? (
                        <input
                          value={tipoUsuarioEditado}
                          onChange={(e) => setTipoUsuarioEditado(e.target.value)}
                        />
                      ) : tipo.tipousuario1}
                    </td>
                    <td>
                      {editandoTipoUsuario === tipo.usuarioid ? (
                        <>
                          <button onClick={() => salvarTipoUsuario(tipo.usuarioid)}>Salvar</button>
                          <button onClick={() => setEditandoTipoUsuario(null)}>Cancelar</button>
                          <button type="button" aria-label="Salvar tipo de usuário" onClick={() => salvarTipoUsuario(tipo.usuarioid)}>Salvar</button>
                          <button type="button" aria-label="Cancelar edição" onClick={() => setEditandoTipoUsuario(null)}>Cancelar</button>
                        </>
                      ) : (
                        <>
                          <button onClick={() => {
                            setEditandoTipoUsuario(tipo.usuarioid);
                            setTipoUsuarioEditado(tipo.tipousuario1);
                          }}>Editar</button>
                          <button onClick={() => excluirItem(tipo.usuarioid, 'tipousuario')}>Excluir</button>
                          <button type="button" aria-label="Editar tipo de usuário" onClick={() => {
                            setEditandoTipoUsuario(tipo.usuarioid);
                            setTipoUsuarioEditado(tipo.tipousuario1);
                          }}>Editar</button>
                          <button type="button" aria-label="Excluir tipo de usuário" onClick={() => excluirItem(tipo.usuarioid, 'tipousuario')}>Excluir</button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}

        {abaAtiva === 'tipopesquisa' && (
          <>
            <div className="admin-toolbar">
              <h1>Gerenciamento de Tipo de Pesquisa</h1>
              <p>Total: {tiposPesquisa.length}</p>
              <input
                type="text"
                placeholder="Buscar por nome..."
                value={filtro}
                onChange={(e) => setFiltro(e.target.value)}
              />
            </div>

            <div className="admin-add-form">
              <input
                type="text"
                placeholder="Novo Tipo de Pesquisa"
                value={novoTipoPesquisa}
                onChange={(e) => setNovoTipoPesquisa(e.target.value)}
              />
              <button onClick={adicionarTipoPesquisa}>Adicionar</button>
            </div>

            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {tiposPesquisa.filter(tipo => tipo.tipopesquisa1?.toLowerCase().includes(filtro.toLowerCase())).map((tipo, index) => (
                  <tr key={tipo.tipopesquisaid}>
                    <td>{index + 1}</td>
                    <td>
                      {editandoTipoPesquisa === tipo.tipopesquisaid ? (
                        <input
                          value={tipoPesquisaEditado}
                          onChange={(e) => setTipoPesquisaEditado(e.target.value)}
                        />
                      ) : tipo.tipopesquisa1}
                    </td>
                    <td>
                      {editandoTipoPesquisa === tipo.tipopesquisaid ? (
                        <>
                          <button onClick={() => salvarTipoPesquisa(tipo.tipopesquisaid)}>Salvar</button>
                          <button onClick={() => setEditandoTipoPesquisa(null)}>Cancelar</button>
                          <button type="button" aria-label="Salvar tipo de pesquisa" onClick={() => salvarTipoPesquisa(tipo.tipopesquisaid)}>Salvar</button>
                          <button type="button" aria-label="Cancelar edição" onClick={() => setEditandoTipoPesquisa(null)}>Cancelar</button>
                        </>
                      ) : (
                        <>
                          <button onClick={() => {
                            setEditandoTipoPesquisa(tipo.tipopesquisaid);
                            setTipoPesquisaEditado(tipo.tipopesquisa1);
                          }}>Editar</button>
                          <button onClick={() => excluirItem(tipo.tipopesquisaid, 'tipopesquisa')}>Excluir</button>
                          <button type="button" aria-label="Editar tipo de pesquisa" onClick={() => {
                            setEditandoTipoPesquisa(tipo.tipopesquisaid);
                            setTipoPesquisaEditado(tipo.tipopesquisa1);
                          }}>Editar</button>
                          <button type="button" aria-label="Excluir tipo de pesquisa" onClick={() => excluirItem(tipo.tipopesquisaid, 'tipopesquisa')}>Excluir</button>
              <button type="button" aria-label="Adicionar tipo de usuário" onClick={adicionarTipoUsuario}>Adicionar</button>
                          <button type="button" aria-label="Salvar tipo de pesquisa" onClick={() => salvarTipoPesquisa(tipo.tipopesquisaid)}>Salvar</button>
                          <button type="button" aria-label="Cancelar edição" onClick={() => setEditandoTipoPesquisa(null)}>Cancelar</button>
                          <button type="button" aria-label="Editar tipo de pesquisa" onClick={() => {
                            setEditandoTipoPesquisa(tipo.tipopesquisaid);
                            setTipoPesquisaEditado(tipo.tipopesquisa1);
                          }}>Editar</button>
                          <button type="button" aria-label="Excluir tipo de pesquisa" onClick={() => excluirItem(tipo.tipopesquisaid, 'tipopesquisa')}>Excluir</button>
              <button type="button" aria-label="Adicionar tipo de pesquisa" onClick={adicionarTipoPesquisa}>Adicionar</button>
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </>
        )}
      </div>
    </div>
  );
};

export default AdminPage;
