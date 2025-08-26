import React from 'react';
import './LoadingSpinner.module.css';

const LoadingSpinner = ({ size = 'medium', color = 'primary', text = 'Carregando...' }) => {
  const sizeClasses = {
    small: 'spinner-small',
    medium: 'spinner-medium',
    large: 'spinner-large'
  };

  const colorClasses = {
    primary: 'spinner-primary',
    secondary: 'spinner-secondary',
    white: 'spinner-white'
  };

  return (
    <div className="loading-container">
      <div className={`spinner ${sizeClasses[size]} ${colorClasses[color]}`}></div>
      {text && <p className="loading-text">{text}</p>}
    </div>
  );
};

export default LoadingSpinner;
