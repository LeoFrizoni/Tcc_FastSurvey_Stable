import React, { useState, useEffect } from 'react';
import { Play, Users, Clock, Trophy, Settings, X, Gamepad2 } from 'lucide-react';
import axios from 'axios';
import { toast } from 'react-toastify';
import styles from './InteractiveSessionModal.module.css';

const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5062';

const api = axios.create({ baseURL: API_BASE });

// Interceptor para adicionar token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

const InteractiveSessionModal = ({ isOpen, onClose, pesquisaId, pesquisaTitulo }) => {
  const [sessionConfig, setSessionConfig] = useState({
    enableTimer: true,
    defaultTimeLimit: 30,
    showLeaderboard: true,
    enableSound: true,
    enableAnimations: true,
    allowLateJoining: true,
    maxPlayers: 100,
    enableTeamMode: false,
    showCorrectAnswers: true,
    enablePowerUps: false
  });

  const [isCreating, setIsCreating] = useState(false);
  const [sessionData, setSessionData] = useState(null);

  const handleCreateSession = async () => {
    setIsCreating(true);
    try {
      const hostName = localStorage.getItem('usuario') || 'Professor';
      
      const response = await api.post('/api/game/create', {
        PesquisaId: parseInt(pesquisaId),
        hostName,
        settings: sessionConfig
      });

      if (response.data.success) {
        setSessionData(response.data.data);
        toast.success('Sessão interativa criada com sucesso!');
      } else {
        toast.error(response.data.message || 'Erro ao criar sessão');
      }
    } catch (error) {
      console.error('Erro ao criar sessão:', error);
      toast.error('Erro ao criar sessão interativa. Tente novamente.');
    } finally {
      setIsCreating(false);
    }
  };

  const handleStartSession = () => {
    if (sessionData) {
      // Redirecionar para a página de controle da sessão
      window.open(`/game/host/${sessionData.sessionId}`, '_blank');
      onClose();
    }
  };

  const copyAccessCode = () => {
    if (sessionData?.accessCode) {
      navigator.clipboard.writeText(sessionData.accessCode);
      toast.success('Código de acesso copiado!');
    }
  };

  const copyJoinUrl = () => {
    if (sessionData?.accessCode) {
      const joinUrl = `${window.location.origin}/game/join/${sessionData.accessCode}`;
      navigator.clipboard.writeText(joinUrl);
      toast.success('Link de acesso copiado!');
    }
  };

  if (!isOpen) return null;

  return (
    <div className={styles.modalOverlay}>
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <div className={styles.modalTitle}>
            <Gamepad2 size={24} />
            <h2>Sessão Interativa</h2>
          </div>
          <button className={styles.closeButton} onClick={onClose}>
            <X size={20} />
          </button>
        </div>

        <div className={styles.modalContent}>
          {!sessionData ? (
            <>
              <div className={styles.sessionInfo}>
                <h3>{pesquisaTitulo}</h3>
                <p>Configure as opções da sua sessão gamificada</p>
              </div>

              <div className={styles.configSection}>
                <h4>Configurações da Sessão</h4>
                
                <div className={styles.configGrid}>
                  <div className={styles.configItem}>
                    <label>
                      <input
                        type="checkbox"
                        checked={sessionConfig.enableTimer}
                        onChange={(e) => setSessionConfig(prev => ({
                          ...prev,
                          enableTimer: e.target.checked
                        }))}
                      />
                      Timer por pergunta
                    </label>
                  </div>

                  <div className={styles.configItem}>
                    <label>
                      <input
                        type="checkbox"
                        checked={sessionConfig.showLeaderboard}
                        onChange={(e) => setSessionConfig(prev => ({
                          ...prev,
                          showLeaderboard: e.target.checked
                        }))}
                      />
                      Mostrar ranking
                    </label>
                  </div>

                  <div className={styles.configItem}>
                    <label>
                      <input
                        type="checkbox"
                        checked={sessionConfig.enableSound}
                        onChange={(e) => setSessionConfig(prev => ({
                          ...prev,
                          enableSound: e.target.checked
                        }))}
                      />
                      Efeitos sonoros
                    </label>
                  </div>

                  <div className={styles.configItem}>
                    <label>
                      <input
                        type="checkbox"
                        checked={sessionConfig.enableTeamMode}
                        onChange={(e) => setSessionConfig(prev => ({
                          ...prev,
                          enableTeamMode: e.target.checked
                        }))}
                      />
                      Modo equipe
                    </label>
                  </div>

                  <div className={styles.configItem}>
                    <label>
                      <input
                        type="checkbox"
                        checked={sessionConfig.enablePowerUps}
                        onChange={(e) => setSessionConfig(prev => ({
                          ...prev,
                          enablePowerUps: e.target.checked
                        }))}
                      />
                      Power-ups
                    </label>
                  </div>

                  <div className={styles.configItem}>
                    <label>
                      <input
                        type="checkbox"
                        checked={sessionConfig.showCorrectAnswers}
                        onChange={(e) => setSessionConfig(prev => ({
                          ...prev,
                          showCorrectAnswers: e.target.checked
                        }))}
                      />
                      Mostrar respostas corretas
                    </label>
                  </div>
                </div>

                <div className={styles.timeConfig}>
                  <label>
                    Tempo por pergunta (segundos):
                    <input
                      type="number"
                      min="5"
                      max="120"
                      value={sessionConfig.defaultTimeLimit}
                      onChange={(e) => setSessionConfig(prev => ({
                        ...prev,
                        defaultTimeLimit: parseInt(e.target.value) || 30
                      }))}
                    />
                  </label>
                </div>

                <div className={styles.maxPlayersConfig}>
                  <label>
                    Máximo de participantes:
                    <input
                      type="number"
                      min="1"
                      max="200"
                      value={sessionConfig.maxPlayers}
                      onChange={(e) => setSessionConfig(prev => ({
                        ...prev,
                        maxPlayers: parseInt(e.target.value) || 100
                      }))}
                    />
                  </label>
                </div>
              </div>

              <div className={styles.modalActions}>
                <button
                  className={styles.cancelButton}
                  onClick={onClose}
                  disabled={isCreating}
                >
                  Cancelar
                </button>
                <button
                  className={styles.createButton}
                  onClick={handleCreateSession}
                  disabled={isCreating}
                >
                  {isCreating ? 'Criando...' : 'Criar Sessão'}
                </button>
              </div>
            </>
          ) : (
            <>
              <div className={styles.sessionCreated}>
                <div className={styles.successIcon}>🎮</div>
                <h3>Sessão Criada com Sucesso!</h3>
                <p>Sua sessão gamificada está pronta para começar</p>
              </div>

              <div className={styles.sessionDetails}>
                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Código de Acesso:</span>
                  <div className={styles.accessCodeContainer}>
                    <span className={styles.accessCode}>{sessionData.accessCode}</span>
                    <button className={styles.copyButton} onClick={copyAccessCode}>
                      Copiar
                    </button>
                  </div>
                </div>

                <div className={styles.detailItem}>
                  <span className={styles.detailLabel}>Link de Acesso:</span>
                  <div className={styles.linkContainer}>
                    <span className={styles.joinLink}>
                      {`${window.location.origin}/game/join/${sessionData.accessCode}`}
                    </span>
                    <button className={styles.copyButton} onClick={copyJoinUrl}>
                      Copiar
                    </button>
                  </div>
                </div>

                <div className={styles.sessionStats}>
                  <div className={styles.statItem}>
                    <Users size={16} />
                    <span>0 participantes</span>
                  </div>
                  <div className={styles.statItem}>
                    <Clock size={16} />
                    <span>Pronta para iniciar</span>
                  </div>
                </div>
              </div>

              <div className={styles.modalActions}>
                <button
                  className={styles.secondaryButton}
                  onClick={() => setSessionData(null)}
                >
                  Nova Configuração
                </button>
                <button
                  className={styles.startButton}
                  onClick={handleStartSession}
                >
                  <Play size={16} />
                  Iniciar Sessão
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default InteractiveSessionModal;
