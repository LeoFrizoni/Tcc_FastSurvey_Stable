import React, { useState, useRef, useEffect } from "react";
import styles from "./login.module.css";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { Eye, EyeOff } from "lucide-react";
import GoogleLoginButton from "../../components/GoogleLoginButton";

/* ===================== Config API (CRA) ===================== */
const API_BASE =
  process.env.REACT_APP_API_URL ||
  (typeof window !== "undefined" && window.API_BASE_URL) ||
  "http://localhost:5062";

const api = axios.create({ baseURL: API_BASE });

const ENDPOINTS = {
  login: "/api/login/Autenticar",
  register: "/api/login/CadastrarLogin",
  googleAuthPreferred: "/api/login/GoogleAuth",
  googleAuthFallback: "/api/Login/GoogleAuth",
};

const RESET_PASSWORD_ENDPOINTS = [
  "/api/login/SolicitarRedefinicaoSenha",
  "/api/Login/RecuperarSenha",
  "/api/login/forgot",
];

const RESEND_VERIFICATION_ENDPOINTS = [
  "/api/login/ReenviarConfirmacao",
  "/api/Login/ReenviarConfirmacao",
  "/api/auth/resend-confirmation",
];

/* Google Client ID (CRA) */
const GOOGLE_CLIENT_ID =
  process.env.REACT_APP_GOOGLE_CLIENT_ID ||
  (typeof window !== "undefined" && window.GOOGLE_CLIENT_ID) ||
  "";

export default function Login() {
  const [isActive, setIsActive] = useState(false);

  // Login
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [loadingLogin, setLoadingLogin] = useState(false);
  const [showPassLogin, setShowPassLogin] = useState(false);

  // Cadastro
  const [novoUsuario, setNovoUsuario] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [novoEmail, setNovoEmail] = useState("");
  const [loadingCadastro, setLoadingCadastro] = useState(false);
  const [showPassCadastro, setShowPassCadastro] = useState(false);

  // LGPD
  const [showLgpd, setShowLgpd] = useState(false);
  const [scrolledToEnd, setScrolledToEnd] = useState(false);
  const [accepted, setAccepted] = useState(false);
  const termsRef = useRef(null);

  // Esqueci a senha
  const [showForgot, setShowForgot] = useState(false);
  const [forgotEmail, setForgotEmail] = useState("");
  const [loadingForgot, setLoadingForgot] = useState(false);

  const navigate = useNavigate();
  const validarSenhaForte = (s) => /^(?=.*[A-Z])(?=.*\d).{8,}$/.test(s);

  /* axios: injeta token se existir */
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) api.defaults.headers.common.Authorization = `Bearer ${token}`;
    const reqI = api.interceptors.request.use((cfg) => {
      const t = localStorage.getItem("token");
      if (t) cfg.headers.Authorization = `Bearer ${t}`;
      return cfg;
    });
    return () => api.interceptors.request.eject(reqI);
  }, []);



  async function handleGoogleCredentialResponse(response) {
    const id_token = response?.credential;
    if (!id_token) return toast.error("Resposta do Google inválida.");
    try {
      let data;
      try {
        ({ data } = await api.post(ENDPOINTS.googleAuthPreferred, { id_token }));
      } catch {
        ({ data } = await api.post(ENDPOINTS.googleAuthFallback, { id_token }));
      }
      if (!data?.token) return toast.error("Resposta do servidor sem token.");

      localStorage.setItem("token", data.token);
      if (data.id != null) localStorage.setItem("userId", String(data.id));
      if (data.tipousuarioid != null) localStorage.setItem("tipousuarioid", String(data.tipousuarioid));
      api.defaults.headers.common.Authorization = `Bearer ${data.token}`;

      toast.success(`Olá, ${data.usuario || "usuário Google"}!`);
      const tipo = Number(data.tipousuarioid);
      setTimeout(() => (tipo === 15 ? navigate("/admin") : navigate("/home")), 600);
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Falha na autenticação com Google.";
      toast.error(msg);
    }
  }

  // Login
  const handleLogin = async (e) => {
    e.preventDefault();
    if (!usuario || !senha) return toast.warn("Informe usuário e senha.");
    setLoadingLogin(true);
    try {
      const { data } = await api.post(ENDPOINTS.login, { usuario, senha });
      localStorage.setItem("token", data.token);
      if (data.id != null) localStorage.setItem("userId", String(data.id));
      if (data.tipousuarioid != null) localStorage.setItem("tipousuarioid", String(data.tipousuarioid));
      api.defaults.headers.common.Authorization = `Bearer ${data.token}`;
      toast.success(`Bem-vindo(a), ${data.usuario || usuario}!`);
      const tipo = Number(data.tipousuarioid);
      setTimeout(() => (tipo === 15 ? navigate("/admin") : navigate("/home")), 700);
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Usuário ou senha inválidos.";
      toast.error(msg);
    } finally {
      setLoadingLogin(false);
    }
  };

  // Cadastro
  const abrirModalLgpd = (e) => {
    e.preventDefault();
    if (!novoUsuario || !novoEmail || !novaSenha)
      return toast.error("Preencha todos os campos de cadastro.");
    if (!validarSenhaForte(novaSenha))
      return toast.error("A senha deve ter no mínimo 8 caracteres, com ao menos uma letra maiúscula e um número.");
    setShowLgpd(true);
    setScrolledToEnd(false);
    setAccepted(false);
  };

  const confirmarCadastro = async () => {
    if (!accepted) return;
    setLoadingCadastro(true);
    const formData = new FormData();
    formData.append("usuario", novoUsuario);
    formData.append("senha", novaSenha);
    formData.append("email", novoEmail);
    formData.append("tipousuarioid", 13);
    try {
      await api.post(ENDPOINTS.register, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      toast.success("Usuário cadastrado com sucesso!");
      setNovoUsuario(""); setNovaSenha(""); setNovoEmail("");
      setShowLgpd(false);
      setTimeout(() => setIsActive(false), 600);
    } catch (err) {
      const msg =
        err?.response?.data?.message ||
        (typeof err?.response?.data === "string" ? err.response.data : null) ||
        "Erro ao cadastrar usuário.";
      toast.error(msg);
    } finally {
      setLoadingCadastro(false);
    }
  };

  // Esqueci a senha
  const handleForgot = async () => {
    if (!forgotEmail || !/^\S+@\S+\.\S+$/.test(forgotEmail))
      return toast.warn("Informe um e-mail válido.");
    setLoadingForgot(true);
    try {
      let ok = false, lastErr = null;
      for (const ep of RESET_PASSWORD_ENDPOINTS) {
        try { await api.post(ep, { email: forgotEmail }); ok = true; break; }
        catch (e) { lastErr = e; }
      }
      if (ok) { toast.success("Se o e-mail existir, enviaremos instruções para redefinir a senha."); setShowForgot(false); setForgotEmail(""); }
      else {
        const msg =
          lastErr?.response?.data?.message ||
          (typeof lastErr?.response?.data === "string" ? lastErr.response.data : null) ||
          "Não foi possível solicitar a redefinição de senha.";
        toast.error(msg);
      }
    } finally { setLoadingForgot(false); }
  };

  // Reenviar verificação (usa o e‑mail do CADASTRO)
  const handleResendVerification = async () => {
    if (!novoEmail || !/^\S+@\S+\.\S+$/.test(novoEmail))
      return toast.warn("Informe um e-mail válido no campo de e‑mail do cadastro.");
    try {
      let ok = false, lastErr = null;
      for (const ep of RESEND_VERIFICATION_ENDPOINTS) {
        try { await api.post(ep, { email: novoEmail }); ok = true; break; }
        catch (e) { lastErr = e; }
      }
      if (ok) toast.success("Se o e-mail existir, reenviamos o link de verificação.");
      else {
        const msg =
          lastErr?.response?.data?.message ||
          (typeof lastErr?.response?.data === "string" ? lastErr.response.data : null) ||
          "Não foi possível reenviar a verificação.";
        toast.error(msg);
      }
    } catch {
      toast.error("Falha ao reenviar verificação.");
    }
  };

  // Modal LGPD: habilita checkbox ao rolar até o fim
  const onScrollTerms = () => {
    const el = termsRef.current;
    if (!el) return;
    const atEnd = el.scrollTop + el.clientHeight >= el.scrollHeight - 8;
    if (atEnd && !scrolledToEnd) setScrolledToEnd(true);
  };

  useEffect(() => {
    const onKeyDown = (e) => {
      if (e.key === "Escape" && showLgpd && !loadingCadastro) setShowLgpd(false);
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [showLgpd, loadingCadastro]);

  const renderRequisito = (cond, texto) => (
    <p style={{ color: cond ? "green" : "red", fontSize: "12px", margin: "3px 0" }}>
      {cond ? "✓" : "✗"} {texto}
    </p>
  );

  return (
    <div className={styles.viewport}>
      <div className={`${styles.container} ${isActive ? styles.active : ""}`} id="container">

        {/* ====== CADASTRO ====== */}
        <div className={`${styles["form-container"]} ${styles["sign-up"]}`}>
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
                type={showPassCadastro ? "text" : "password"}
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
                onClick={() => setShowPassCadastro((v) => !v)}
                aria-label={showPassCadastro ? "Ocultar senha" : "Mostrar senha"}
                title={showPassCadastro ? "Ocultar senha" : "Mostrar senha"}
              >
                {showPassCadastro ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>

            {novaSenha && (
              <div className={styles["validacao-senha"]}>
                {renderRequisito(novaSenha.length >= 8, "Mínimo de 8 caracteres")}
                {renderRequisito(/[A-Z]/.test(novaSenha), "Pelo menos uma letra maiúscula")}
                {renderRequisito(/\d/.test(novaSenha), "Pelo menos um número")}
              </div>
            )}

            {/* Ações lado a lado */}
            <div className={styles.actionsRow}>
              <button type="button" onClick={abrirModalLgpd} disabled={loadingCadastro}>
                {loadingCadastro ? "Cadastrando..." : "Cadastrar"}
              </button>

              <button
                type="button"
                className={styles.btnLink}
                onClick={handleResendVerification}
                title="Reenviar e‑mail de verificação para o e‑mail digitado"
              >
                Reenviar verificação
              </button>
            </div>
          </form>
        </div>

        {/* ====== LOGIN ====== */}
        <div className={`${styles["form-container"]} ${styles["sign-in"]}`}>
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
                type={showPassLogin ? "text" : "password"}
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
                onClick={() => setShowPassLogin((v) => !v)}
                aria-label={showPassLogin ? "Ocultar senha" : "Mostrar senha"}
                title={showPassLogin ? "Ocultar senha" : "Mostrar senha"}
              >
                {showPassLogin ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>

            {/* separador */}
            <div className={styles.sep}>
              <div className={styles.sepLine} />
              <span>ou</span>
              <div className={styles.sepLine} />
            </div>

            {/* Google no login */}
            <div className={styles.googleWrap}>
              <GoogleLoginButton
                onSuccess={handleGoogleCredentialResponse}
                onError={() => toast.error("Falha ao autenticar com Google.")}
              />
            </div>

            {/* Esqueci minha senha */}
            <div className={styles.forgotRow}>
              {!showForgot ? (
                <button type="button" className={styles.linkBtn} onClick={() => setShowForgot(true)}>
                  Esqueci minha senha
                </button>
              ) : (
                <div className={styles.forgotInline}>
                  <input
                    type="email"
                    placeholder="Seu e-mail"
                    value={forgotEmail}
                    onChange={(e) => setForgotEmail(e.target.value)}
                  />
                  <button
                    type="button"
                    className={styles.btnMini}
                    onClick={handleForgot}
                    disabled={loadingForgot}
                    title="Enviar link de redefinição"
                  >
                    {loadingForgot ? "Enviando..." : "Enviar link"}
                  </button>
                  <button
                    type="button"
                    className={styles.btnMiniGhost}
                    onClick={() => { setShowForgot(false); setForgotEmail(""); }}
                  >
                    Cancelar
                  </button>
                </div>
              )}
            </div>

            <button type="submit" disabled={loadingLogin}>
              {loadingLogin ? "Entrando..." : "Entrar"}
            </button>
          </form>
        </div>

        {/* Painéis alternáveis */}
        <div className={styles["toggle-container"]}>
          <div className={styles.toggle}>
            <div className={`${styles["toggle-panel"]} ${styles["toggle-left"]}`}>
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
            <div className={`${styles["toggle-panel"]} ${styles["toggle-right"]}`}>
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

      {/* ---- MODAL LGPD ---- */}
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
              <p style={{ marginTop: 16, fontStyle: "italic" }}>
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
                <button type="button" className={styles.btnSecondary} onClick={() => setShowLgpd(false)} disabled={loadingCadastro}>
                  Cancelar
                </button>
                <button type="button" className={styles.btnPrimary} onClick={confirmarCadastro} disabled={!accepted || loadingCadastro}>
                  {loadingCadastro ? "Registrando…" : "Confirmar cadastro"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <ToastContainer position="top-right" autoClose={3000} />
    </div>
  );
}