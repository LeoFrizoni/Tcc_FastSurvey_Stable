import React, { useState, useEffect } from 'react';
import { 
  BarChart3, 
  PieChart, 
  TrendingUp, 
  Users, 
  FileText, 
  Activity, 
  Calendar, 
  Download, 
  Mail,
  Eye,
  Share2,
  Settings,
  RefreshCw,
  AlertCircle
} from 'lucide-react';
import axios from 'axios';
import { toast } from 'react-toastify';
import TopNavbar from '../../components/layouts/TopNavBar';
import styles from './AnalyticsDashboard.module.css';

import env from '../../config/env';
const API_BASE = env.REACT_APP_API_URL;
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

  const [error, setError] = useState('');

  useEffect(() => {
    loadAnalytics();
  }, [dateRange, loadAnalytics]);

  const loadAnalytics = async () => {
    try {
      setLoading(true);
      setError('');
      const params = {};
      if (dateRange.startDate) params.startDate = dateRange.startDate.toISOString();
      if (dateRange.endDate) params.endDate = dateRange.endDate.toISOString();
      
      const response = await api.get('/api/analytics/dashboard', { params });
      setAnalytics(response.data);
    } catch (error) {
      console.error('Erro ao carregar analytics:', error);
      setError('Erro ao carregar dados do dashboard');
      toast.error('Erro ao carregar dados do dashboard');
    } finally {
      setLoading(false);
    }
  };





  if (loading) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.loading}>
            <Activity className={styles.loadingIcon} />
            <p>Carregando analytics...</p>
          </div>
        </div>
      </>
    );
  }

  if (error || !analytics) {
    return (
      <>
        <TopNavbar />
        <div className={styles.container}>
          <div className={styles.error}>
            <AlertCircle size={48} />
            <h2>Erro ao carregar dados</h2>
            <p>{error || 'Não foi possível carregar os dados do dashboard'}</p>
            <button onClick={loadAnalytics} className={styles.retryBtn}>
              <RefreshCw size={16} />
              Tentar novamente
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
            <h1>Dashboard Analytics</h1>
            <p>Visualize estatísticas e insights das suas pesquisas</p>
          </div>
          
          <div className={styles.dateFilter}>
            <Calendar size={20} />
            <input
              type="date"
              className={styles.dateInput}
              value={dateRange.startDate || ''}
              onChange={(e) => setDateRange(prev => ({ ...prev, startDate: e.target.value }))}
              placeholder="Data inicial"
            />
            <span>até</span>
            <input
              type="date"
              className={styles.dateInput}
              value={dateRange.endDate || ''}
              onChange={(e) => setDateRange(prev => ({ ...prev, endDate: e.target.value }))}
              placeholder="Data final"
            />
          </div>
        </div>

        {/* Métricas Principais */}
        <div className={styles.metricsGrid}>
          <div className={styles.metricCard}>
            <div className={styles.metricIcon}>
              <FileText size={24} />
            </div>
            <div className={styles.metricContent}>
              <h3>{analytics.totalPesquisas || 0}</h3>
              <p>Total de Pesquisas</p>
            </div>
          </div>

          <div className={styles.metricCard}>
            <div className={styles.metricIcon}>
              <Users size={24} />
            </div>
            <div className={styles.metricContent}>
              <h3>{analytics.totalRespostas || 0}</h3>
              <p>Total de Respostas</p>
            </div>
          </div>

          <div className={styles.metricCard}>
            <div className={styles.metricIcon}>
              <Activity size={24} />
            </div>
            <div className={styles.metricContent}>
              <h3>{analytics.pesquisasAtivas || 0}</h3>
              <p>Pesquisas Ativas</p>
            </div>
          </div>

          <div className={styles.metricCard}>
            <div className={styles.metricIcon}>
              <TrendingUp size={24} />
            </div>
            <div className={styles.metricContent}>
              <h3>{analytics.pesquisasInterativas || 0}</h3>
              <p>Pesquisas Interativas</p>
            </div>
          </div>
        </div>

        {/* Gráficos e Estatísticas */}
        <div className={styles.chartsSection}>
          <div className={styles.chartCard}>
            <div className={styles.chartHeader}>
              <h3><BarChart3 size={20} /> Pesquisas Populares</h3>
              <div className={styles.chartActions}>
                <button className={styles.chartActionBtn}>
                  <Download size={16} />
                  Exportar
                </button>
              </div>
            </div>
            <div className={styles.chartContent}>
              {analytics.pesquisasPopulares && analytics.pesquisasPopulares.length > 0 ? (
                <div className={styles.popularList}>
                  {analytics.pesquisasPopulares.slice(0, 5).map((pesquisa, index) => (
                    <div key={pesquisa.pesquisaId} className={styles.popularItem}>
                      <div className={styles.popularRank}>#{index + 1}</div>
                      <div className={styles.popularInfo}>
                        <h4>{pesquisa.titulo}</h4>
                        <p>{pesquisa.totalRespostas} respostas</p>
                      </div>
                      <div className={styles.popularActions}>
                        <button className={styles.actionBtn} title="Ver detalhes">
                          <Eye size={16} />
                        </button>
                        <button className={styles.actionBtn} title="Compartilhar">
                          <Share2 size={16} />
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className={styles.emptyState}>
                  <BarChart3 size={48} />
                  <p>Nenhuma pesquisa popular encontrada</p>
                </div>
              )}
            </div>
          </div>

          <div className={styles.chartCard}>
            <div className={styles.chartHeader}>
              <h3><PieChart size={20} /> Estatísticas Mensais</h3>
              <div className={styles.chartActions}>
                <button className={styles.chartActionBtn}>
                  <Settings size={16} />
                  Configurar
                </button>
              </div>
            </div>
            <div className={styles.chartContent}>
              {analytics.estatisticasMensais && analytics.estatisticasMensais.length > 0 ? (
                <div className={styles.monthlyStats}>
                  {analytics.estatisticasMensais.slice(0, 6).map((stat, index) => (
                    <div key={index} className={styles.monthlyItem}>
                      <div className={styles.monthlyBar}>
                        <div 
                          className={styles.monthlyBarFill}
                          style={{ height: `${(stat.totalRespostas / Math.max(...analytics.estatisticasMensais.map(s => s.totalRespostas))) * 100}%` }}
                        ></div>
                      </div>
                      <span className={styles.monthlyLabel}>{stat.mes}</span>
                      <span className={styles.monthlyValue}>{stat.totalRespostas}</span>
                    </div>
                  ))}
                </div>
              ) : (
                <div className={styles.emptyState}>
                  <PieChart size={48} />
                  <p>Nenhuma estatística mensal disponível</p>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Ações Rápidas */}
        <div className={styles.quickActions}>
          <h3>Ações Rápidas</h3>
          <div className={styles.actionsGrid}>
            <button className={styles.quickActionBtn}>
              <FileText size={20} />
              <span>Criar Nova Pesquisa</span>
            </button>
            <button className={styles.quickActionBtn}>
              <Download size={20} />
              <span>Exportar Relatório</span>
            </button>
            <button className={styles.quickActionBtn}>
              <Mail size={20} />
              <span>Agendar Relatório</span>
            </button>
            <button className={styles.quickActionBtn}>
              <Settings size={20} />
              <span>Configurações</span>
            </button>
          </div>
        </div>
      </div>
    </>
  );
};

export default AnalyticsDashboard;
