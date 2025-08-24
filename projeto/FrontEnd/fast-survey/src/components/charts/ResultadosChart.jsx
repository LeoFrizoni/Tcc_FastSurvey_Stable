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
import { Bar, Pie } from 'react-chartjs-2';

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

  const chartData = {
    labels: dados.map(item => item.label || item.texto || 'Opção'),
    datasets: [
      {
        label: 'Respostas',
        data: dados.map(item => item.value || item.total || 0),
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
        borderWidth: 1,
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top',
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
            const total = context.dataset.data.reduce((a, b) => a + b, 0);
            const percentage = ((context.parsed.y / total) * 100).toFixed(1);
            return `${context.parsed.y} respostas (${percentage}%)`;
          }
        }
      }
    },
    scales: tipo === 'bar' ? {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1,
        },
      },
    } : undefined,
  };

  return (
    <div style={{ 
      backgroundColor: 'white', 
      padding: '20px', 
      borderRadius: '12px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
      marginBottom: '20px'
    }}>
      {tipo === 'pie' ? (
        <Pie data={chartData} options={options} />
      ) : (
        <Bar data={chartData} options={options} />
      )}
    </div>
  );
};

export default ResultadosChart;
