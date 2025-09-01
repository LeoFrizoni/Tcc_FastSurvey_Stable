import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Users, Presentation, ArrowRight, Sparkles, Home } from 'lucide-react';
import api from '../../lib/api';
import styles from './InteractiveJoin.module.css';

const InteractiveJoin = () => {
  const { accessCode } = useParams();
  const navigate = useNavigate();
  
  const [session, setSession] = useState(null);
  const [participantName, setParticipantName] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isJoining, setIsJoining] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    loadSession();
  }, [accessCode, loadSession]);

  const loadSession = async () => {
    try {
      const response = await api.get(`/api/PesquisaInterativa/sessao/${accessCode}`);
      if (response.data.success) {
        setSession(response.data.data);
      } else {
        setError('Sessão não encontrada');
      }
    } catch (err) {
      console.error('Erro ao carregar sessão:', err);
      setError('Sessão não encontrada ou inacessível');
    } finally {
      setIsLoading(false);
    }
  };

  const handleJoinSession = async (e) => {
    e.preventDefault();
    
    if (!participantName.trim()) {
      setError('Por favor, informe seu nome');
      return;
    }

    setIsJoining(true);
    setError('');

    try {
      const response = await api.post('/api/pesquisa-interativa/entrar', {
        codigoAcesso: accessCode,
        nomeParticipante: participantName.trim()
      });

      if (response.data.success) {
        const participantData = response.data.data;
        localStorage.setItem('interactiveParticipantData', JSON.stringify(participantData));
        navigate(`/interactive/player/${session.sessaoId}`);
      } else {
        setError(response.data.message || 'Erro ao entrar na sessão');
      }
    } catch (err) {
      console.error('Erro ao entrar na sessão:', err);
      setError(err.response?.data?.message || 'Erro ao entrar na sessão');
    } finally {
      setIsJoining(false);
    }
  };

  if (isLoading) {
    return (
      <div className={styles.loading}>
        <div className={styles.spinner}></div>
        <p>Carregando sessão...</p>
      </div>
    );
  }

  if (error && !session) {
    return (
      <div className={styles.error}>
        <p>{error}</p>
        <button onClick={() => navigate('/home')} className={styles.homeButton}>
          <Home size={16} />
          Voltar ao Início
        </button>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.content}>
        <div className={styles.header}>
          <div className={styles.iconContainer}>
            <Presentation size={48} />
          </div>
          <h1>Sessão Interativa</h1>
          <p>Entre na sessão para participar</p>
        </div>

        {session && (
          <div className={styles.sessionInfo}>
            <h2>{session.tituloPesquisa}</h2>
            <div className={styles.sessionDetails}>
              <div className={styles.detail}>
                <Users size={20} />
                <span>Código: {session.codigoAcesso}</span>
              </div>
              <div className={styles.detail}>
                <Presentation size={20} />
                <span>Sessão controlada pelo apresentador</span>
              </div>
            </div>
          </div>
        )}

        <div className={styles.features}>
          <div className={styles.feature}>
            <Sparkles size={20} />
            <span>Sincronização em tempo real</span>
          </div>
          <div className={styles.feature}>
            <Users size={20} />
            <span>Veja a mesma pergunta que todos</span>
          </div>
          <div className={styles.feature}>
            <Presentation size={20} />
            <span>Controle total do apresentador</span>
          </div>
        </div>

        <form onSubmit={handleJoinSession} className={styles.joinForm}>
          <div className={styles.inputGroup}>
            <label htmlFor="participantName">Seu nome</label>
            <input
              type="text"
              id="participantName"
              value={participantName}
              onChange={(e) => setParticipantName(e.target.value)}
              placeholder="Digite seu nome"
              maxLength={100}
              required
              className={styles.input}
            />
          </div>

          {error && <div className={styles.error}>{error}</div>}

          <button
            type="submit"
            disabled={isJoining || !participantName.trim()}
            className={styles.joinButton}
          >
            {isJoining ? 'Entrando...' : (
              <>
                <ArrowRight size={20} />
                Entrar na Sessão
              </>
            )}
          </button>
        </form>
      </div>
    </div>
  );
};

export default InteractiveJoin;
