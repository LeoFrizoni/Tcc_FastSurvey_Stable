import { Play, Users, Clock, X, Presentation } from 'lucide-react';
import { useState } from 'react';
import api from '../../lib/api';
import styles from './InteractiveSessionModal.module.css';

const InteractiveSessionModal = ({ isOpen, onClose, pesquisaId, pesquisaTitulo }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [sessionData, setSessionData] = useState(null);
  const [error, setError] = useState('');

  const handleCreateSession = async () => {
    setIsLoading(true);
    setError('');

    try {
      const response = await api.post('/api/pesquisa-interativa/iniciar', {
        pesquisaId: parseInt(pesquisaId),
        isAtiva: false
      });

      if (response.data.success) {
        setSessionData(response.data.data);
      } else {
        setError(response.data.message || 'Erro ao criar sessão');
      }
    } catch (err) {
      console.error('Erro ao criar sessão:', err);
      setError(err.response?.data?.message || 'Erro ao criar sessão interativa');
    } finally {
      setIsLoading(false);
    }
  };

  const handleOpenHost = () => {
    if (sessionData) {
      window.open(`/interactive/host/${sessionData.sessaoId}`, '_blank');
    }
  };

  const handleCopyJoinUrl = () => {
    if (sessionData) {
      const joinUrl = `${window.location.origin}/interactive/join/${sessionData.codigoAcesso}`;
      navigator.clipboard.writeText(joinUrl);
    }
  };

  const handleClose = () => {
    setSessionData(null);
    setError('');
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className={styles.modalOverlay}>
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <div className={styles.modalTitle}>
            <Presentation size={24} />
            <h2>Sessão Interativa</h2>
          </div>
          <button onClick={handleClose} className={styles.closeButton}>
            <X size={20} />
          </button>
        </div>

        <div className={styles.modalContent}>
          {!sessionData ? (
            <div className={styles.createSession}>
              <div className={styles.sessionInfo}>
                <h3>{pesquisaTitulo}</h3>
                <p>Crie uma sessão interativa onde você controla o avanço das perguntas</p>
              </div>

              <div className={styles.features}>
                <div className={styles.feature}>
                  <Presentation size={20} />
                  <span>Controle total do avanço</span>
                </div>
                <div className={styles.feature}>
                  <Users size={20} />
                  <span>Participantes sincronizados</span>
                </div>
                <div className={styles.feature}>
                  <Clock size={20} />
                  <span>Tempo real</span>
                </div>
              </div>

              {error && <div className={styles.error}>{error}</div>}

              <button
                onClick={handleCreateSession}
                disabled={isLoading}
                className={styles.createButton}
              >
                {isLoading ? 'Criando...' : 'Criar Sessão'}
              </button>
            </div>
          ) : (
            <div className={styles.sessionCreated}>
              <div className={styles.successMessage}>
                <h3>Sessão criada com sucesso!</h3>
                <p>Código de acesso: <strong>{sessionData.codigoAcesso}</strong></p>
              </div>

              <div className={styles.sessionActions}>
                <button onClick={handleOpenHost} className={styles.hostButton}>
                  <Play size={16} />
                  Abrir Painel do Apresentador
                </button>

                <div className={styles.joinSection}>
                  <h4>Link para participantes:</h4>
                  <div className={styles.urlContainer}>
                    <input
                      type="text"
                      value={`${window.location.origin}/interactive/join/${sessionData.codigoAcesso}`}
                      readOnly
                      className={styles.urlInput}
                    />
                    <button onClick={handleCopyJoinUrl} className={styles.copyButton}>
                      Copiar
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default InteractiveSessionModal;
