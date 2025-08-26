import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { BarChart3, PieChart, Download, RefreshCw } from 'lucide-react';
import ResultadosChart from './ResultadosChart';
import { toast } from 'react-toastify';
import { API_BASE_URL } from '../../config';

const ResultadosCompletos = ({ pesquisaId }) => {
  const [dadosGraficos, setDadosGraficos] = useState([]);
  const [loading, setLoading] = useState(true);
  const [tipoVisualizacao, setTipoVisualizacao] = useState('bar'); // 'bar' ou 'pie'

  useEffect(() => {
    if (pesquisaId) {
      carregarGraficos();
    }
  }, [pesquisaId, carregarGraficos]);

  const carregarGraficos = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get(
        `${API_BASE_URL}/api/resultados/graficos/${pesquisaId}`,
        {
          headers: { Authorization: `Bearer ${token}` }
        }
      );

      if (response.data && response.data.data && response.data.data.graficos) {
        setDadosGraficos(response.data.data.graficos);
      } else if (response.data && response.data.graficos) {
        setDadosGraficos(response.data.graficos);
      } else {
        setDadosGraficos([]);
      }
    } catch (error) {
      console.error('Erro ao carregar gráficos:', error);
      toast.error('Erro ao carregar os gráficos de resultados');
      setDadosGraficos([]);
    } finally {
      setLoading(false);
    }
  };

  const exportarResultados = async (formato = 'pdf') => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.post(
        `${API_BASE_URL}/api/resultados/exportar`,
        {
          PesquisaId: parseInt(pesquisaId),
          formato: formato,
          incluirGraficos: true
        },
        {
          headers: { Authorization: `Bearer ${token}` },
          responseType: 'blob'
        }
      );

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `resultados_pesquisa_${pesquisaId}.${formato}`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);

      toast.success(`Resultados exportados em ${formato.toUpperCase()}`);
    } catch (error) {
      console.error('Erro ao exportar resultados:', error);
      toast.error('Erro ao exportar resultados');
    }
  };

  if (loading) {
    return (
      <div style={{ 
        padding: '40px', 
        textAlign: 'center',
        backgroundColor: 'white',
        borderRadius: '12px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <RefreshCw size={24} style={{ animation: 'spin 1s linear infinite' }} />
        <p style={{ marginTop: '10px', color: '#666' }}>Carregando gráficos...</p>
        <style>{`
          @keyframes spin {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
          }
        `}</style>
      </div>
    );
  }

  if (!dadosGraficos || dadosGraficos.length === 0) {
    return (
      <div style={{ 
        padding: '40px', 
        textAlign: 'center',
        backgroundColor: 'white',
        borderRadius: '12px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <BarChart3 size={48} style={{ color: '#ccc', marginBottom: '10px' }} />
        <h3 style={{ color: '#666', marginBottom: '10px' }}>Nenhum resultado disponível</h3>
        <p style={{ color: '#999' }}>
          Esta pesquisa ainda não possui respostas para gerar gráficos.
        </p>
      </div>
    );
  }

  return (
    <div style={{ padding: '20px' }}>
      {/* Header com controles */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        marginBottom: '20px',
        padding: '16px',
        backgroundColor: 'white',
        borderRadius: '12px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <div>
          <h2 style={{ margin: 0, color: '#333' }}>Gráficos de Resultados</h2>
          <p style={{ margin: '5px 0 0 0', color: '#666', fontSize: '14px' }}>
            {dadosGraficos.length} pergunta{dadosGraficos.length !== 1 ? 's' : ''} com dados
          </p>
        </div>

        <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
          {/* Toggle de visualização */}
          <div style={{ display: 'flex', backgroundColor: '#f3f4f6', borderRadius: '8px', padding: '4px' }}>
            <button
              onClick={() => setTipoVisualizacao('bar')}
              style={{
                padding: '8px 12px',
                border: 'none',
                borderRadius: '6px',
                background: tipoVisualizacao === 'bar' ? '#7c3aed' : 'transparent',
                color: tipoVisualizacao === 'bar' ? 'white' : '#666',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: '6px'
              }}
            >
              <BarChart3 size={16} />
              Barras
            </button>
            <button
              onClick={() => setTipoVisualizacao('pie')}
              style={{
                padding: '8px 12px',
                border: 'none',
                borderRadius: '6px',
                background: tipoVisualizacao === 'pie' ? '#7c3aed' : 'transparent',
                color: tipoVisualizacao === 'pie' ? 'white' : '#666',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: '6px'
              }}
            >
              <PieChart size={16} />
              Pizza
            </button>
          </div>

          {/* Botões de ação */}
          <button
            onClick={carregarGraficos}
            style={{
              padding: '8px 12px',
              border: '1px solid #ddd',
              borderRadius: '8px',
              background: 'white',
              color: '#666',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              gap: '6px'
            }}
          >
            <RefreshCw size={16} />
            Atualizar
          </button>

          <div style={{ display: 'flex', gap: '5px' }}>
            <button
              onClick={() => exportarResultados('pdf')}
              style={{
                padding: '8px 12px',
                border: 'none',
                borderRadius: '8px',
                background: '#7c3aed',
                color: 'white',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
                fontSize: '14px'
              }}
            >
              <Download size={16} />
              PDF
            </button>
            <button
              onClick={() => exportarResultados('excel')}
              style={{
                padding: '8px 12px',
                border: 'none',
                borderRadius: '8px',
                background: '#059669',
                color: 'white',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: '6px',
                fontSize: '14px'
              }}
            >
              <Download size={16} />
              Excel
            </button>
          </div>
        </div>
      </div>

      {/* Gráficos */}
      <div style={{ display: 'grid', gap: '20px' }}>
        {dadosGraficos.map((grafico, index) => (
          <div key={index}>
            <ResultadosChart
              dados={grafico.dados || grafico.opcoes || []}
              tipo={grafico.tipo || tipoVisualizacao}
              titulo={(grafico.Titulo ?? grafico.titulo) || grafico.texto || `Pergunta ${index + 1}`}
            />
          </div>
        ))}
      </div>
    </div>
  );
};

export default ResultadosCompletos;
