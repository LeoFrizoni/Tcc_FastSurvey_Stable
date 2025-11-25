import React, { useState, useEffect, useCallback } from 'react';
import api from '../../lib/api';
import { BarChart3, PieChart, RefreshCw, Sparkles, Clock } from 'lucide-react';
import ResultadosChart from './ResultadosChart';
import { toast } from 'react-toastify';
import { API_BASE_URL } from '../../config';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '../ui/card';
import { Tabs, TabsList, TabsTrigger } from '../ui/tabs';
import { Badge } from '../ui/badge';
import { Separator } from '../ui/separator';

const chartTabs = [
  { id: 'auto', label: 'Automático', icon: Sparkles },
  { id: 'bar', label: 'Barras', icon: BarChart3 },
  { id: 'pie', label: 'Pizza', icon: PieChart },
];

const sentimentPalette = {
  positive: '#16a34a',
  neutral: '#f97316',
  negative: '#dc2626',
};

const tipoPerguntaLabel = (tipoId) => {
  const map = {
    1: 'Discursiva',
    2: 'Única escolha',
    3: 'Múltipla escolha',
  };
  return map[tipoId] || 'Pergunta';
};

const formatarData = (valor) => {
  if (!valor) return '—';
  const data = new Date(valor);
  if (Number.isNaN(data.getTime())) return '—';
  return data.toLocaleString('pt-BR');
};

const ResultadosCompletos = ({ pesquisaId, modoCompacto = false }) => {
  const [dadosGraficos, setDadosGraficos] = useState([]);
  const [sentimentInsights, setSentimentInsights] = useState(null);
  const [payloadMeta, setPayloadMeta] = useState(null);
  const [loading, setLoading] = useState(true);
  const [visualizacao, setVisualizacao] = useState('auto');
  const outerPadding = modoCompacto ? '0' : '20px';

  const carregarGraficos = useCallback(async () => {
    setLoading(true);
    try {
      const response = await api.get(
        `${API_BASE_URL}/api/resultados/graficos/${pesquisaId}`,
      );

      const payload = response.data?.data ?? response.data ?? {};
      console.log('📊 Payload completo recebido:', payload);
      console.log('📊 Graficos:', payload.graficos);
      console.log('📊 SentimentInsights:', payload.sentimentInsights);
      
      setDadosGraficos(payload.graficos ?? []);
      setSentimentInsights(payload.sentimentInsights ?? null);
      setPayloadMeta(payload.meta ?? null);
    } catch (error) {
      console.error('Erro ao carregar gráficos:', error);
      toast.error('Erro ao carregar os gráficos de resultados');
      setDadosGraficos([]);
      setSentimentInsights(null);
      setPayloadMeta(null);
    } finally {
      setLoading(false);
    }
  }, [pesquisaId]);

  useEffect(() => {
    if (pesquisaId) carregarGraficos();
  }, [pesquisaId, carregarGraficos]);

  const extrairSerie = useCallback((grafico) => {
    if (!grafico) return [];
    if (Array.isArray(grafico.dados)) return grafico.dados;
    if (Array.isArray(grafico.opcoes)) return grafico.opcoes;
    return [];
  }, []);

  const possuiValores = useCallback((serie) => (
    serie.some((item) => Number(item?.value ?? item?.total ?? item?.count ?? 0) > 0)
  ), []);

  if (loading) {
    return (
      <Card style={{ padding: '32px', margin: '0 auto' }}>
        <CardContent style={{ alignItems: 'center', textAlign: 'center' }}>
          <RefreshCw size={28} style={{ animation: 'spin 1s linear infinite' }} />
          <p style={{ marginTop: '8px', color: '#475569' }}>Carregando gráficos...</p>
          <style>{`
            @keyframes spin {
              from { transform: rotate(0deg); }
              to { transform: rotate(360deg); }
            }
          `}</style>
        </CardContent>
      </Card>
    );
  }

  if ((!dadosGraficos || dadosGraficos.length === 0) && !sentimentInsights) {
    return (
      <Card style={{ padding: modoCompacto ? '24px' : '40px', textAlign: 'center' }}>
        <CardContent>
          <BarChart3 size={48} color="#cbd5f5" />
          <CardTitle style={{ marginTop: '12px' }}>Nenhum resultado disponível</CardTitle>
          <CardDescription>
            Assim que respostas forem registradas, esta aba exibirá gráficos e insights.
          </CardDescription>
        </CardContent>
      </Card>
    );
  }

  const perguntasComDados = (dadosGraficos || []).filter((grafico) => {
    if (!grafico) return false;
    if (Number(grafico.tipo) === 1) return false; // discursiva
    return possuiValores(extrairSerie(grafico));
  });

  const totalPerguntas = perguntasComDados.length;
  const ultimaAtualizacao = payloadMeta?.atualizadoEm ? formatarData(payloadMeta.atualizadoEm) : null;
  const resolvedVisualization = (graficoTipo, tipoPergunta) => {
    if (visualizacao !== 'auto') return visualizacao;
    if (graficoTipo) return graficoTipo;
    if (tipoPergunta === 3) return 'donut';
    if (tipoPergunta === 2) return 'pie';
    return 'bar';
  };

  const respostasConsolidadas = payloadMeta?.totalRespostas
    ?? perguntasComDados.reduce((sum, grafico) => sum + (grafico.totalRespostas || 0), 0);
  const participantes = payloadMeta?.totalParticipantes
    ?? payloadMeta?.participantes
    ?? payloadMeta?.respondentes
    ?? '-';
  const perguntasIgnoradas = (dadosGraficos?.length || 0) - perguntasComDados.length;

  return (
    <div style={{ padding: outerPadding }}>
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
          gap: '16px',
          marginBottom: '20px',
        }}
      >
        {[{
          label: 'Respostas consolidadas',
          value: respostasConsolidadas,
          helper: 'Total em todas as perguntas',
        }, {
          label: 'Participantes únicos',
          value: participantes,
          helper: 'Com pelo menos 1 resposta',
        }, {
          label: 'Perguntas com gráficos',
          value: totalPerguntas,
          helper: perguntasIgnoradas > 0 ? `${perguntasIgnoradas} ocultadas (sem dados)` : 'Todas com dados',
        }].map((metric) => (
          <Card key={metric.label}>
            <CardContent style={{ padding: '18px 20px' }}>
              <p style={{ margin: 0, color: '#64748b', fontSize: 13 }}>{metric.label}</p>
              <strong style={{ fontSize: 28, color: '#0f172a' }}>{metric.value ?? '—'}</strong>
              <p style={{ margin: 0, color: '#94a3b8', fontSize: 12 }}>{metric.helper}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {sentimentInsights && (
        <Card style={{ marginBottom: '20px' }}>
          <CardHeader>
            <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '12px' }}>
              <div>
                <CardTitle>Insights de Sentimento</CardTitle>
                <CardDescription>
                  {sentimentInsights.total || 0} respostas discursivas analisadas pela IA
                </CardDescription>
              </div>
              {ultimaAtualizacao && (
                <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px', color: '#475569' }}>
                  <Clock size={16} />
                  Atualizado {ultimaAtualizacao}
                </span>
              )}
            </div>
          </CardHeader>
          <CardContent>
            <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
              {['positive', 'neutral', 'negative'].map((key) => {
                const keyMap = {
                  positive: ['positive', 'positivo'],
                  neutral: ['neutral', 'neutro'],
                  negative: ['negative', 'negativo'],
                };
                const labelMap = {
                  positive: 'Positivas',
                  neutral: 'Neutras',
                  negative: 'Negativas',
                };
                
                // Tenta múltiplas chaves possíveis
                let total = 0;
                if (sentimentInsights.sentimentos) {
                  for (const possibleKey of keyMap[key]) {
                    if (sentimentInsights.sentimentos[possibleKey]) {
                      total = sentimentInsights.sentimentos[possibleKey];
                      break;
                    }
                  }
                }
                
                const percentual = sentimentInsights.total
                  ? Math.round((total / sentimentInsights.total) * 100)
                  : 0;
                return (
                  <div
                    key={key}
                    style={{
                      flex: 1,
                      minWidth: 160,
                      border: '1px solid rgba(15, 23, 42, 0.08)',
                      borderRadius: 14,
                      padding: '16px',
                      background: 'linear-gradient(120deg, #fff, #f8fafc)',
                    }}
                  >
                    <p style={{ margin: 0, color: '#475569', fontSize: 13 }}>{labelMap[key]}</p>
                    <strong style={{ fontSize: 26, color: sentimentPalette[key] }}>{total}</strong>
                    <p style={{ margin: 0, color: '#94a3b8', fontSize: 12 }}>{percentual}% das falas</p>
                  </div>
                );
              })}
            </div>

            <Separator />

            <div style={{ display: 'flex', flexWrap: 'wrap', gap: '24px' }}>
              <div style={{ flex: 1, minWidth: 220 }}>
                <h4 style={{ margin: '0 0 10px', color: '#334155' }}>Palavras-chave</h4>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                  {(sentimentInsights.palavrasChave || []).map((item) => (
                    <Badge key={item.termo}>
                      {item.termo}
                      <span style={{ marginLeft: 6, fontSize: 12 }}>×{item.peso}</span>
                    </Badge>
                  ))}
                  {(!sentimentInsights.palavrasChave || sentimentInsights.palavrasChave.length === 0) && (
                    <CardDescription>Sem termos relevantes ainda.</CardDescription>
                  )}
                </div>
              </div>
              <div style={{ flex: 1, minWidth: 260 }}>
                <h4 style={{ margin: '0 0 10px', color: '#334155' }}>Falas recentes</h4>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                  {(sentimentInsights.exemplos || []).map((item) => (
                    <div key={item.respostaId} style={{ border: '1px solid rgba(15, 23, 42, 0.08)', borderRadius: 12, padding: '12px' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                        <strong style={{ color: '#0f172a', fontSize: 14 }}>{item.categoria}</strong>
                        <Badge variant="muted">{item.sentimento}</Badge>
                      </div>
                      <p style={{ margin: 0, color: '#475569' }}>
                        “{item.texto?.slice(0, 160)}{item.texto && item.texto.length > 160 ? '…' : ''}”
                      </p>
                    </div>
                  ))}
                  {(!sentimentInsights.exemplos || sentimentInsights.exemplos.length === 0) && (
                    <CardDescription>Recolha mais respostas para liberar exemplos.</CardDescription>
                  )}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Card style={{ marginBottom: '20px' }}>
        <CardHeader>
          <CardTitle>Gráficos de Resultados</CardTitle>
          <CardDescription>
            {totalPerguntas} pergunta{totalPerguntas !== 1 ? 's' : ''} com dados consolidados
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div
            style={{
              display: 'flex',
              flexWrap: 'wrap',
              alignItems: 'center',
              justifyContent: 'space-between',
              gap: '12px',
            }}
          >
            <Tabs defaultValue="auto" value={visualizacao} onValueChange={setVisualizacao}>
              <TabsList>
                {chartTabs.map(({ id, label, icon: Icon }) => (
                  <TabsTrigger key={id} value={id}>
                    <span style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
                      <Icon size={16} />
                      {label}
                    </span>
                  </TabsTrigger>
                ))}
              </TabsList>
            </Tabs>

            <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
              <button
                type="button"
                onClick={carregarGraficos}
                style={{
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: '6px',
                  borderRadius: 10,
                  border: '1px solid rgba(15,23,42,0.1)',
                  padding: '8px 14px',
                  background: '#fff',
                  cursor: 'pointer',
                }}
              >
                <RefreshCw size={16} /> Atualizar
              </button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Gráficos de Analytics de Sentimento */}
      {sentimentInsights && sentimentInsights.total > 0 && (
        <div style={{ display: 'grid', gap: '20px', marginBottom: '20px' }}>
          <Card>
            <CardHeader>
              <CardTitle>Classificação das respotas</CardTitle>
              <CardDescription>Análise automática de {sentimentInsights.total} respostas discursivas</CardDescription>
            </CardHeader>
            <CardContent>
              <ResultadosChart
                dados={[
                  { 
                    label: 'Positivas', 
                    value: sentimentInsights.sentimentos?.positive || sentimentInsights.sentimentos?.positivo || 0, 
                    color: '#16a34a' 
                  },
                  { 
                    label: 'Neutras', 
                    value: sentimentInsights.sentimentos?.neutral || sentimentInsights.sentimentos?.neutro || 0, 
                    color: '#f97316' 
                  },
                  { 
                    label: 'Negativas', 
                    value: sentimentInsights.sentimentos?.negative || sentimentInsights.sentimentos?.negativo || 0, 
                    color: '#dc2626' 
                  },
                ].filter(item => item.value > 0)}
                tipo={visualizacao === 'auto' ? 'pie' : visualizacao}
                titulo=""
              />
            </CardContent>
          </Card>

          {sentimentInsights.palavrasChave && sentimentInsights.palavrasChave.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>Palavras-chave Mais Mencionadas</CardTitle>
                <CardDescription>Termos extraídos das respostas com maior relevância</CardDescription>
              </CardHeader>
              <CardContent>
                <ResultadosChart
                  dados={sentimentInsights.palavrasChave.slice(0, 10).map(item => ({
                    label: item.termo,
                    value: item.peso,
                    color: '#7c3aed'
                  }))}
                  tipo="bar"
                  titulo=""
                />
              </CardContent>
            </Card>
          )}
        </div>
      )}

      {/* Gráficos das Perguntas */}
      <div
        style={{
          display: 'grid',
          gap: '20px',
          gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))',
        }}
      >
        {perguntasComDados.map((grafico, index) => {
          const chartType = resolvedVisualization(grafico.chartType, grafico.tipo);
          const titulo = grafico.texto || grafico.titulo || `Pergunta ${index + 1}`;
          return (
            <Card key={grafico.perguntaId ?? index}>
              <CardHeader>
                <div style={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: '12px' }}>
                  <div>
                    <CardTitle style={{ marginBottom: 4 }}>{titulo}</CardTitle>
                    <CardDescription>
                      {grafico.totalRespostas || 0} resposta{(grafico.totalRespostas || 0) === 1 ? '' : 's'} registradas
                    </CardDescription>
                  </div>
                  <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                    <Badge>{tipoPerguntaLabel(grafico.tipo)}</Badge>
                    <Badge variant="muted">
                      {chartType === 'donut' ? 'Donut' : chartType === 'pie' ? 'Pizza' : 'Barras'}
                    </Badge>
                  </div>
                </div>
              </CardHeader>
              <CardContent>
                <ResultadosChart
                  dados={extrairSerie(grafico)}
                  tipo={chartType}
                  titulo=""
                />
              </CardContent>
            </Card>
          );
        })}
        {perguntasComDados.length === 0 && (
          <Card>
            <CardContent style={{ padding: '32px', textAlign: 'center' }}>
              <CardTitle>Nenhum gráfico disponível</CardTitle>
              <CardDescription>
                Apenas perguntas objetivas ou com respostas registradas aparecem aqui. Continue coletando dados
                para liberar os gráficos automáticos.
              </CardDescription>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
};

export default ResultadosCompletos;
