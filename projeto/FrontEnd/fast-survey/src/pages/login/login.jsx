import React, { useState, useRef, useEffect } from 'react';
import styles from '../login/login.module.css';
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

  // LGPD Modal
  const [showLgpd, setShowLgpd] = useState(false);
  const [scrolledToEnd, setScrolledToEnd] = useState(false);
  const [accepted, setAccepted] = useState(false);
  const termsRef = useRef(null);

  const navigate = useNavigate();

  const validarSenhaForte = (senha) => /^(?=.*[A-Z])(?=.*\d).{8,}$/.test(senha);

  // ------- LOGIN -------
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
        if (resposta.data.tipousuarioid === 15) navigate('/admin');
        else navigate('/home');
      }, 1000);
    } catch {
      toast.error('Usuário ou senha inválidos');
    } finally {
      setLoadingLogin(false);
    }
  };

  // ------- CADASTRO: abre modal LGPD em vez de enviar direto -------
  const abrirModalLgpd = (e) => {
    e.preventDefault();
    if (!validarSenhaForte(novaSenha)) {
      toast.error('A senha deve ter no mínimo 8 caracteres, com ao menos uma letra maiúscula e um número.');
      return;
    }
    if (!novoUsuario || !novoEmail || !novaSenha) {
      toast.error('Preencha todos os campos de cadastro.');
      return;
    }
    setShowLgpd(true);
    setScrolledToEnd(false);
    setAccepted(false);
  };

  // ------- CADASTRO: confirmação após aceitar LGPD -------
  const confirmarCadastro = async () => {
    if (!accepted) return;
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
      setShowLgpd(false);
      setTimeout(() => setIsActive(false), 1000);
    } catch (erro) {
      toast.error('Erro ao cadastrar usuário: ' + (erro.response?.data || erro.message));
    } finally {
      setLoadingCadastro(false);
    }
  };

  // Detecta scroll até o final do texto
  const onScrollTerms = () => {
    const el = termsRef.current;
    if (!el) return;
    const atEnd = el.scrollTop + el.clientHeight >= el.scrollHeight - 8;
    if (atEnd && !scrolledToEnd) setScrolledToEnd(true);
  };

  // A11y: fecha com ESC
  useEffect(() => {
    const onKeyDown = (e) => {
      if (e.key === 'Escape' && showLgpd && !loadingCadastro) setShowLgpd(false);
    };
    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [showLgpd, loadingCadastro]);

  const renderRequisito = (condicao, texto) => (
    <p style={{ color: condicao ? 'green' : 'red', fontSize: '12px', margin: '3px 0' }}>
      {condicao ? '✓' : '✗'} {texto}
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
              <div className={styles['validacao-senha']}>
                {renderRequisito(novaSenha.length >= 8, 'Mínimo de 8 caracteres')}
                {renderRequisito(/[A-Z]/.test(novaSenha), 'Pelo menos uma letra maiúscula')}
                {renderRequisito(/\d/.test(novaSenha), 'Pelo menos um número')}
              </div>
            )}

            {/* IMPORTANTE: vira button normal e abre o modal LGPD */}
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

      {/* MODAL LGPD */}
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

            <div
              className={styles.modalBody}
              ref={termsRef}
              onScroll={onScrollTerms}
              tabIndex={0}
            >
              <h3>Termos de Uso & Privacidade — FastSurvey</h3>
              <p><strong>Última atualização:</strong> 17/08/2025</p>

              <h4>1. Quem somos e escopo</h4>
              <p>
                O FastSurvey é uma plataforma para criação, distribuição e análise de pesquisas.
                Este Termo rege o uso do serviço e o tratamento de dados pessoais,
                conforme a Lei nº 13.709/2018 (LGPD) e o Marco Civil da Internet.
              </p>

              <h4>2. Dados que tratamos</h4>
              <ul>
                <li>Conta: nome de usuário, e-mail e senha.</li>
                <li>Uso do serviço: logs de acesso, IPs, data/hora e ações realizadas.</li>
                <li>Conteúdo: pesquisas, perguntas, anexos e respostas submetidas.</li>
                <li>Cookies: para autenticação, segurança e melhoria da experiência.</li>
              </ul>

              <h4>3. Bases legais</h4>
              <p>
                Tratamos dados para execução do contrato, cumprimento de obrigação legal
                e legítimo interesse, sempre observando seus direitos. Quando necessário,
                solicitaremos consentimento.
              </p>

              <h4>4. Finalidades</h4>
              <p>
                Operar e manter a plataforma, autenticar usuários, armazenar pesquisas e respostas,
                gerar QR codes, prevenir fraudes, cumprir obrigações legais e melhorar recursos.
              </p>

              <h4>5. Compartilhamento</h4>
              <p>
                Pode ocorrer com provedores de infraestrutura, analytics ou autoridades,
                quando necessário e proporcional.
              </p>

              <h4>6. Segurança e retenção</h4>
              <p>
                Adotamos medidas de segurança adequadas e retemos dados pelo tempo necessário
                às finalidades ou exigências legais.
              </p>

              <h4>7. Direitos do titular</h4>
              <p>
                Você pode solicitar confirmação, acesso, correção, portabilidade, eliminação
                e outras prerrogativas previstas na LGPD. Contato: <strong>dpo@fastsurvey.local</strong>.
              </p>

              <h4>8. Crianças e adolescentes</h4>
              <p>
                O FastSurvey não é direcionado a menores. Dados identificados sem base legal
                serão removidos.
              </p>

              <h4>9. Transferências internacionais</h4>
              <p>
                Podem ocorrer quando usamos provedores fora do Brasil, sempre com salvaguardas adequadas.
              </p>

              <h4>10. Atualizações e contato</h4>
              <p>
                Os termos podem ser atualizados. Notificaremos mudanças relevantes na plataforma.
                Dúvidas: <strong>dpo@fastsurvey.local</strong>.
              </p>

              <h4>11. Foro</h4>
              <p>
                Aplica-se a legislação brasileira. Foro: domicílio do titular ou local do réu,
                conforme a lei aplicável.
              </p>

              <p style={{ marginTop: 16, fontStyle: 'italic' }}>
                Role até o final para habilitar a opção de aceite.
              </p>
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
                <button
                  type="button"
                  className={styles.btnSecondary}
                  onClick={() => setShowLgpd(false)}
                  disabled={loadingCadastro}
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  className={styles.btnPrimary}
                  onClick={confirmarCadastro}
                  disabled={!accepted || loadingCadastro}
                >
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
