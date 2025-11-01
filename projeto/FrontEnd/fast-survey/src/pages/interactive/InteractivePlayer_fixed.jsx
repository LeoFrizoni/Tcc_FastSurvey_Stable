import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Presentation, 
  CheckCircle, 
  Clock, 
  Home,
  Send
} from 'lucide-react';
import api from '../../lib/api';
import styles from './InteractivePlayer.module.css';

const InteractivePlayer = () => {
  const { sessionId } = useParams();
  const navigate = useNavigate();
  
  const [session, setSession] = useState(null);
  const [currentQuestion, setCurrentQuestion] = useState(null);
  const [participant, setParticipant] = useState(null);
  const [selectedOptions, setSelectedOptions] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [questionActive, setQuestionActive] = useState(false);
  const [hasAnswered, setHasAnswered] = useState(false);

  const loadParticipantData = useCallback(() => {
    const participantData = localStorage.getItem('interactiveParticipantData');
    if (participantData) {
      try {
        setParticipant(JSON.parse(participantData));
      } catch (err) {
        console.error('Erro ao carregar dados do participante:', err);
        navigate('/home');
      }
    } else {
      navigate('/home');
    }
  }, [navigate]);

  const loadSession = useCallback(async () => {
    try {
      const response = await api.get(`/api/PesquisaInterativa/sessao/${sessionId}`);
      if (response.data.success) {
        const sessionData = response.data.data;
        setSession(sessionData);
        
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
  }, [sessionId]);

  useEffect(() => {
    loadParticipantData();
    loadSession();
    const interval = setInterval(loadSession, 2000); // Atualiza a cada 2 segundos
    return () => clearInterval(interval);
  }, [loadParticipantData, loadSession]);

  const handleOptionSelect = (optionId) => {
    if (!questionActive || hasAnswered) return;

    if (currentQuestion?.permiteMultiplasSelecao) {
      setSelectedOptions(prev => 
        prev.includes(optionId) 
          ? prev.filter(id => id !== optionId)
          : [...prev, optionId]
      );
    } else {
      setSelectedOptions([optionId]);
    }
  };

  const handleSubmitAnswer = async () => {
    if (!questionActive || hasAnswered || selectedOptions.length === 0) return;

    setIsSubmitting(true);
    setError('');

    try {
      const response = await api.post('/api/pesquisa-interativa/responder', {
        sessaoId: sessionId,
        participanteId: participant.participanteId,
        perguntaId: currentQuestion.perguntaId,
        opcoesSelecionadas: selectedOptions,
        textoResposta: ''
      });

      if (response.data.success) {
        setHasAnswered(true);
        setSelectedOptions([]);
      }
    } catch (err) {
      console.error('Erro ao enviar resposta:', err);
      setError('Erro ao enviar resposta');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleLeaveSession = () => {
    localStorage.removeItem('interactiveParticipantData');
    navigate('/home');
  };

  if (isLoading) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>
          <div className={styles.spinner}></div>
          <p>Carregando sessão...</p>
        </div>
      </div>
    );
  }

  if (!session || !participant) {
    return (
      <div className={styles.container}>
        <div className={styles.error}>
          <h2>Sessão não encontrada</h2>
          <p>Não foi possível carregar a sessão interativa.</p>
          <button onClick={() => navigate('/home')} className={styles.homeButton}>
            <Home size={16} />
            Voltar ao Início
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <div className={styles.sessionInfo}>
          <h1>{session.titulo}</h1>
          <p>Participante: <strong>{participant.nome}</strong></p>
        </div>
        <div className={styles.status}>
          <span className={questionActive ? styles.active : styles.inactive}>
            {questionActive ? (
              <>
                <CheckCircle size={16} />
                Pergunta Ativa
              </>
            ) : (
              <>
                <Clock size={16} />
                Aguardando
              </>
            )}
          </span>
        </div>
        <button onClick={handleLeaveSession} className={styles.leaveButton}>
          Sair
        </button>
      </div>

      <div className={styles.mainContent}>
        {currentQuestion ? (
          <div className={styles.questionSection}>
            <div className={styles.questionCard}>
              <h2>{currentQuestion.texto}</h2>
              
              {questionActive && !hasAnswered && (
                <div className={styles.options}>
                  {currentQuestion.opcoes?.map((option) => (
                    <button
                      key={option.opcaoId}
                      onClick={() => handleOptionSelect(option.opcaoId)}
                      className={`${styles.option} ${
                        selectedOptions.includes(option.opcaoId) ? styles.selected : ''
                      }`}
                      disabled={isSubmitting}
                    >
                      {option.texto}
                    </button>
                  ))}
                </div>
              )}

              {questionActive && hasAnswered && (
                <div className={styles.answered}>
                  <CheckCircle size={24} />
                  <span>Resposta enviada!</span>
                </div>
              )}

              {!questionActive && (
                <div className={styles.waiting}>
                  <Presentation size={24} />
                  <span>Aguardando o apresentador ativar as respostas...</span>
                </div>
              )}
            </div>

            {questionActive && !hasAnswered && selectedOptions.length > 0 && (
              <button
                onClick={handleSubmitAnswer}
                disabled={isSubmitting}
                className={styles.submitButton}
              >
                {isSubmitting ? 'Enviando...' : (
                  <>
                    <Send size={16} />
                    Enviar Resposta
                  </>
                )}
              </button>
            )}
          </div>
        ) : (
          <div className={styles.noQuestion}>
            <div className={styles.waitingMessage}>
              <Presentation size={48} />
              <h2>Aguardando o apresentador</h2>
              <p>O apresentador irá selecionar uma pergunta em breve...</p>
            </div>
          </div>
        )}
      </div>

      {error && (
        <div className={styles.errorMessage}>
          {error}
        </div>
      )}
    </div>
  );
};

export default InteractivePlayer;
