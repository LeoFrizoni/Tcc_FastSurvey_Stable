import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Eye, EyeOff, CheckCircle, XCircle } from 'lucide-react';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import api from '../../lib/api.js';
import styles from './resetPassword.module.css';

const ResetPassword = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');
  
  // Estados do formulário
  const [novaSenha, setNovaSenha] = useState('');
  const [confirmarSenha, setConfirmarSenha] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [tokenValido, setTokenValido] = useState(false);
  const [verificandoToken, setVerificandoToken] = useState(true);
  const [senhaAlterada, setSenhaAlterada] = useState(false);

  // Validação de senha forte
  const validarSenhaForte = (senha) => {
    const requisitos = {
      length: senha.length >= 8,
      uppercase: /[A-Z]/.test(senha),
      lowercase: /[a-z]/.test(senha),
      number: /\d/.test(senha),
      special: /[^A-Za-z0-9]/.test(senha) // Qualquer caractere que não seja letra ou número
    };
    return requisitos;
  };

  const requisitos = validarSenhaForte(novaSenha);
  const senhaValida = Object.values(requisitos).every(req => req);

  // Verificar se o token é válido ao carregar a página
  useEffect(() => {
    const verificarToken = async () => {
      if (!token) {
        setTokenValido(false);
        setVerificandoToken(false);
        return;
      }

      try {
        // Aqui você pode fazer uma chamada para verificar se o token é válido
        // Por enquanto, vamos assumir que é válido se existe
        setTokenValido(true);
      } catch (error) {
        setTokenValido(false);
        toast.error('Token inválido ou expirado');
      } finally {
        setVerificandoToken(false);
      }
    };

    verificarToken();
  }, [token]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!token) {
      toast.error('Token não encontrado');
      return;
    }

    if (!senhaValida) {
      toast.error('A senha não atende aos requisitos mínimos');
      return;
    }

    if (novaSenha !== confirmarSenha) {
      toast.error('As senhas não coincidem');
      return;
    }

    setLoading(true);
    try {
      await api.post('/api/login/ResetarSenha', {
        token: token,
        novaSenha: novaSenha
      });
      
      setSenhaAlterada(true);
      toast.success('Senha alterada com sucesso!');
    } catch (error) {
      const msg = error.response?.data?.message || 'Erro ao alterar senha';
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  };

  if (verificandoToken) {
    return (
      <div className={styles.container}>
        <div className={styles.loadingCard}>
          <div className={styles.spinner}></div>
          <p>Verificando token...</p>
        </div>
      </div>
    );
  }

  if (!tokenValido || !token) {
    return (
      <div className={styles.container}>
        <div className={styles.errorCard}>
          <XCircle size={48} color="#ef4444" />
          <h2>Token Inválido</h2>
          <p>O link de reset de senha é inválido ou expirou.</p>
          <button 
            className={styles.btnPrimary}
            onClick={() => navigate('/login')}
          >
            Voltar para o Login
          </button>
        </div>
      </div>
    );
  }

  if (senhaAlterada) {
    return (
      <div className={styles.container}>
        <div className={styles.successCard}>
          <CheckCircle size={48} color="#10b981" />
          <h2>Senha Alterada!</h2>
          <p>Sua senha foi alterada com sucesso. Agora você pode fazer login com sua nova senha.</p>
          <button 
            className={styles.btnPrimary}
            onClick={() => navigate('/login')}
          >
            Ir para o Login
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <div className={styles.header}>
          <CheckCircle size={32} color="#10b981" />
          <h1>Redefinir Senha</h1>
          <p>Digite sua nova senha abaixo</p>
        </div>

        <form onSubmit={handleSubmit} className={styles.form}>
          <div className={styles.inputGroup}>
            <label htmlFor="novaSenha">Nova Senha</label>
            <div className={styles.passwordInput}>
                             <input
                 type={showPassword ? 'text' : 'password'}
                 id="novaSenha"
                 value={novaSenha}
                 onChange={(e) => setNovaSenha(e.target.value)}
                 placeholder="Digite sua nova senha"
                 required
                 className={styles.input}
                 style={{ paddingRight: '40px' }}
               />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className={styles.eyeButton}
                aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
              >
                {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>
          </div>

          <div className={styles.inputGroup}>
            <label htmlFor="confirmarSenha">Confirmar Senha</label>
            <div className={styles.passwordInput}>
                             <input
                 type={showConfirmPassword ? 'text' : 'password'}
                 id="confirmarSenha"
                 value={confirmarSenha}
                 onChange={(e) => setConfirmarSenha(e.target.value)}
                 placeholder="Confirme sua nova senha"
                 required
                 className={styles.input}
                 style={{ paddingRight: '40px' }}
               />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className={styles.eyeButton}
                aria-label={showConfirmPassword ? 'Ocultar senha' : 'Mostrar senha'}
              >
                {showConfirmPassword ? <EyeOff size={20} /> : <Eye size={20} />}
              </button>
            </div>
          </div>

          {/* Validação de senha */}
          <div className={styles.passwordValidation}>
            <h4>Requisitos da senha:</h4>
            <div className={styles.requisitos}>
              <div className={`${styles.requisito} ${requisitos.length ? styles.valid : styles.invalid}`}>
                {requisitos.length ? <CheckCircle size={16} /> : <XCircle size={16} />}
                <span>Mínimo 8 caracteres</span>
              </div>
              <div className={`${styles.requisito} ${requisitos.uppercase ? styles.valid : styles.invalid}`}>
                {requisitos.uppercase ? <CheckCircle size={16} /> : <XCircle size={16} />}
                <span>Pelo menos uma letra maiúscula</span>
              </div>
              <div className={`${styles.requisito} ${requisitos.lowercase ? styles.valid : styles.invalid}`}>
                {requisitos.lowercase ? <CheckCircle size={16} /> : <XCircle size={16} />}
                <span>Pelo menos uma letra minúscula</span>
              </div>
              <div className={`${styles.requisito} ${requisitos.number ? styles.valid : styles.invalid}`}>
                {requisitos.number ? <CheckCircle size={16} /> : <XCircle size={16} />}
                <span>Pelo menos um número</span>
              </div>
              <div className={`${styles.requisito} ${requisitos.special ? styles.valid : styles.invalid}`}>
                {requisitos.special ? <CheckCircle size={16} /> : <XCircle size={16} />}
                <span>Pelo menos um caractere especial</span>
              </div>
            </div>
          </div>

          <div className={styles.buttonGroup}>
            <button
              type="button"
              onClick={() => navigate('/login')}
              className={styles.btnSecondary}
              disabled={loading}
            >
              Cancelar
            </button>
            <button
              type="submit"
              className={styles.btnPrimary}
              disabled={loading || !senhaValida || novaSenha !== confirmarSenha}
            >
              {loading ? 'Alterando...' : 'Alterar Senha'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ResetPassword;
