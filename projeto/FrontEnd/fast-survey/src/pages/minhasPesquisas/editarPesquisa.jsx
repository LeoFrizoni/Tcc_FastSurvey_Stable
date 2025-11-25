import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Edit3, 
  Eye, 
  BarChart3, 
  Share2, 
  Copy, 
  Trash2, 
  Settings, 
  FileText,
  AlertCircle,
  Save,
  RotateCcw,
  Calendar,
  Users,
  Clock,
  CheckCircle,
  XCircle,
  ListChecks,
  ArrowRight
} from 'lucide-react';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import styles from './editarPesquisa.module.css';
import { toast } from 'react-toastify';
import ResultadosCompletos from '../../components/charts/ResultadosCompletos';

// Configuração da API
import env from '../../config/env';
const API_BASE_URL = env.REACT_APP_API_URL;

const mapPerguntasDoTemplate = (templateJson) => {
  if (!templateJson) return {};
  let blocos = [];
  if (Array.isArray(templateJson)) {
    blocos = templateJson;
  } else {
    try {
      blocos = JSON.parse(templateJson);
    } catch {
      return {};
    }
  }

  const map = {};
  blocos.forEach((bloco) => {
    const rawId = bloco?.perguntaId ?? bloco?.PerguntaId ?? bloco?.PerguntaID;
    const perguntaId = rawId != null ? Number(rawId) : null;
    if (!perguntaId || Number.isNaN(perguntaId)) return;

    map[perguntaId] = {
      texto: bloco?.texto ?? bloco?.Texto ?? `Pergunta #${perguntaId}`,
      tipo: (bloco?.tipo ?? bloco?.Tipo ?? '').toString().toLowerCase(),
      opcoes: (bloco?.opcoes ?? bloco?.Opcoes ?? []).reduce((acc, opcao, index) => {
        const rawOpcaoId = opcao?.opcaoId ?? opcao?.OpcaoId ?? opcao?.id;
        const opcaoId = rawOpcaoId != null ? Number(rawOpcaoId) : null;
        if (opcaoId && !Number.isNaN(opcaoId)) {
          acc[opcaoId] = opcao?.texto ?? opcao?.Texto ?? `Opção ${index + 1}`;
        }
        return acc;
      }, {}),
    };
  });

  return map;
};

const formatarDataHora = (valor) => {
  if (!valor) return '-';
  const data = new Date(valor);
  if (Number.isNaN(data.getTime())) return '-';
  return data.toLocaleString('pt-BR');
};

const EditarPesquisa = () => {
  const { id: rawId } = useParams();
  const id = rawId?.replace(/[^0-9]/g, ''); // Remove caracteres não numéricos
  const navigate = useNavigate();
  const [pesquisa, setPesquisa] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [editando, setEditando] = useState(false);
  const [formData, setFormData] = useState({
    titulo: '',
    descricao: '',
    ativa: true,
    permiteRespostasAnonimas: true,
    limiteRespostas: null,
    temLimitadorTempo: false,
    dataFechamento: null
  });
  const [perguntasDetalhes, setPerguntasDetalhes] = useState({});
  const [mostrarRespostas, setMostrarRespostas] = useState(false);
  const [respostas, setRespostas] = useState([]);
  const [carregandoRespostas, setCarregandoRespostas] = useState(false);
  const [erroRespostas, setErroRespostas] = useState('');
  const [filtroUsuario, setFiltroUsuario] = useState('todos');
  const [participantesInfo, setParticipantesInfo] = useState({});
  const [mostrarGraficosPreview, setMostrarGraficosPreview] = useState(false);

  // Função para obter token de autenticação
  const getAuthHeaders = useCallback(() => {
    const token = localStorage.getItem('token') || sessionStorage.getItem('token');
    return token ? { Authorization: `Bearer ${token}` } : {};
  }, []);

  // Carregar dados da pesquisa
  useEffect(() => {
    async function carregarPesquisa() {
      if (!id || !/^\d+$/.test(id)) {
        setErro('ID da pesquisa inválido.');
        setCarregando(false);
        return;
      }
      
      try {
        setCarregando(true);
        setErro('');
        
        const { data } = await axios.get(`${API_BASE_URL}/api/pesquisas/${id}`, {
          headers: getAuthHeaders()
        });
        
        setPesquisa(data);
        setPerguntasDetalhes(mapPerguntasDoTemplate(data.templateJson));
        setFormData({
          titulo: data.titulo || '',
          descricao: data.descricao || '',
          ativa: data.ativa !== false,
          permiteRespostasAnonimas: data.permiteRespostasAnonimas !== false,
          limiteRespostas: data.limiteRespostas || null,
          temLimitadorTempo: data.temLimitadorTempo || false,
          dataFechamento: data.dataFechamento ? new Date(data.dataFechamento).toISOString().split('T')[0] : null
        });
      } catch (error) {
        console.error('Erro ao carregar pesquisa:', error);
        if (error.response?.status === 401) {
          setErro('Não autorizado. Faça login para continuar.');
          setTimeout(() => navigate('/login'), 1200);
        } else if (error.response?.status === 404) {
          setErro('Pesquisa não encontrada.');
        } else {
          setErro('Não foi possível carregar a pesquisa. Tente novamente mais tarde.');
        }
      } finally {
        setCarregando(false);
      }
    }

    if (id) {
      carregarPesquisa();
    }
  }, [id, navigate, getAuthHeaders]);

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const salvarAlteracoes = async () => {
    try {
      setEditando(true);
      
      await axios.put(`${API_BASE_URL}/api/pesquisas/${id}`, {
        titulo: formData.titulo,
        descricao: formData.descricao,
        ativa: formData.ativa,
        permiteRespostasAnonimas: formData.permiteRespostasAnonimas,
        limiteRespostas: formData.limiteRespostas ? parseInt(formData.limiteRespostas) : null,
        temLimitadorTempo: formData.temLimitadorTempo,
        dataFechamento: formData.dataFechamento ? new Date(formData.dataFechamento).toISOString() : null
      }, {
        headers: getAuthHeaders()
      });
      
      // Recarregar dados da pesquisa
      const { data } = await axios.get(`${API_BASE_URL}/api/pesquisas/${id}`, {
        headers: getAuthHeaders()
      });
      setPesquisa(data);
      setEditando(false);
      toast.success('Pesquisa atualizada com sucesso!');
    } catch (error) {
      console.error('Erro ao salvar:', error);
      setEditando(false);
      toast.error('Erro ao salvar alterações. Tente novamente.');
    }
  };

  const excluirPesquisa = async () => {
    if (!window.confirm('Tem certeza que deseja excluir esta pesquisa? Esta ação não pode ser desfeita.')) {
      return;
    }
    
    try {
      await axios.delete(`${API_BASE_URL}/api/pesquisas/${id}`, {
        headers: getAuthHeaders()
      });
      toast.success('Pesquisa excluída com sucesso!');
      navigate('/home');
    } catch (error) {
      console.error('Erro ao excluir:', error);
      toast.error('Erro ao excluir pesquisa. Tente novamente.');
    }
  };

  const copiarLink = () => {
    const link = `${window.location.origin}/responder/${id}`;
    navigator.clipboard.writeText(link);
    toast.success('Link copiado para a área de transferência!');
  };

  const resetarFormulario = () => {
    if (pesquisa) {
      setFormData({
        titulo: pesquisa.titulo || '',
        descricao: pesquisa.descricao || '',
        ativa: pesquisa.ativa !== false,
        permiteRespostasAnonimas: pesquisa.permiteRespostasAnonimas !== false,
        limiteRespostas: pesquisa.limiteRespostas || null,
        temLimitadorTempo: pesquisa.temLimitadorTempo || false,
        dataFechamento: pesquisa.dataFechamento ? new Date(pesquisa.dataFechamento).toISOString().split('T')[0] : null
      });
    }
  };

  const atualizarParticipantesInfo = useCallback(
    async (lista) => {
      const ids = Array.from(
        new Set(
          (lista || [])
            .map((resposta) => resposta?.participanteId ?? resposta?.ParticipanteId)
            .filter((id) => id != null)
        )
      );
      const faltantes = ids.filter((id) => !participantesInfo[id]);
      if (faltantes.length === 0) return;

      try {
        const headers = getAuthHeaders();
        const resultados = await Promise.all(
          faltantes.map(async (pid) => {
            try {
              const { data } = await axios.get(`${API_BASE_URL}/api/ParticipanteSessao/${pid}`, {
                headers,
              });
              const nome =
                data?.nomeParticipante ??
                data?.NomeParticipante ??
                data?.nome ??
                data?.Nome ??
                `Participante #${pid}`;
              return { pid, nome };
            } catch (error) {
              console.error('Erro ao buscar participante', pid, error);
              return { pid, nome: `Participante #${pid}` };
            }
          })
        );

        setParticipantesInfo((prev) => {
          const next = { ...prev };
          resultados.forEach(({ pid, nome }) => {
            next[pid] = nome;
          });
          return next;
        });
      } catch (error) {
        console.error('Erro ao carregar informações de participantes', error);
      }
    },
    [getAuthHeaders, participantesInfo]
  );

  const carregarRespostas = useCallback(async () => {
    if (!id) return;
    setCarregandoRespostas(true);
    setErroRespostas('');
    try {
      const { data } = await axios.get(`${API_BASE_URL}/api/respostas/pesquisa/${id}`, {
        headers: getAuthHeaders(),
      });
      const lista = Array.isArray(data) ? data : [];
      setRespostas(lista);
      await atualizarParticipantesInfo(lista);
    } catch (error) {
      console.error('Erro ao carregar respostas:', error);
      setErroRespostas('Não foi possível carregar as respostas desta pesquisa.');
      toast.error('Não foi possível carregar as respostas.');
    } finally {
      setCarregandoRespostas(false);
    }
  }, [id, getAuthHeaders, atualizarParticipantesInfo]);

  const abrirModalRespostas = () => {
    setMostrarRespostas(true);
    if (respostas.length === 0) {
      carregarRespostas();
    }
  };

  const fecharModalRespostas = () => {
    setMostrarRespostas(false);
    setFiltroUsuario('todos');
  };

  const toggleGraficosPreview = () => {
    setMostrarGraficosPreview((prev) => !prev);
  };

  const rotuloParticipante = useCallback(
    (participanteId) => {
      if (!participanteId) return 'Anônimo';
      return participantesInfo[participanteId] || `Participante #${participanteId}`;
    },
    [participantesInfo]
  );

  const respostasFiltradas = useMemo(() => {
    if (filtroUsuario === 'todos') return respostas;
    if (filtroUsuario === 'anon')
      return respostas.filter((resposta) => !(resposta?.participanteId ?? resposta?.ParticipanteId));
    return respostas.filter(
      (resposta) => String(resposta?.participanteId ?? resposta?.ParticipanteId) === filtroUsuario
    );
  }, [respostas, filtroUsuario]);

  const participantesOptions = useMemo(() => {
    const ids = Array.from(
      new Set(
        respostas
          .map((resposta) => resposta?.participanteId ?? resposta?.ParticipanteId)
          .filter((id) => id != null)
      )
    );
    return ids.map((id) => ({ id: String(id), label: participantesInfo[id] || `Participante #${id}` }));
  }, [respostas, participantesInfo]);

  const renderRespostaConteudo = useCallback(
    (resposta) => {
      const texto = resposta?.texto ?? resposta?.Texto;
      const opcoesSelecionadas = resposta?.opcoes ?? resposta?.Opcoes ?? [];
      const perguntaId = resposta?.perguntaId ?? resposta?.PerguntaId;
      const detalhe = perguntaId ? perguntasDetalhes[Number(perguntaId)] : null;

      if (opcoesSelecionadas.length > 0) {
        return (
          <ul className={styles.responseOptions}>
            {opcoesSelecionadas.map((opcaoId) => (
              <li key={`${perguntaId}-${opcaoId}`}>
                {detalhe?.opcoes?.[opcaoId] ?? `Opção #${opcaoId}`}
              </li>
            ))}
          </ul>
        );
      }

      if (texto) {
        return <p className={styles.responseText}>{texto}</p>;
      }

      return <p className={styles.emptyAnswer}>Resposta sem conteúdo.</p>;
    },
    [perguntasDetalhes]
  );

  if (carregando) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.loading}>
            <div className={styles.loadingSpinner}></div>
            <p>Carregando pesquisa...</p>
          </div>
        </div>
      </>
    );
  }

  if (erro || !pesquisa) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.error}>
            <AlertCircle size={48} />
            <h2>Erro</h2>
            <p>{erro || 'Pesquisa não encontrada'}</p>
            <div className={styles.errorActions}>
              <button onClick={() => navigate('/home')}>
                Voltar para Home
              </button>
            </div>
          </div>
        </div>
      </>
    );
  }

  const getStatusBadge = () => {
    if (!pesquisa.ativa) return { text: 'Inativa', color: 'inactive', icon: XCircle };
    if (pesquisa.temLimitadorTempo && new Date(pesquisa.dataFechamento) < new Date()) {
      return { text: 'Expirada', color: 'expired', icon: Clock };
    }
    return { text: 'Ativa', color: 'active', icon: CheckCircle };
  };

  const status = getStatusBadge();
  const StatusIcon = status.icon;

  return (
    <>
      <TopNavbar />
      <div className={styles.container}>
        {/* Header */}
        <div className={styles.header}>
          <div className={styles.headerContent}>
            <div className={styles.headerTitle}>
              <Edit3 size={24} />
              <h1>Editar Pesquisa</h1>
            </div>
            <div className={styles.statusInfo}>
              <span className={`${styles.statusBadge} ${styles[status.color]}`}>
                <StatusIcon size={16} />
                {status.text}
              </span>
            </div>
          </div>
          <p className={styles.pesquisaTitle}>{pesquisa.titulo}</p>
        </div>
        
        <div className={styles.content}>
          {/* Formulário Principal */}
          <div className={styles.mainSection}>
            {/* Informações Básicas */}
            <div className={styles.formSection}>
              <div className={styles.sectionHeader}>
                <Edit3 size={20} />
                <h3>Informações Básicas</h3>
              </div>
              
              <div className={styles.formGroup}>
                <label htmlFor="titulo">Título da Pesquisa</label>
                <input
                  type="text"
                  id="titulo"
                  name="titulo"
                  value={formData.titulo}
                  onChange={handleInputChange}
                  placeholder="Digite o título da pesquisa"
                  className={styles.input}
                />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="descricao">Descrição</label>
                <textarea
                  id="descricao"
                  name="descricao"
                  value={formData.descricao}
                  onChange={handleInputChange}
                  placeholder="Digite uma descrição para a pesquisa"
                  className={styles.textarea}
                  rows={4}
                />
              </div>

              <div className={styles.formRow}>
                <div className={styles.checkboxGroup}>
                  <label className={styles.checkboxLabel}>
                    <input
                      type="checkbox"
                      name="ativa"
                      checked={formData.ativa}
                      onChange={handleInputChange}
                      className={styles.checkbox}
                    />
                    <span>Pesquisa Ativa</span>
                  </label>
                </div>

                <div className={styles.checkboxGroup}>
                  <label className={styles.checkboxLabel}>
                    <input
                      type="checkbox"
                      name="permiteRespostasAnonimas"
                      checked={formData.permiteRespostasAnonimas}
                      onChange={handleInputChange}
                      className={styles.checkbox}
                    />
                    <span>Permitir Respostas Anônimas</span>
                  </label>
                </div>
              </div>
            </div>

            {/* Configurações Avançadas */}
            <div className={styles.formSection}>
              <div className={styles.sectionHeader}>
                <Settings size={20} />
                <h3>Configurações Avançadas</h3>
              </div>
              
              <div className={styles.formGroup}>
                <label htmlFor="limiteRespostas">Limite de Respostas</label>
                <input
                  type="number"
                  id="limiteRespostas"
                  name="limiteRespostas"
                  value={formData.limiteRespostas || ''}
                  onChange={handleInputChange}
                  placeholder="Deixe vazio para ilimitado"
                  className={styles.input}
                  min="1"
                />
                <small className={styles.inputHint}>
                  Deixe vazio para permitir respostas ilimitadas
                </small>
              </div>

              <div className={styles.checkboxGroup}>
                <label className={styles.checkboxLabel}>
                  <input
                    type="checkbox"
                    name="temLimitadorTempo"
                    checked={formData.temLimitadorTempo}
                    onChange={handleInputChange}
                    className={styles.checkbox}
                  />
                  <span>Definir Data de Fechamento</span>
                </label>
              </div>

              {formData.temLimitadorTempo && (
                <div className={styles.formGroup}>
                  <label htmlFor="dataFechamento">Data de Fechamento</label>
                  <input
                    type="datetime-local"
                    id="dataFechamento"
                    name="dataFechamento"
                    value={formData.dataFechamento || ''}
                    onChange={handleInputChange}
                    className={styles.input}
                  />
                </div>
              )}
            </div>

            {/* Gráficos e Insights */}
            <div className={`${styles.formSection} ${styles.chartsSection}`}>
              <div className={styles.sectionHeader}>
                <div className={styles.sectionTitleWrapper}>
                  <BarChart3 size={20} />
                  <h3>Gráficos e Insights</h3>
                </div>
                <button
                  type="button"
                  className={styles.toggleChartsButton}
                  onClick={toggleGraficosPreview}
                >
                  {mostrarGraficosPreview ? 'Ocultar pré-visualização' : 'Mostrar pré-visualização'}
                </button>
              </div>

              {mostrarGraficosPreview ? (
                <div className={styles.chartsPreviewContainer}>
                  <ResultadosCompletos pesquisaId={id} modoCompacto />
                </div>
              ) : (
                <p className={styles.chartsPlaceholder}>
                  Ative a pré-visualização para carregar os gráficos desta pesquisa diretamente nesta tela.
                </p>
              )}

              <button
                type="button"
                className={styles.outlineButton}
                onClick={() => navigate(`/minhas-pesquisas/resultado/${id}`)}
              >
                <ArrowRight size={18} />
                Abrir página completa de gráficos
              </button>
            </div>

            {/* Ações do Formulário */}
            <div className={styles.actionsSection}>
              <button 
                className={styles.primaryButton}
                onClick={salvarAlteracoes}
                disabled={editando}
              >
                <Save size={18} />
                {editando ? 'Salvando...' : 'Salvar Alterações'}
              </button>
              
              <button 
                className={styles.secondaryButton}
                onClick={resetarFormulario}
              >
                <RotateCcw size={18} />
                Cancelar
              </button>
            </div>
          </div>

          {/* Sidebar */}
          <div className={styles.sidebar}>
            {/* Informações da Pesquisa */}
            <div className={styles.infoCard}>
              <div className={styles.cardHeader}>
                <FileText size={20} />
                <h3>Informações</h3>
              </div>
              <div className={styles.infoItem}>
                <Calendar size={16} />
                <span className={styles.infoLabel}>Criada em:</span>
                <span>{new Date(pesquisa.dataCriacao).toLocaleDateString('pt-BR')}</span>
              </div>
              <div className={styles.infoItem}>
                <FileText size={16} />
                <span className={styles.infoLabel}>Tipo:</span>
                <span>{pesquisa.tipoPesquisa?.descricao || 'Não definido'}</span>
              </div>
              <div className={styles.infoItem}>
                <Users size={16} />
                <span className={styles.infoLabel}>Interativa:</span>
                <span>{pesquisa.isInterativa ? 'Sim' : 'Não'}</span>
              </div>
            </div>

            {/* Ações Rápidas */}
            <div className={styles.actionsCard}>
              <div className={styles.cardHeader}>
                <Settings size={20} />
                <h3>Ações</h3>
              </div>
              
              <button 
                className={styles.actionButton}
                onClick={() => navigate(`/minhas-pesquisas/resultado/${id}`)}
              >
                <BarChart3 size={18} />
                Ver Resultados
              </button>

              <button 
                className={styles.actionButton}
                onClick={abrirModalRespostas}
              >
                <ListChecks size={18} />
                Ver Respostas
              </button>
              
              <button 
                className={styles.actionButton}
                onClick={() => navigate(`/responder/${id}`)}
              >
                <Eye size={18} />
                Testar Pesquisa
              </button>
              
              <button 
                className={styles.actionButton}
                onClick={() => navigate(`/criar?duplicar=${id}`)}
              >
                <Copy size={18} />
                Duplicar Pesquisa
              </button>
              
              <button 
                className={styles.actionButton}
                onClick={copiarLink}
              >
                <Share2 size={18} />
                Copiar Link
              </button>
              
              <button 
                className={`${styles.actionButton} ${styles.dangerButton}`}
                onClick={excluirPesquisa}
              >
                <Trash2 size={18} />
                Excluir Pesquisa
              </button>
            </div>
          </div>
        </div>
      </div>

      {mostrarRespostas && (
        <div className={styles.responsesOverlay} onMouseDown={fecharModalRespostas}>
          <div
            className={styles.responsesModal}
            role="dialog"
            aria-modal="true"
            onMouseDown={(e) => e.stopPropagation()}
          >
            <div className={styles.responsesHeader}>
              <div>
                <h3>Respostas da pesquisa</h3>
                <p>{respostas.length} recebidas</p>
              </div>
              <button type="button" className={styles.closeButton} onClick={fecharModalRespostas}>
                Fechar
              </button>
            </div>

            <div className={styles.responsesFilters}>
              <label htmlFor="filtro-usuario">Filtrar por usuário</label>
              <div className={styles.filtersRow}>
                <select
                  id="filtro-usuario"
                  value={filtroUsuario}
                  onChange={(e) => setFiltroUsuario(e.target.value)}
                >
                  <option value="todos">Todos os usuários</option>
                  {respostas.some((resposta) => !(resposta?.participanteId ?? resposta?.ParticipanteId)) && (
                    <option value="anon">Somente anônimas</option>
                  )}
                  {participantesOptions.map((opt) => (
                    <option key={opt.id} value={opt.id}>
                      {opt.label}
                    </option>
                  ))}
                </select>
                <button
                  type="button"
                  className={styles.refreshButton}
                  onClick={carregarRespostas}
                  disabled={carregandoRespostas}
                >
                  {carregandoRespostas ? 'Atualizando…' : 'Atualizar'}
                </button>
              </div>
            </div>

            <div className={styles.responsesContent}>
              {carregandoRespostas ? (
                <div className={styles.responsesLoading}>Carregando respostas…</div>
              ) : erroRespostas ? (
                <p className={styles.errorMessage}>{erroRespostas}</p>
              ) : respostasFiltradas.length === 0 ? (
                <p className={styles.emptyAnswer}>Nenhuma resposta encontrada para este filtro.</p>
              ) : (
                <div className={styles.responsesList}>
                  {respostasFiltradas.map((resposta) => {
                    const respostaId =
                      resposta?.respostaId ??
                      resposta?.RespostaId ??
                      `${resposta?.perguntaId ?? resposta?.PerguntaId}-${resposta?.dataResposta ?? resposta?.DataResposta}`;
                    const participanteId = resposta?.participanteId ?? resposta?.ParticipanteId;
                    const perguntaId = resposta?.perguntaId ?? resposta?.PerguntaId;
                    const perguntaDetalhe = perguntaId ? perguntasDetalhes[Number(perguntaId)] : null;
                    const perguntaTexto = perguntaDetalhe?.texto ?? (perguntaId ? `Pergunta #${perguntaId}` : 'Pergunta');
                    const dataResposta = resposta?.dataResposta ?? resposta?.DataResposta;
                    const ehAnonima = resposta?.respostaAnonima ?? resposta?.RespostaAnonima ?? false;

                    return (
                      <article key={respostaId} className={styles.responseItem}>
                        <div className={styles.responseMeta}>
                          <div>
                            <h4>{perguntaTexto}</h4>
                            <small>{formatarDataHora(dataResposta)}</small>
                          </div>
                          <div className={styles.responseUser}>
                            <span>{rotuloParticipante(participanteId)}</span>
                            {ehAnonima && <span className={styles.responseTag}>Anônima</span>}
                          </div>
                        </div>
                        <div className={styles.responseAnswer}>{renderRespostaConteudo(resposta)}</div>
                      </article>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default EditarPesquisa;