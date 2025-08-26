import React from 'react';
import { getGoogleConfigError } from '../config/google';

const LoginFallback = ({ showInstructions = true }) => {
  const configError = getGoogleConfigError();

  if (!showInstructions) {
    return (
      <div style={{ 
        padding: '12px', 
        border: '1px solid #ddd', 
        borderRadius: '8px', 
        background: '#f9f9f9',
        textAlign: 'center',
        color: '#666',
        fontSize: '12px'
      }}>
        Login com Google temporariamente indisponível
      </div>
    );
  }

  return (
    <div style={{ 
      padding: '16px', 
      border: '1px solid #ff6b6b', 
      borderRadius: '8px', 
      background: '#fff5f5',
      textAlign: 'center',
      color: '#d63031',
      fontSize: '14px',
      maxWidth: '400px',
      margin: '0 auto'
    }}>
      <div style={{ fontWeight: 'bold', marginBottom: '8px' }}>
        {configError.title}
      </div>
      <div style={{ marginBottom: '12px', fontSize: '12px' }}>
        {configError.message}
      </div>
      <div style={{ textAlign: 'left', fontSize: '11px' }}>
        <strong>Instruções:</strong>
        <ul style={{ margin: '8px 0', paddingLeft: '20px' }}>
          {configError.instructions.map((instruction, index) => (
            <li key={index}>{instruction}</li>
          ))}
        </ul>
      </div>
      <div style={{ 
        marginTop: '12px', 
        padding: '8px', 
        background: '#e8f5e8', 
        border: '1px solid #4caf50',
        borderRadius: '4px',
        fontSize: '11px',
        color: '#2e7d32'
      }}>
        💡 <strong>Dica:</strong> Você pode usar o login tradicional enquanto configura o Google OAuth
      </div>
    </div>
  );
};

export default LoginFallback;
