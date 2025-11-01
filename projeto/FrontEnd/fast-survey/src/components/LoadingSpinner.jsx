// src/components/LoadingSpinner.jsx
import React from 'react';
import '../styles/LoadingSpinner.css';

const LoadingSpinner = ({ size = 'medium', color = '#3b82f6' }) => {
  const sizeClass = {
    small: 'spinner-small',
    medium: 'spinner-medium',
    large: 'spinner-large'
  }[size];

  return (
    <div className="loading-container">
      <div 
        className={`loading-spinner ${sizeClass}`}
        style={{ borderTopColor: color }}
      >
      </div>
      <p className="loading-text">Carregando...</p>
    </div>
  );
};

export default LoadingSpinner;
