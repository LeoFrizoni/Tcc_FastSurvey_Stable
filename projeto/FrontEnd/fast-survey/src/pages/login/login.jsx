import React, { useState, useRef, useEffect } from 'react';
import styles from '../login/login.module.css';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { Eye, EyeOff } from 'lucide-react';

const API_BASE =
  (typeof import.meta !== 'undefined' && import.meta.env?.VITE_API_URL) ||
  'http://localhost:5062';

const api = axios.create({ baseURL: API_BASE });

const Login = () => {
  const [isActive, setIsActive] = useState(false);

  // Login
  const [usuario, setUsuario] = useState('');
  const [senha, setSenha] = useState('');
  const [loadingLogin, setLoadingLogin] = useState(false);
  const [showPassLogin, setShowPassLogin] = useState(false);

  // Cadastro
  const [novoUsuario, setNovoUsuario] = useState('');
  const [novaSenha, setNovaSenha] = useState('');
  const [novoEmail, setNovoEmail] = useState('');
  const [loadingCadastro, setLoadingCadastro] = useState(false);
  const [showPassCadastro, setShowPassCadastro] = useState(false);

  // LGPD Modal
  const [showLgpd, setShowLgpd] = useState(false);
  const [scrolledToEnd, setScrolledToEnd] = useState(false);
  const [accepted, setAccepted] = useState(false);
  const termsRef = useRef(null);

  const navigate = useNavigate();
  const validarSenhaForte = (s) => /^(?=.*[A-Z])(?=.*\d).{8,}$/.test(s);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) api.defaults.headers.common.Authorization = `Bearer ${token}`;
  }, []);

  // ------- LOGIN -------
  const handleLogin = async (e) => {
    e.preventDefault();
    if (!usuario || !senha) {
      toast.warn('Informe usuário e senha.');
      return;
    }
    setLoadingLogin(true);
    try {
      const { data } = await api.post('/api/login/Autenticar', { usuario, senha });

      localStorage.setItem('token', data.token);
      localStorage.setItem('userId', String(data.id));
      localStorage.setItem('tipousuarioid', String(data.tipousuarioid));
      api.defaults.headers.common.Authorization = `Bearer ${data.token}`;

      toast.success(`Bem-vindo(a), ${data.usuario}!`);

      const tipo = Number(data.tipousuarioid);
      setTimeout(() => {
        if (tipo === 15) navigate('/admin');
        else navigate('/home');
      }, 800);
    } catch (err) {
      const msg =
        err?.response?.data ||
        err?.response?.data?.message ||
        'Usuário ou senha inválidos';
      toast.error(typeof msg === 'string' ? msg : 'Falha ao autenticar.');
    } finally {
      setLoadingLogin(false);
    }
  };

  // ------- CADASTRO -------
  const abrirModalLgpd = (e) => {
    e.preventDefault();
    if (!novoUsuario || !novoEmail || !novaSenha) {
      toast.error('Preencha todos os campos de cadastro.');
      return;
    }
    if (!validarSenhaForte(novaSenha)) {
      toast.error('A senha deve ter no mínimo 8 caracteres, com ao menos uma letra maiúscula e um número.');
      return;
    }
    setShowLgpd(true);
    setScrolledToEnd(false);
    setAccepted(false);
  };

  const confirmarCadastro = async () => {
    if (!accepted) return;
    setLoadingCadastro(true);

    const formData = new FormData();
    formData.append('usuario', novoUsuario);
    formData.append('senha', novaSenha);
    formData.append('email', novoEmail);
    formData.append('tipousuarioid', 13);

    try {
      await api.post('/api/login/CadastrarLogin', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      toast.success('Usuário cadastrado com sucesso!');
      setNovoUsuario('');
      setNovaSenha('');
      setNovoEmail('');
      setShowLgpd(false);
      setTimeout(() => setIsActive(false), 800);
    } catch (err) {
      const msg =
        err?.response?.data ||
        err?.response?.data?.message ||
        'Erro ao cadastrar usuário.';
      toast.error(typeof msg === 'string' ? msg : 'Erro ao cadastrar usuário.');
    } finally {
      setLoadingCadastro(false);
    }
  };

  // ------- Modal helpers -------
  const onScrollTerms = () => {
    const el = termsRef.current;
    if (!el) return;
    const atEnd = el.scrollTop + el.clientHeight >= el.scrollHeight - 8;
    if (atEnd && !scrolledToEnd) setScrolledToEnd(true);
  };

  useEffect(() => {
    const onKeyDown = (e) => {
      if (e.key === 'Escape' && showLgpd && !loadingCadastro) setShowLgpd(false);
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [showLgpd, loadingCadastro]);

  const renderRequisito = (cond, texto) => (
    <p style={{ color: cond ? 'green' : 'red', fontSize: '12px', margin: '3px 0' }}>
      {cond ? '✓' : '✗'} {texto}
    </p>
  );

  return (
    <div className={styles.viewport}>
      <div className={`${styles.container} ${isActive ? styles.active : ''}`} id="container">
        {/* Cadastro */}
        <div className={`${styles['form-container']} ${styles['sign-up']}`}>
          <form>
            <h1>Crie sua conta</h1>
            <span>Preencha seus dados para se cadastrar</span>

            <input
              type="text"
              placeholder="Nome de usuário"
              value={novoUsuario}
              onChange={(e) => setNovoUsuario(e.target.value)}
              required
              autoComplete="username"
            />
            <input
              type="email"
              placeholder="E-mail"
              value={novoEmail}
              onChange={(e) => setNovoEmail(e.target.value)}
              required
              autoComplete="email"
            />

            <div className={styles.inputRow}>
              <input
                type={showPassCadastro ? 'text' : 'password'}
                placeholder="Senha"
                value={novaSenha}
                onChange={(e) => setNovaSenha(e.target.value)}
                required
                autoComplete="new-password"
                className={styles.inputWithEye}
              />
              <button
                type="button"
                className={styles.eyeBtn}
                onClick={() => setShowPassCadastro(v => !v)}
                aria-label={showPassCadastro ? 'Ocultar senha' : 'Mostrar senha'}
                title={showPassCadastro ? 'Ocultar senha' : 'Mostrar senha'}
              >
                {showPassCadastro ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>

            {/* <<< REQUISITOS VISUAIS DA SENHA (aparece quando começa a digitar) >>> */}
            {novaSenha && (
              <div className={styles['validacao-senha']}>
                {renderRequisito(novaSenha.length >= 8, 'Mínimo de 8 caracteres')}
                {renderRequisito(/[A-Z]/.test(novaSenha), 'Pelo menos uma letra maiúscula')}
                {renderRequisito(/\d/.test(novaSenha), 'Pelo menos um número')}
              </div>
            )}

            <button type="button" onClick={abrirModalLgpd} disabled={loadingCadastro}>
              {loadingCadastro ? 'Cadastrando...' : 'Cadastrar'}
            </button>
          </form>
        </div>

        {/* Login */}
        <div className={`${styles['form-container']} ${styles['sign-in']}`}>
          <form onSubmit={handleLogin}>
            <h1>FastSurvey</h1>
            <span>Entre com seu usuário e senha</span>

            <input
              type="text"
              placeholder="Usuário"
              value={usuario}
              onChange={(e) => setUsuario(e.target.value)}
              required
              autoComplete="username"
            />

            <div className={styles.inputRow}>
              <input
                type={showPassLogin ? 'text' : 'password'}
                placeholder="Senha"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                required
                autoComplete="current-password"
                className={styles.inputWithEye}
              />
              <button
                type="button"
                className={styles.eyeBtn}
                onClick={() => setShowPassLogin(v => !v)}
                aria-label={showPassLogin ? 'Ocultar senha' : 'Mostrar senha'}
                title={showPassLogin ? 'Ocultar senha' : 'Mostrar senha'}
              >
                {showPassLogin ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>

            <button type="submit" disabled={loadingLogin}>
              {loadingLogin ? 'Entrando...' : 'Entrar'}
            </button>
          </form>
        </div>

        {/* Painéis alternáveis */}
        <div className={styles['toggle-container']}>
          <div className={styles['toggle']}>
            <div className={`${styles['toggle-panel']} ${styles['toggle-left']}`}>
              <h1>Olá, novo por aqui?</h1>
              <p>Preencha seus dados para começar</p>
              <button
                className={`${styles.toggleBtn} ${styles.btnSolid}`}
                type="button"
                aria-label="Já tenho conta"
                onClick={() => setIsActive(false)}
              >
                Já tenho conta
              </button>
            </div>

            <div className={`${styles['toggle-panel']} ${styles['toggle-right']}`}>
              <h1>Bem-vindo de volta!</h1>
              <p>Entre para acessar o sistema</p>
              <button
                className={`${styles.toggleBtn} ${styles.btnOutline}`}
                type="button"
                aria-label="Registrar-se"
                onClick={() => setIsActive(true)}
              >
                Registrar-se
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* ---- MODAL LGPD (idêntico ao anterior) ---- */}
      {showLgpd && (
        <div className={styles.modalOverlay} role="dialog" aria-modal="true" aria-labelledby="lgpd-title">
          <div className={styles.modalContent}>
            <div className={styles.modalHeader}>
              <h2 id="lgpd-title">Termos de Uso & Privacidade (LGPD)</h2>
              <button
                type="button"
                className={styles.modalClose}
                onClick={() => !loadingCadastro && setShowLgpd(false)}
                aria-label="Fechar"
                disabled={loadingCadastro}
              >
                ×
              </button>
            </div>

            <div className={styles.modalBody} ref={termsRef} onScroll={onScrollTerms} tabIndex={0}>
              {/* ...texto dos termos... */}
              <p style={{ marginTop: 16, fontStyle: 'italic' }}>Role até o final para habilitar a opção de aceite.</p>
            </div>

            <div className={styles.modalFooter}>
              <label className={styles.checkboxRow}>
                <input
                  type="checkbox"
                  disabled={!scrolledToEnd || loadingCadastro}
                  checked={accepted}
                  onChange={(e) => setAccepted(e.target.checked)}
                />
                <span>Li e aceito os Termos de Uso & Privacidade</span>
              </label>
              <div className={styles.modalActions}>
                <button type="button" className={styles.btnSecondary} onClick={() => setShowLgpd(false)} disabled={loadingCadastro}>Cancelar</button>
                <button type="button" className={styles.btnPrimary} onClick={confirmarCadastro} disabled={!accepted || loadingCadastro}>
                  {loadingCadastro ? 'Registrando…' : 'Confirmar cadastro'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <ToastContainer position="top-right" autoClose={3000} />
    </div>
  );
};

export default Login;
