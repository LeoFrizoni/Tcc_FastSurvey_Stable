import React, { useEffect, useRef, useState } from 'react';
import { GOOGLE_CONFIG, isGoogleConfigured, getGoogleConfigError } from '../config/google';

const GoogleLoginButton = ({ 
  onSuccess, 
  onError, 
  clientId = GOOGLE_CONFIG.CLIENT_ID,
  buttonText = "Entrar com Google",
  theme = GOOGLE_CONFIG.BUTTON_CONFIG.theme,
  size = GOOGLE_CONFIG.BUTTON_CONFIG.size,
  type = GOOGLE_CONFIG.BUTTON_CONFIG.type,
  text = GOOGLE_CONFIG.BUTTON_CONFIG.text,
  shape = GOOGLE_CONFIG.BUTTON_CONFIG.shape,
  width = GOOGLE_CONFIG.BUTTON_CONFIG.width,
  logoAlignment = GOOGLE_CONFIG.BUTTON_CONFIG.logoAlignment
}) => {
  const [gisReady, setGisReady] = useState(false);
  const [buttonRendered, setButtonRendered] = useState(false);
  const buttonRef = useRef(null);

  // Carrega o script do Google
  useEffect(() => {
    if (!isGoogleConfigured()) {
      console.warn('Google Client ID não configurado corretamente');
      return;
    }

    const loadGoogleScript = () => {
      const existing = document.getElementById('google-gis');
      if (existing) {
        console.log('Google script already loaded');
        setGisReady(true);
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.id = 'google-gis';
      script.onload = () => {
        console.log('Google script loaded');
        setGisReady(true);
      };
      script.onerror = () => {
        console.error('Failed to load Google script');
        onError?.('Failed to load Google script');
      };
      document.head.appendChild(script);
    };

    loadGoogleScript();
  }, [clientId, onError]);

  // Renderiza o botão do Google
  const renderGoogleButton = () => {
    if (!gisReady || !isGoogleConfigured() || !window.google || !buttonRef.current || buttonRendered) {
      return;
    }

    try {
      console.log('Rendering Google button...');
      
      // Limpa o container
      buttonRef.current.innerHTML = '';
      
      // Inicializa o Google Identity Services
      window.google.accounts.id.initialize({
        client_id: clientId,
        callback: (response) => {
          console.log('Google login response:', response);
          onSuccess?.(response);
        },
        auto_select: false,
        cancel_on_tap_outside: true,
        // Adiciona configurações extras para debug
        prompt_parent_id: buttonRef.current.id || 'google-login-button',
        context: 'signin'
      });

      const options = {
        theme,
        size,
        type,
        text,
        shape,
        width,
        logo_alignment: logoAlignment,
      };

      // Renderiza o botão
      window.google.accounts.id.renderButton(buttonRef.current, options);
      console.log('Google button rendered successfully');
      setButtonRendered(true);
    } catch (error) {
      console.error('Error rendering Google button:', error);
      onError?.('Error rendering Google button');
    }
  };

  // Tenta renderizar o botão quando o script estiver pronto
  useEffect(() => {
    renderGoogleButton();
  }, [gisReady, clientId]);

  // Tenta renderizar novamente após um pequeno delay
  useEffect(() => {
    if (gisReady && !buttonRendered) {
      const timer = setTimeout(() => {
        renderGoogleButton();
      }, 200);

      return () => clearTimeout(timer);
    }
  }, [gisReady, buttonRendered]);

  if (!isGoogleConfigured()) {
    const configError = getGoogleConfigError();
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
      </div>
    );
  }

  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center',
      minHeight: '44px',
      padding: '8px',
      position: 'relative'
    }}>
      <div 
        ref={buttonRef}
        id="google-login-button"
        style={{
          minWidth: `${width}px`,
          minHeight: '44px',
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          borderRadius: '8px',
          background: 'transparent'
        }}
      />
      {!gisReady && (
        <div style={{ 
          position: 'absolute',
          fontSize: '12px',
          color: '#666',
          zIndex: 1
        }}>
          Carregando Google...
        </div>
      )}
      {gisReady && !buttonRendered && (
        <div style={{ 
          position: 'absolute',
          fontSize: '12px',
          color: '#666',
          zIndex: 1
        }}>
          Inicializando botão...
        </div>
      )}
    </div>
  );
};

export default GoogleLoginButton;
