import React, { useState, useEffect } from 'react';
import { BarChart3, PieChart, TrendingUp, Users, FileText, Activity, Calendar, Download, Mail } from 'lucide-react';
import axios from 'axios';
import { toast } from 'react-toastify';
import styles from './AnalyticsDashboard.module.css';

const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5062';
const api = axios.create({ baseURL: API_BASE });
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

const AnalyticsDashboard = () => {
  const [analytics, setAnalytics] = useState(null);
  const [loading, setLoading] = useState(true);
  const [dateRange, setDateRange] = useState({ startDate: null, endDate: null });
  const [selectedPesquisa, setSelectedPesquisa] = useState(null);

  useEffect(() => {
    loadAnalytics();
  }, [dateRange]);

  const loadAnalytics = async () => {
    try {
      setLoading(true);
      const params = {};
      if (dateRange.startDate) params.startDate = dateRange.startDate.toISOString();
      if (dateRange.endDate) params.endDate = dateRange.endDate.toISOString();
      
      const response = await api.get('/api/analytics/dashboard', { params });
      setAnalytics(response.data);
    } catch (error) {
      console.error('Erro ao carregar analytics:', error);
      toast.error('Erro ao carregar dados do dashboard');
    } finally {
      setLoading(false);
    }
  };

  const exportRelatorio = async (pesquisaId, formato) => {
    try {
      const response = await api.get(`/api/analytics/exportar-${formato}/${pesquisaId}`, {
        responseType: 'blob'
      });
      
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `relatorio-pesquisa-${pesquisaId}.${formato}`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      
      toast.success(`Relatório ${formato.toUpperCase()} exportado com sucesso!`);
    } catch (error) {
      console.error('Erro ao exportar relatório:', error);
      toast.error('Erro ao exportar relatório');
    }
  };

  const agendarRelatorio = async (pesquisaId) => {
    try {
      const request = {
        PesquisaId: pesquisaId,
        tipoRelatorio: 'Relatório Completo',
        frequencia: 'semanal',
        emailDestino: 'usuario@exemplo.com',
        dataInicio: new Date().toISOString(),
        formato: 'PDF'
      };
      
      await api.post('/api/analytics/agendar-relatorio', request);
      toast.success('Relatório agendado com sucesso!');
    } catch (error) {
      console.error('Erro ao agendar relatório:', error);
      toast.error('Erro ao agendar relatório');
    }
  };

  if (loading) {
    return (
      <div className={styles.container}>
        <div className={styles.loading}>
          <Activity className={styles.loadingIcon} />
          <p>Carregando analytics...</p>
        </div>
      </div>
    );
  }

  if (!analytics) {
    return (
      <div className={styles.container}>
        <div className={styles.error}>
          <p>Erro ao carregar dados do dashboard</p>
          <button onClick={loadAnalytics} className={styles.retryBtn}>
            Tentar novamente
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.container}>
      <header className={styles.header}>
        <div className={styles.headerContent}>
          <h1>📊 Dashboard Analytics</h1>
          <p>Análise completa das suas pesquisas e métricas de performance</p>
        </div>
        
        <div className={styles.dateFilter}>
          <input
            type="date"
            value={dateRange.startDate?.toISOString().split('T')[0] || ''}
            onChange={(e) => setDateRange(prev => ({ ...prev, startDate: e.target.value ? new Date(e.target.value) : null }))}
            className={styles.dateInput}
          />
          <span>até</span>
          <input
            type="date"
            value={dateRange.endDate?.toISOString().split('T')[0] || ''}
            onChange={(e) => setDateRange(prev => ({ ...prev, endDate: e.target.value ? new Date(e.target.value) : null }))}
            className={styles.dateInput}
          />
        </div>
      </header>

      {/* Métricas Principais */}
      <section className={styles.metricsGrid}>
        <div className={styles.metricCard}>
          <div className={styles.metricIcon}>
            <FileText size={24} />
          </div>
          <div className={styles.metricContent}>
            <h3>{analytics.totalPesquisas}</h3>
            <p>Total de Pesquisas</p>
          </div>
        </div>

        <div className={styles.metricCard}>
          <div className={styles.metricIcon}>
            <Users size={24} />
          </div>
          <div className={styles.metricContent}>
            <h3>{analytics.totalRespostas}</h3>
            <p>Total de Respostas</p>
          </div>
        </div>

        <div className={styles.metricCard}>
          <div className={styles.metricIcon}>
            <Activity size={24} />
          </div>
          <div className={styles.metricContent}>
            <h3>{analytics.pesquisasAtivas}</h3>
            <p>Pesquisas Ativas</p>
          </div>
        </div>

        <div className={styles.metricCard}>
          <div className={styles.metricIcon}>
            <TrendingUp size={24} />
          </div>
          <div className={styles.metricContent}>
            <h3>{analytics.taxaRespostaMedia.toFixed(1)}%</h3>
            <p>Taxa de Resposta</p>
          </div>
        </div>
      </section>

      {/* Gráficos */}
      <section className={styles.chartsSection}>
        <div className={styles.chartContainer}>
          <h3>📈 Respostas por Dia (Últimos 30 dias)</h3>
          <div className={styles.chart}>
            {analytics.respostasPorDia.length > 0 ? (
              <div className={styles.barChart}>
                {analytics.respostasPorDia.map((item, index) => (
                  <div key={index} className={styles.barItem}>
                    <div 
                      className={styles.bar} 
                      style={{ height: `${Math.max(item.totalRespostas * 2, 20)}px` }}
                    >
                      <span className={styles.barValue}>{item.totalRespostas}</span>
                    </div>
                    <span className={styles.barLabel}>
                      {new Date(item.data).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })}
                    </span>
                  </div>
                ))}
              </div>
            ) : (
              <p className={styles.noData}>Nenhum dado disponível</p>
            )}
          </div>
        </div>

        <div className={styles.chartContainer}>
          <h3>🥧 Pesquisas por Tipo</h3>
          <div className={styles.chart}>
            {analytics.pesquisasPorTipo.length > 0 ? (
              <div className={styles.pieChart}>
                {analytics.pesquisasPorTipo.map((item, index) => (
                  <div key={index} className={styles.pieItem}>
                    <div className={styles.pieSlice} style={{ 
                      background: `hsl(${index * 60}, 70%, 60%)`,
                      transform: `rotate(${index * 45}deg)`
                    }}></div>
                    <div className={styles.pieLabel}>
                      <span className={styles.pieColor} style={{ background: `hsl(${index * 60}, 70%, 60%)` }}></span>
                      <span>{item.tipoPesquisa}</span>
                      <span className={styles.pieValue}>{item.quantidade}</span>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className={styles.noData}>Nenhum dado disponível</p>
            )}
          </div>
        </div>
      </section>

      {/* Performance Mensal */}
      <section className={styles.performanceSection}>
        <h3>📊 Performance Mensal</h3>
        <div className={styles.performanceGrid}>
          {analytics.performanceMensal.map((item, index) => (
            <div key={index} className={styles.performanceCard}>
              <h4>{item.mes}</h4>
              <div className={styles.performanceMetrics}>
                <div className={styles.performanceMetric}>
                  <span className={styles.metricLabel}>Criadas:</span>
                  <span className={styles.metricValue}>{item.pesquisasCriadas}</span>
                </div>
                <div className={styles.performanceMetric}>
                  <span className={styles.metricLabel}>Respostas:</span>
                  <span className={styles.metricValue}>{item.respostasRecebidas}</span>
                </div>
                <div className={styles.performanceMetric}>
                  <span className={styles.metricLabel}>Engajamento:</span>
                  <span className={styles.metricValue}>{(item.taxaEngajamento * 100).toFixed(1)}%</span>
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      {/* Pesquisas Recentes */}
      <section className={styles.recentSection}>
        <h3>🕒 Pesquisas Recentes</h3>
        <div className={styles.recentGrid}>
          {analytics.pesquisasRecentes.map((pesquisa) => (
            <div key={pesquisa.PesquisaId} className={styles.recentCard}>
              <div className={styles.recentHeader}>
                <h4>{pesquisa.titulo}</h4>
                <span className={`${styles.status} ${pesquisa.status === 'Ativa' ? styles.active : styles.inactive}`}>
                  {pesquisa.status}
                </span>
              </div>
              <div className={styles.recentDetails}>
                <p>Criada em: {new Date(pesquisa.DataCriacao).toLocaleDateString('pt-BR')}</p>
                <p>Respostas: {pesquisa.totalRespostas}</p>
              </div>
              <div className={styles.recentActions}>
                <button 
                  onClick={() => exportRelatorio(pesquisa.PesquisaId, 'pdf')}
                  className={styles.actionBtn}
                  title="Exportar PDF"
                >
                  <Download size={16} />
                </button>
                <button 
                  onClick={() => exportRelatorio(pesquisa.PesquisaId, 'excel')}
                  className={styles.actionBtn}
                  title="Exportar Excel"
                >
                  <BarChart3 size={16} />
                </button>
                <button 
                  onClick={() => agendarRelatorio(pesquisa.PesquisaId)}
                  className={styles.actionBtn}
                  title="Agendar Relatório"
                >
                  <Mail size={16} />
                </button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
};

export default AnalyticsDashboard;
