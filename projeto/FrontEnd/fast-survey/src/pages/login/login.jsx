import React, { useState } from 'react';
import './login.css';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const Login = () => {
  const [isActive, setIsActive] = useState(false);

  // Login
  const [usuario, setUsuario] = useState('');
  const [senha, setSenha] = useState('');
  const [loadingLogin, setLoadingLogin] = useState(false);

  // Cadastro
  const [novoUsuario, setNovoUsuario] = useState('');
  const [novaSenha, setNovaSenha] = useState('');
  const [novoEmail, setNovoEmail] = useState('');
  const [loadingCadastro, setLoadingCadastro] = useState(false);

  const navigate = useNavigate();

  const validarSenhaForte = (senha) => {
    return /^(?=.*[A-Z])(?=.*\d).{8,}$/.test(senha);
  };

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoadingLogin(true);
    try {
      const resposta = await axios.post('http://localhost:5062/api/login/Autenticar', {
        usuario,
        senha
      });
      localStorage.setItem('token', resposta.data.token);
      localStorage.setItem('userId', resposta.data.id);
      localStorage.setItem('tipousuarioid', resposta.data.tipousuarioid);
      toast.success(`Bem-vindo(a), ${resposta.data.usuario}!`);

      setTimeout(() => {
        if (resposta.data.tipousuarioid === 15) {
          navigate('/admin');
        } else {
          navigate('/home');
        }
      }, 1000);
    } catch {
      toast.error('Usuário ou senha inválidos');
    } finally {
      setLoadingLogin(false);
    }
  };

  const handleCadastro = async (e) => {
    e.preventDefault();
    if (!validarSenhaForte(novaSenha)) {
      toast.error('A senha deve ter no mínimo 8 caracteres, com ao menos uma letra maiúscula e um número.');
      return;
    }

    setLoadingCadastro(true);
    const formData = new FormData();
    formData.append('usuario', novoUsuario);
    formData.append('senha', novaSenha);
    formData.append('email', novoEmail);

    try {
      const resposta = await axios.post('http://localhost:5062/api/login/cadastrarlogin', formData);
      toast.success(`Usuário ${resposta.data.usuario} cadastrado com sucesso!`);
      setNovoUsuario('');
      setNovaSenha('');
      setNovoEmail('');
      setTimeout(() => {
        setIsActive(false);
      }, 3000);
    } catch (erro) {
      toast.error('Erro ao cadastrar usuário: ' + (erro.response?.data || erro.message));
    } finally {
      setLoadingCadastro(false);
    }
  };

  const renderRequisito = (condicao, texto) => (
    <p style={{ color: condicao ? 'green' : 'red', fontSize: '12px', margin: '3px 0' }}>
      {condicao ? '✓' : '✗'} {texto}
    </p>
  );

  return (
    <div className={`container ${isActive ? 'active' : ''}`} id="container">
      {/* Cadastro */}
      <div className="form-container sign-up">
        <form onSubmit={handleCadastro}>
          <h1>Crie sua conta</h1>
          <span>Preencha seus dados para se cadastrar</span>

          <input
            type="text"
            placeholder="Nome de usuário"
            value={novoUsuario}
            onChange={(e) => setNovoUsuario(e.target.value)}
            required
          />
          <input
            type="email"
            placeholder="E-mail"
            value={novoEmail}
            onChange={(e) => setNovoEmail(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Senha"
            value={novaSenha}
            onChange={(e) => setNovaSenha(e.target.value)}
            required
          />

          {novaSenha && (
            <div className="validacao-senha">
              {renderRequisito(novaSenha.length >= 8, 'Mínimo de 8 caracteres')}
              {renderRequisito(/[A-Z]/.test(novaSenha), 'Pelo menos uma letra maiúscula')}
              {renderRequisito(/\d/.test(novaSenha), 'Pelo menos um número')}
            </div>
          )}

          <button type="submit" disabled={loadingCadastro}>
            {loadingCadastro ? 'Cadastrando...' : 'Cadastrar'}
          </button>
        </form>
      </div>

      {/* Login */}
      <div className="form-container sign-in">
        <form onSubmit={handleLogin}>
          <h1>FastSurvey</h1>
          <span>Entre com seu usuário e senha</span>

          <input
            type="text"
            placeholder="Usuário"
            value={usuario}
            onChange={(e) => setUsuario(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Senha"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            required
          />

          <button type="submit" disabled={loadingLogin}>
            {loadingLogin ? 'Entrando...' : 'Entrar'}
          </button>
        </form>
      </div>

      {/* Painéis alternáveis */}
      <div className="toggle-container">
        <div className="toggle">
          <div className="toggle-panel toggle-left">
            <h1>Olá, novo por aqui?</h1>
            <p>Preencha seus dados para começar</p>
            <button className="hidden" type="button" aria-label="Já tenho conta" onClick={() => setIsActive(false)}>Já tenho conta</button>
          </div>
          <div className="toggle-panel toggle-right">
            <h1>Bem-vindo de volta!</h1>
            <p>Entre para acessar o sistema</p>
            <button className="hidden" type="button" aria-label="Registrar-se" onClick={() => setIsActive(true)}>Registrar-se</button>
          </div>
        </div>
      </div>

      {/* Toast container */}
      <ToastContainer position="top-right" autoClose={3000} />
    </div>
  );
};

export default Login;
