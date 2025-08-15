// components/layouts/ModalQrCode.jsx
import React from "react";
import { QRCodeCanvas } from "qrcode.react";
import "./modalQrCode.css";

const ModalQRCode = ({ isOpen, onClose, qrUrl }) => {
  if (!isOpen || !qrUrl) return null;

  return (
    <div className="qr-modal-overlay">
      <div className="qr-modal-content">
        <h2>QR Code da Pesquisa</h2>
        <QRCodeCanvas value={qrUrl} size={200} />
        <p style={{ marginTop: "1rem" }}>
          <a href={qrUrl} target="_blank" rel="noopener noreferrer">
            Ir para página de resposta
          </a>
        </p>
        <button onClick={onClose} className="btn-fechar">Fechar</button>
      </div>
    </div>
  );
};

export default ModalQRCode;
