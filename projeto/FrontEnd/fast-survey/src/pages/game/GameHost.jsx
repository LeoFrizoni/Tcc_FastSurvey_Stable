import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Play, Pause, SkipForward, Users, Clock, Trophy, 
  Settings, ArrowLeft, Eye, EyeOff, Volume2, VolumeX 
} from 'lucide-react';
import axios from 'axios';
import { toast } from 'react-toastify';
import styles from './GameHost.module.css';

const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5062';

const api = axios.create({ baseURL: API_BASE });

// Interceptor para adicionar token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

const GameHost = () => {
  const { sessionId } = useParams();
  const navigate = useNavigate();
  const [sessionData, setSessionData] = useState(null);
  const [currentQuestion, setCurrentQuestion] = useState(null);
  const [players, setPlayers] = useState([]);
  const [gameStatus, setGameStatus] = useState('waiting'); // waiting, active, paused, finished
  const [timeLeft, setTimeLeft] = useState(0);
  const [showLeaderboard, setShowLeaderboard] = useState(true);
  const [showAnswers, setShowAnswers] = useState(false);
  const [soundEnabled, setSoundEnabled] = useState(true);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const timerRef = useRef(null);
  const intervalRef = useRef(null);

  useEffect(() => {
    loadSessionData();
    const interval = setInterval(loadSessionData, 2000); // Polling a cada 2 segundos
    intervalRef.current = interval;

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [sessionId]);

  const loadSessionData = async () => {
    try {
      const [sessionResponse, leaderboardResponse] = await Promise.all([
        api.get(`/api/game/${sessionId}/status`),
        api.get(`/api/game/${sessionId}/leaderboard`)
      ]);

      if (sessionResponse.data.success) {
        setSessionData(sessionResponse.data.data);
        setGameStatus(sessionResponse.data.data.status);
        
        if (sessionResponse.data.data.currentQuestion) {
          setCurrentQuestion(sessionResponse.data.data.currentQuestion);
          setTimeLeft(sessionResponse.data.data.timeLeft || 0);
        }
      }

      if (leaderboardResponse.data.success) {
        setPlayers(leaderboardResponse.data.data.players || []);
      }

      setLoading(false);
    } catch (error) {
      console.error('Erro ao carregar dados da sessão:', error);
      setError('Erro ao carregar dados da sessão');
      setLoading(false);
    }
  };

  const startQuestion = async () => {
    try {
      const response = await api.post(`/api/game/${sessionId}/start-timer`, {
        Perguntaid: currentQuestion?.questionId,
        timeLimit: sessionData?.settings?.defaultTimeLimit || 30
      });

      if (response.data.success) {
        setGameStatus('active');
        startTimer(sessionData?.settings?.defaultTimeLimit || 30);
        toast.success('Pergunta iniciada!');
      }
    } catch (error) {
      toast.error('Erro ao iniciar pergunta');
    }
  };

  const endQuestion = async () => {
    try {
      const response = await api.post(`/api/game/${sessionId}/end-question`, {
        Perguntaid: currentQuestion?.questionId
      });

      if (response.data.success) {
        setGameStatus('waiting');
        setShowAnswers(true);
        clearTimer();
        toast.success('Pergunta finalizada!');
      }
    } catch (error) {
      toast.error('Erro ao finalizar pergunta');
    }
  };

  const nextQuestion = async () => {
    try {
      // Simular próxima pergunta (implementar lógica real)
      setShowAnswers(false);
      setCurrentQuestion(null);
      setGameStatus('waiting');
      toast.info('Próxima pergunta carregada');
    } catch (error) {
      toast.error('Erro ao carregar próxima pergunta');
    }
  };

  const startTimer = (duration) => {
    setTimeLeft(duration);
    clearTimer();
    
    timerRef.current = setInterval(() => {
      setTimeLeft((prev) => {
        if (prev <= 1) {
          clearTimer();
          endQuestion();
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
  };

  const clearTimer = () => {
    if (timerRef.current) {
      clearInterval(timerRef.current);
      timerRef.current = null;
    }
  };

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  const getQuestionTypeLabel = (type) => {
    const types = {
      1: 'Múltipla Escolha',
      2: 'Verdadeiro/Falso',
      3: 'Texto',
      4: 'Drag & Drop',
      5: 'Ordenação',
      6: 'Correspondência'
    };
    return types[type] || 'Desconhecido';
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
        <button onClick={() => navigate('/home')}>Voltar para Home</button>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      {/* Header */}
      <header className={styles.header}>
        <div className={styles.headerLeft}>
          <button className={styles.backButton} onClick={() => navigate('/home')}>
            <ArrowLeft size={20} />
            Voltar
          </button>
          <div className={styles.sessionInfo}>
            <h1>{sessionData?.hostName || 'Sessão Interativa'}</h1>
            <p>Código: {sessionData?.accessCode}</p>
          </div>
        </div>
        
        <div className={styles.headerRight}>
          <button 
            className={styles.soundButton}
            onClick={() => setSoundEnabled(!soundEnabled)}
          >
            {soundEnabled ? <Volume2 size={20} /> : <VolumeX size={20} />}
          </button>
          <button 
            className={styles.leaderboardButton}
            onClick={() => setShowLeaderboard(!showLeaderboard)}
          >
            {showLeaderboard ? <EyeOff size={20} /> : <Eye size={20} />}
            Ranking
          </button>
        </div>
      </header>

      <div className={styles.mainContent}>
        {/* Área da Pergunta */}
        <div className={styles.questionArea}>
          {currentQuestion ? (
            <>
              <div className={styles.questionHeader}>
                <h2>Pergunta {currentQuestion.order}</h2>
                <span className={styles.questionType}>
                  {getQuestionTypeLabel(currentQuestion.type)}
                </span>
              </div>

              <div className={styles.questionContent}>
                <h3>{currentQuestion.text}</h3>
                
                {currentQuestion.options && (
                  <div className={styles.optionsGrid}>
                    {currentQuestion.options.map((option, index) => (
                      <div 
                        key={option.optionId}
                        className={`${styles.optionCard} ${
                          showAnswers && option.isCorrect ? styles.correctAnswer : ''
                        }`}
                        style={{ 
                          backgroundColor: option.color || '#f3f4f6',
                          borderColor: showAnswers && option.isCorrect ? '#10b981' : '#e5e7eb'
                        }}
                      >
                        <div className={styles.optionIcon}>{option.icon || '🔘'}</div>
                        <div className={styles.optionText}>{option.text}</div>
                        {showAnswers && option.isCorrect && (
                          <div className={styles.correctBadge}>✓</div>
                        )}
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* Timer */}
              {gameStatus === 'active' && (
                <div className={styles.timerContainer}>
                  <div className={`${styles.timer} ${timeLeft <= 10 ? styles.timerWarning : ''}`}>
                    <Clock size={24} />
                    <span>{formatTime(timeLeft)}</span>
                  </div>
                </div>
              )}

              {/* Controles da Pergunta */}
              <div className={styles.questionControls}>
                {gameStatus === 'waiting' && (
                  <button className={styles.startButton} onClick={startQuestion}>
                    <Play size={16} />
                    Iniciar Pergunta
                  </button>
                )}
                
                {gameStatus === 'active' && (
                  <button className={styles.endButton} onClick={endQuestion}>
                    <Pause size={16} />
                    Finalizar Pergunta
                  </button>
                )}
                
                {showAnswers && (
                  <button className={styles.nextButton} onClick={nextQuestion}>
                    <SkipForward size={16} />
                    Próxima Pergunta
                  </button>
                )}
              </div>
            </>
          ) : (
            <div className={styles.noQuestion}>
              <h2>Nenhuma pergunta ativa</h2>
              <p>Selecione uma pergunta para começar</p>
            </div>
          )}
        </div>

        {/* Sidebar com Participantes */}
        {showLeaderboard && (
          <aside className={styles.sidebar}>
            <div className={styles.sidebarHeader}>
              <h3>Participantes ({players.length})</h3>
              <Users size={20} />
            </div>
            
            <div className={styles.playersList}>
              {players.length === 0 ? (
                <p className={styles.noPlayers}>Nenhum participante ainda</p>
              ) : (
                players.map((player, index) => (
                  <div key={player.id} className={styles.playerCard}>
                    <div className={styles.playerRank}>#{index + 1}</div>
                    <div className={styles.playerInfo}>
                      <div className={styles.playerName}>{player.name}</div>
                      {player.teamName && (
                        <div className={styles.playerTeam}>{player.teamName}</div>
                      )}
                    </div>
                    <div className={styles.playerScore}>
                      <Trophy size={16} />
                      {player.score}
                    </div>
                  </div>
                ))
              )}
            </div>
          </aside>
        )}
      </div>
    </div>
  );
};

export default GameHost;
