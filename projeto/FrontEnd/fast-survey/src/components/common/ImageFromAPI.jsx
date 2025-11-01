import React, { useState, useEffect } from 'react';
import axios from '../../config/axios';

/**
 * Componente para exibir imagens que estão armazenadas no banco como Base64
 * via endpoint de download da API
 */
const ImageFromAPI = ({ 
  anexoId, 
  alt = "Imagem", 
  className = "", 
  style = {},
  onError = null,
  placeholder = null 
}) => {
  const [imageSrc, setImageSrc] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    if (!anexoId) {
      setLoading(false);
      setError(true);
      return;
    }

    const fetchImage = async () => {
      try {
        setLoading(true);
        setError(false);
        
        // Busca o arquivo via API de download
        const response = await axios.get(`/api/anexos/${anexoId}/download`, {
          responseType: 'blob' // Importante: receber como blob
        });
        
        // Criar URL temporária para o blob
        const imageBlob = new Blob([response.data], { 
          type: response.headers['content-type'] || 'image/jpeg' 
        });
        const imageUrl = URL.createObjectURL(imageBlob);
        
        setImageSrc(imageUrl);
        setLoading(false);
      } catch (err) {
        console.error(`Erro ao carregar imagem anexo ${anexoId}:`, err);
        setError(true);
        setLoading(false);
        onError?.(err);
      }
    };

    fetchImage();

    // Cleanup: liberar URL do blob quando componente desmontar
    return () => {
      if (imageSrc) {
        URL.revokeObjectURL(imageSrc);
      }
    };
  }, [anexoId]);

  if (loading) {
    return (
      <div 
        className={`image-loading ${className}`} 
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f5f5f5',
          minHeight: '100px',
          ...style
        }}
      >
        {placeholder || <span>Carregando imagem...</span>}
      </div>
    );
  }

  if (error || !imageSrc) {
    return (
      <div 
        className={`image-error ${className}`} 
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          backgroundColor: '#f8f8f8',
          border: '2px dashed #ddd',
          minHeight: '100px',
          color: '#666',
          ...style
        }}
      >
        <span>Imagem não disponível</span>
      </div>
    );
  }

  return (
    <img 
      src={imageSrc}
      alt={alt}
      className={className}
      style={style}
      onError={() => {
        setError(true);
        onError?.(new Error('Falha ao carregar imagem'));
      }}
    />
  );
};

export default ImageFromAPI;
