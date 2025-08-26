import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Users, Gamepad2, ArrowRight, Sparkles } from 'lucide-react';
import axios from 'axios';
import { toast } from 'react-toastify';
import styles from './GameJoin.module.css';

const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5062';

const api = axios.create({ baseURL: API_BASE });

const GameJoin = () => {
  const { accessCode } = useParams();
  const navigate = useNavigate();
  const [sessionData, setSessionData] = useState(null);
  const [playerName, setPlayerName] = useState('');
  const [teamName, setTeamName] = useState('');
  const [isJoining, setIsJoining] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadSessionInfo();
  }, [accessCode]);

  const loadSessionInfo = async () => {
    try {
      const response = await api.get(`/api/game/join/${accessCode}`);
      
      if (response.data.success) {
        setSessionData(response.data.data);
      } else {
        setError('Sessão não encontrada ou inválida');
      }
    } catch (error) {
      console.error('Erro ao carregar sessão:', error);
      setError('Erro ao carregar informações da sessão');
    } finally {
      setLoading(false);
    }
  };

  const handleJoinGame = async (e) => {
    e.preventDefault();
    
    if (!playerName.trim()) {
      toast.error('Por favor, insira seu nome');
      return;
    }

    setIsJoining(true);
    try {
      const response = await api.post('/api/game/join', {
        accessCode,
        playerName: playerName.trim(),
        teamName: teamName.trim() || null
      });

      if (response.data.success) {
        const playerData = response.data.data;
        localStorage.setItem('gamePlayerData', JSON.stringify(playerData));
        toast.success('Entrou na sessão com sucesso!');
        navigate(`/game/player/${playerData.sessionId}`);
      } else {
        toast.error(response.data.message || 'Erro ao entrar na sessão');
      }
    } catch (error) {
      console.error('Erro ao entrar na sessão:', error);
      toast.error('Erro ao entrar na sessão. Tente novamente.');
    } finally {
      setIsJoining(false);
    }
  };

  if (loading) {
    return (
      <div className={styles.loadingContainer}>
        <div className={styles.loadingSpinner}></div>
        <p>Carregando sessão...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.errorContainer}>
        <h2>Erro</h2>
        <p>{error}</p>
        <button onClick={() => navigate('/')}>Voltar</button>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.joinCard}>
        <div className={styles.header}>
          <div className={styles.gameIcon}>
            <Gamepad2 size={32} />
          </div>
          <h1>Entrar no Jogo</h1>
          <p className={styles.sessionCode}>Código: {accessCode}</p>
        </div>

        {sessionData && (
          <div className={styles.sessionInfo}>
            <h2>{sessionData.hostName}</h2>
            <div className={styles.sessionStats}>
              <div className={styles.stat}>
                <Users size={16} />
                <span>{sessionData.totalPlayers || 0} participantes</span>
              </div>
              <div className={styles.stat}>
                <Sparkles size={16} />
                <span>Sessão ativa</span>
              </div>
            </div>
          </div>
        )}

        <form onSubmit={handleJoinGame} className={styles.joinForm}>
          <div className={styles.inputGroup}>
            <label htmlFor="playerName">Seu Nome *</label>
            <input
              id="playerName"
              type="text"
              value={playerName}
              onChange={(e) => setPlayerName(e.target.value)}
              placeholder="Digite seu nome"
              maxLength={20}
              required
            />
          </div>

          {sessionData?.settings?.enableTeamMode && (
            <div className={styles.inputGroup}>
              <label htmlFor="teamName">Nome da Equipe (opcional)</label>
              <input
                id="teamName"
                type="text"
                value={teamName}
                onChange={(e) => setTeamName(e.target.value)}
                placeholder="Digite o nome da equipe"
                maxLength={15}
              />
            </div>
          )}

          <button 
            type="submit" 
            className={styles.joinButton}
            disabled={isJoining || !playerName.trim()}
          >
            {isJoining ? (
              <>
                <div className={styles.spinner}></div>
                Entrando...
              </>
            ) : (
              <>
                <ArrowRight size={20} />
                Entrar no Jogo
              </>
            )}
          </button>
        </form>

        <div className={styles.gameFeatures}>
          <h3>Recursos do Jogo</h3>
          <ul>
            <li>🎯 Respostas em tempo real</li>
            <li>🏆 Ranking dinâmico</li>
            <li>⚡ Pontuação por velocidade</li>
            <li>🎮 Interface gamificada</li>
            {sessionData?.settings?.enablePowerUps && (
              <li>💫 Power-ups especiais</li>
            )}
            {sessionData?.settings?.enableTeamMode && (
              <li>👥 Modo equipe</li>
            )}
          </ul>
        </div>
      </div>
    </div>
  );
};

export default GameJoin;
