import React, { useEffect, useRef, useState } from 'react';

const GOOGLE_CLIENT_ID = process.env.REACT_APP_GOOGLE_CLIENT_ID || '';

const GoogleLoginButton = ({ 
  onSuccess, 
  onError, 
  clientId = GOOGLE_CLIENT_ID,
  buttonText = "Entrar com Google",
  theme = "outline",
  size = "large",
  type = "standard",
  text = "signin_with",
  shape = "pill",
  width = 280,
  logoAlignment = "left"
}) => {
  const [gisReady, setGisReady] = useState(false);
  const [buttonRendered, setButtonRendered] = useState(false);
  const buttonRef = useRef(null);

  // Carrega o script do Google
  useEffect(() => {
    if (!clientId) {
      console.log('No Google Client ID configured');
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
    if (!gisReady || !clientId || !window.google || !buttonRef.current || buttonRendered) {
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

  if (!clientId) {
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
        Google Client ID não configurado
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
