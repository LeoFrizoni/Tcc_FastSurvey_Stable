import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Play, 
  Pause, 
  Users, 
  Eye, 
  SkipForward,
  SkipBack,
  Home,
  BarChart3,
  Share2,
  AlertCircle,
  CheckCircle
} from 'lucide-react';
import TopNavbar from '../../components/layouts/TopNavBar';
import api from '../../lib/api';
import styles from './InteractiveHost.module.css';

const InteractiveHost = () => {
  const { sessionId } = useParams();
  const navigate = useNavigate();
  
  const [session, setSession] = useState(null);
  const [currentQuestion, setCurrentQuestion] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [participants, setParticipants] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [questionActive, setQuestionActive] = useState(false);
  const [showResults, setShowResults] = useState(false);

  useEffect(() => {
    loadSession();
    const interval = setInterval(loadSession, 2000); // Atualiza a cada 2 segundos
    return () => clearInterval(interval);
  }, [sessionId, loadSession]);

  const loadSession = async () => {
    try {
      const response = await api.get(`/api/PesquisaInterativa/sessao/${sessionId}`);
      if (response.data.success) {
        const sessionData = response.data.data;
        setSession(sessionData);
        setQuestions(sessionData.perguntas || []);
        setParticipants(sessionData.participantes || []);
        
        // Encontra a pergunta atual
        if (sessionData.perguntaAtualId) {
          const current = sessionData.perguntas?.find(q => q.perguntaId === sessionData.perguntaAtualId);
          setCurrentQuestion(current);
        }
        
        setQuestionActive(sessionData.perguntaAtiva || false);
        setIsLoading(false);
      }
    } catch (err) {
      console.error('Erro ao carregar sessão:', err);
      setError('Erro ao carregar sessão');
      setIsLoading(false);
    }
  };

  const activateQuestion = async () => {
    try {
      await api.post('/api/pesquisa-interativa/ativar', {
        sessaoId: sessionId,
        ativar: true
      });
      setQuestionActive(true);
    } catch (err) {
      console.error('Erro ao ativar pergunta:', err);
    }
  };

  const deactivateQuestion = async () => {
    try {
      await api.post('/api/pesquisa-interativa/ativar', {
        sessaoId: sessionId,
        ativar: false
      });
      setQuestionActive(false);
    } catch (err) {
      console.error('Erro ao desativar pergunta:', err);
    }
  };

  const nextQuestion = async () => {
    try {
      await api.post('/api/pesquisa-interativa/avancar', {
        sessaoId: sessionId,
        ativarPergunta: false
      });
      setQuestionActive(false);
    } catch (err) {
      console.error('Erro ao avançar pergunta:', err);
    }
  };

  const previousQuestion = async () => {
    try {
      await api.post('/api/pesquisa-interativa/voltar', {
        sessaoId: sessionId,
        ativarPergunta: false
      });
      setQuestionActive(false);
    } catch (err) {
      console.error('Erro ao voltar pergunta:', err);
    }
  };

  const endSession = async () => {
    if (!window.confirm('Tem certeza que deseja encerrar a sessão?')) {
      return;
    }
    
    try {
      await api.post('/api/pesquisa-interativa/encerrar', {
        sessaoId: sessionId
      });
      navigate('/minhas-pesquisas');
    } catch (err) {
      console.error('Erro ao encerrar sessão:', err);
    }
  };

  if (isLoading) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.loading}>
            <div className={styles.loadingSpinner}></div>
            <p>Carregando sessão interativa...</p>
          </div>
        </div>
      </>
    );
  }

  if (error || !session) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.error}>
            <AlertCircle size={48} />
            <h2>Erro ao carregar sessão</h2>
            <p>{error || 'Não foi possível carregar a sessão interativa'}</p>
            <button onClick={() => navigate('/minhas-pesquisas')} className={styles.retryBtn}>
              Voltar para Minhas Pesquisas
            </button>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <TopNavbar />
      <div className={styles.container}>
        <div className={styles.header}>
          <div className={styles.headerContent}>
            <h1>Sessão Interativa</h1>
            <p>{session.pesquisaTitulo || 'Pesquisa'}</p>
            <div className={styles.sessionInfo}>
              <span className={styles.sessionId}>ID: {sessionId}</span>
              <span className={`${styles.statusBadge} ${questionActive ? styles.active : styles.inactive}`}>
                {questionActive ? 'Pergunta Ativa' : 'Pergunta Inativa'}
              </span>
            </div>
          </div>
          
          <div className={styles.headerActions}>
            <button 
              className={styles.actionBtn}
              onClick={() => setShowResults(!showResults)}
            >
              <BarChart3 size={20} />
              {showResults ? 'Ocultar' : 'Mostrar'} Resultados
            </button>
            <button 
              className={styles.actionBtn}
              onClick={() => {
                navigator.clipboard.writeText(`${window.location.origin}/interactive/join/${sessionId}`);
                alert('Link copiado para a área de transferência!');
              }}
            >
              <Share2 size={20} />
              Compartilhar
            </button>
          </div>
        </div>

        <div className={styles.content}>
          <div className={styles.mainSection}>
            {/* Pergunta Atual */}
            <div className={styles.questionCard}>
              <div className={styles.questionHeader}>
                <h2>Pergunta Atual</h2>
                <div className={styles.questionControls}>
                  <button 
                    className={styles.controlBtn}
                    onClick={previousQuestion}
                    disabled={!currentQuestion}
                  >
                    <SkipBack size={20} />
                  </button>
                  
                  {questionActive ? (
                    <button 
                      className={`${styles.controlBtn} ${styles.activeBtn}`}
                      onClick={deactivateQuestion}
                    >
                      <Pause size={20} />
                      Pausar
                    </button>
                  ) : (
                    <button 
                      className={styles.controlBtn}
                      onClick={activateQuestion}
                      disabled={!currentQuestion}
                    >
                      <Play size={20} />
                      Ativar
                    </button>
                  )}
                  
                  <button 
                    className={styles.controlBtn}
                    onClick={nextQuestion}
                    disabled={!currentQuestion}
                  >
                    <SkipForward size={20} />
                  </button>
                </div>
              </div>
              
              <div className={styles.questionContent}>
                {currentQuestion ? (
                  <>
                    <h3>{currentQuestion.texto}</h3>
                    <div className={styles.questionOptions}>
                      {currentQuestion.opcoes?.map((opcao, index) => (
                        <div key={opcao.opcaoId} className={styles.optionItem}>
                          <span className={styles.optionLetter}>
                            {String.fromCharCode(65 + index)}
                          </span>
                          <span className={styles.optionText}>{opcao.texto}</span>
                          {showResults && (
                            <span className={styles.optionCount}>
                              {opcao.totalRespostas || 0} votos
                            </span>
                          )}
                        </div>
                      ))}
                    </div>
                  </>
                ) : (
                  <div className={styles.noQuestion}>
                    <p>Nenhuma pergunta selecionada</p>
                  </div>
                )}
              </div>
            </div>

            {/* Lista de Perguntas */}
            <div className={styles.questionsList}>
              <h3>Perguntas da Pesquisa</h3>
              <div className={styles.questionsGrid}>
                {questions.map((question, index) => (
                  <div 
                    key={question.perguntaId} 
                    className={`${styles.questionItem} ${currentQuestion?.perguntaId === question.perguntaId ? styles.currentQuestion : ''}`}
                  >
                    <div className={styles.questionNumber}>{index + 1}</div>
                    <div className={styles.questionText}>
                      <h4>{question.texto}</h4>
                      <p>{question.opcoes?.length || 0} opções</p>
                    </div>
                    <div className={styles.questionStatus}>
                      {currentQuestion?.perguntaId === question.perguntaId && questionActive && (
                        <CheckCircle size={16} className={styles.activeIcon} />
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          <div className={styles.sidebar}>
            {/* Participantes */}
            <div className={styles.participantsCard}>
              <div className={styles.cardHeader}>
                <h3><Users size={20} /> Participantes</h3>
                <span className={styles.participantCount}>{participants.length}</span>
              </div>
              
              <div className={styles.participantsList}>
                {participants.length > 0 ? (
                  participants.map((participant, index) => (
                    <div key={participant.id || index} className={styles.participantItem}>
                      <div className={styles.participantAvatar}>
                        {participant.nome?.charAt(0) || 'U'}
                      </div>
                      <div className={styles.participantInfo}>
                        <span className={styles.participantName}>
                          {participant.nome || `Participante ${index + 1}`}
                        </span>
                        <span className={styles.participantStatus}>
                          {participant.online ? 'Online' : 'Offline'}
                        </span>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className={styles.emptyState}>
                    <Users size={32} />
                    <p>Nenhum participante conectado</p>
                  </div>
                )}
              </div>
            </div>

            {/* Controles da Sessão */}
            <div className={styles.sessionControls}>
              <h3>Controles da Sessão</h3>
              
              <button 
                className={styles.primaryBtn}
                onClick={() => navigate(`/minhas-pesquisas/resultado/${session.pesquisaId}`)}
              >
                <BarChart3 size={18} />
                Ver Resultados Completos
              </button>
              
              <button 
                className={styles.secondaryBtn}
                onClick={() => navigate(`/responder/${session.pesquisaId}`)}
              >
                <Eye size={18} />
                Visualizar Pesquisa
              </button>
              
              <button 
                className={`${styles.secondaryBtn} ${styles.dangerBtn}`}
                onClick={endSession}
              >
                <Home size={18} />
                Encerrar Sessão
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default InteractiveHost;
