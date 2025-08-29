import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  ChevronLeft, 
  ChevronRight, 
  Play, 
  Pause, 
  Users, 
  Eye, 
  EyeOff,
  SkipForward,
  SkipBack,
  Home
} from 'lucide-react';
import api from '../../config/api';
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

  useEffect(() => {
    loadSession();
    const interval = setInterval(loadSession, 2000); // Atualiza a cada 2 segundos
    return () => clearInterval(interval);
  }, [sessionId]);

  const loadSession = async () => {
    try {
      const response = await api.get(`/api/pesquisa-interativa/sessao/${sessionId}`);
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
      await api.post(`/api/pesquisa-interativa/ativar-pergunta/${sessionId}`);
      setQuestionActive(true);
    } catch (err) {
      console.error('Erro ao ativar pergunta:', err);
    }
  };

  const deactivateQuestion = async () => {
    try {
      await api.post(`/api/pesquisa-interativa/desativar-pergunta/${sessionId}`);
      setQuestionActive(false);
    } catch (err) {
      console.error('Erro ao desativar pergunta:', err);
    }
  };

  const nextQuestion = async () => {
    try {
      await api.post(`/api/pesquisa-interativa/avancar-pergunta/${sessionId}`);
      setQuestionActive(false);
    } catch (err) {
      console.error('Erro ao avançar pergunta:', err);
    }
  };

  const previousQuestion = async () => {
    try {
      await api.post(`/api/pesquisa-interativa/voltar-pergunta/${sessionId}`);
      setQuestionActive(false);
    } catch (err) {
      console.error('Erro ao voltar pergunta:', err);
    }
  };

  const goToQuestion = async (questionId) => {
    try {
      await api.post(`/api/pesquisa-interativa/ir-para-pergunta/${sessionId}`, {
        perguntaId: questionId
      });
      setQuestionActive(false);
    } catch (err) {
      console.error('Erro ao ir para pergunta:', err);
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

  if (error) {
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

  if (!session) {
    return (
      <div className={styles.error}>
        <p>Sessão não encontrada</p>
        <button onClick={() => navigate('/home')} className={styles.homeButton}>
          <Home size={16} />
          Voltar ao Início
        </button>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div className={styles.sessionInfo}>
          <h1>{session.tituloPesquisa}</h1>
          <p>Código: <strong>{session.codigoAcesso}</strong></p>
        </div>
        <div className={styles.participants}>
          <Users size={20} />
          <span>{participants.length} participantes</span>
        </div>
      </div>

      <div className={styles.mainContent}>
        <div className={styles.questionSection}>
          {currentQuestion ? (
            <div className={styles.currentQuestion}>
              <h2>Pergunta Atual</h2>
              <div className={styles.questionCard}>
                <h3>{currentQuestion.texto}</h3>
                <div className={styles.questionStatus}>
                  <span className={questionActive ? styles.active : styles.inactive}>
                    {questionActive ? (
                      <>
                        <Eye size={16} />
                        Ativa para respostas
                      </>
                    ) : (
                      <>
                        <EyeOff size={16} />
                        Inativa
                      </>
                    )}
                  </span>
                </div>
              </div>
            </div>
          ) : (
            <div className={styles.noQuestion}>
              <h2>Nenhuma pergunta selecionada</h2>
              <p>Selecione uma pergunta para começar</p>
            </div>
          )}

          <div className={styles.controls}>
            <button 
              onClick={previousQuestion} 
              disabled={!currentQuestion}
              className={styles.controlButton}
            >
              <SkipBack size={20} />
              Anterior
            </button>

            {questionActive ? (
              <button onClick={deactivateQuestion} className={styles.controlButton}>
                <Pause size={20} />
                Pausar Respostas
              </button>
            ) : (
              <button 
                onClick={activateQuestion} 
                disabled={!currentQuestion}
                className={styles.controlButton}
              >
                <Play size={20} />
                Ativar Respostas
              </button>
            )}

            <button 
              onClick={nextQuestion} 
              disabled={!currentQuestion}
              className={styles.controlButton}
            >
              <SkipForward size={20} />
              Próxima
            </button>
          </div>
        </div>

        <div className={styles.sidebar}>
          <div className={styles.questionsList}>
            <h3>Perguntas</h3>
            <div className={styles.questions}>
              {questions.map((question, index) => (
                <button
                  key={question.perguntaId}
                  onClick={() => goToQuestion(question.perguntaId)}
                  className={`${styles.questionItem} ${
                    currentQuestion?.perguntaId === question.perguntaId ? styles.active : ''
                  }`}
                >
                  <span className={styles.questionNumber}>{index + 1}</span>
                  <span className={styles.questionText}>{question.texto}</span>
                </button>
              ))}
            </div>
          </div>

          <div className={styles.participantsList}>
            <h3>Participantes Ativos</h3>
            <div className={styles.participants}>
              {participants.length > 0 ? (
                participants.map((participant) => (
                  <div key={participant.participanteId} className={styles.participant}>
                    <span className={styles.participantName}>{participant.nomeParticipante}</span>
                    <span className={styles.participantTime}>
                      {new Date(participant.entrouEm).toLocaleTimeString()}
                    </span>
                  </div>
                ))
              ) : (
                <p className={styles.noParticipants}>Nenhum participante conectado</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default InteractiveHost;
