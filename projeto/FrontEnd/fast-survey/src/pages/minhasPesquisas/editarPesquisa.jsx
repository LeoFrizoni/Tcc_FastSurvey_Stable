import React, { useState, useEffect, useCallback } from 'react';
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
  XCircle
} from 'lucide-react';
import axios from 'axios';
import TopNavbar from '../../components/layouts/TopNavBar';
import styles from './editarPesquisa.module.css';
import { toast } from 'react-toastify';

// Configuração da API
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5062';

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
    </>
  );
};

export default EditarPesquisa;