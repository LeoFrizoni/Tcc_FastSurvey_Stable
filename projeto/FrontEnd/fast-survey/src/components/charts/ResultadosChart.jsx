import React from 'react';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
} from 'chart.js';
import { Bar, Pie, Doughnut } from 'react-chartjs-2';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
  ArcElement
);

const ResultadosChart = ({ dados, tipo = 'bar', titulo }) => {
  console.log('🎨 ResultadosChart - dados recebidos:', dados);
  console.log('🎨 ResultadosChart - tipo:', tipo);
  
  if (!dados || !dados.length) {
    return (
      <div style={{ 
        padding: '20px', 
        textAlign: 'center', 
        color: '#666',
        backgroundColor: '#f9f9f9',
        borderRadius: '8px',
        border: '1px dashed #ddd'
      }}>
        <p>Nenhum dado disponível para exibir no gráfico</p>
      </div>
    );
  }

  const labels = dados.map(item => item.label || item.texto || item.categoria || 'Opção');
  const valores = dados.map(item => Number(item.value ?? item.total ?? item.count ?? 0));
  const total = valores.reduce((acc, value) => acc + value, 0);

  const chartData = {
    labels,
    datasets: [
      {
        label: 'Respostas',
        data: valores,
        backgroundColor: dados.map((item, index) => 
          item.color || [
            '#7c3aed',
            '#6366f1',
            '#8b5cf6',
            '#a855f7',
            '#c084fc',
            '#d8b4fe',
            '#e9d5ff',
            '#f3e8ff',
          ][index % 8]
        ),
        borderColor: dados.map((item, index) => 
          item.borderColor || [
            '#6d28d9',
            '#5855eb',
            '#7c3aed',
            '#9333ea',
            '#a855f7',
            '#c084fc',
            '#d8b4fe',
            '#e9d5ff',
          ][index % 8]
        ),
        borderWidth: tipo === 'donut' ? 2 : 1,
      },
    ],
  };
  
  console.log('🎨 ResultadosChart - chartData preparado:', chartData);

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: tipo === 'bar' ? 'bottom' : 'right',
      },
      title: {
        display: !!titulo,
        text: titulo,
        font: {
          size: 16,
          weight: 'bold',
        },
      },
      tooltip: {
        callbacks: {
          label: function(context) {
            const parsedValue = typeof context.parsed === 'object'
              ? (context.parsed.y ?? context.parsed.x ?? 0)
              : context.parsed ?? 0;
            const percentage = total ? ((parsedValue / total) * 100).toFixed(1) : 0;
            return `${parsedValue} respostas (${percentage}%)`;
          }
        }
      }
    },
    cutout: tipo === 'donut' ? '60%' : undefined,
    scales: tipo === 'bar' ? {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1,
        },
      },
    } : undefined,
  };

  const breakdown = labels.map((label, index) => ({
    label,
    value: valores[index],
    percentage: total ? ((valores[index] / total) * 100).toFixed(1) : 0,
    color: chartData.datasets[0].backgroundColor[index],
  }));

  return (
    <div style={{ 
      backgroundColor: 'white', 
      padding: '20px', 
      borderRadius: '12px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
      marginBottom: '20px'
    }}>
      {tipo === 'pie' && <Pie data={chartData} options={options} />}
      {tipo === 'donut' && <Doughnut data={chartData} options={options} />}
      {tipo !== 'pie' && tipo !== 'donut' && <Bar data={chartData} options={options} />}

      {total > 0 && (
        <div
          style={{
            marginTop: 16,
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))',
            gap: 12,
          }}
        >
          {breakdown.map((item) => (
            <div
              key={item.label}
              style={{
                display: 'flex',
                flexDirection: 'column',
                border: '1px solid #e2e8f0',
                borderRadius: 10,
                padding: '10px 12px',
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <span
                  style={{
                    width: 10,
                    height: 10,
                    borderRadius: '50%',
                    backgroundColor: item.color,
                  }}
                />
                <strong style={{ fontSize: 13, color: '#0f172a' }}>{item.label}</strong>
              </div>
              <span style={{ fontSize: 24, fontWeight: 600, color: '#0f172a' }}>{item.percentage}%</span>
              <span style={{ fontSize: 12, color: '#64748b' }}>{item.value} respostas</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ResultadosChart;
