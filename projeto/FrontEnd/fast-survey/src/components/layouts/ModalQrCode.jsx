import React, { useRef } from "react";
import { QRCodeCanvas } from "qrcode.react";
import styles from "./modal-qr-code.module.css";

const ModalQRCode = ({ isOpen, onClose, qrUrl }) => {
  const qrRef = useRef(null);

  if (!isOpen || !qrUrl) return null;

  const handleDownload = () => {
    const canvas = qrRef.current?.querySelector("canvas");
    if (!canvas) return;
    const pngUrl = canvas.toDataURL("image/png");
    const a = document.createElement("a");
    a.href = pngUrl;
    a.download = "fastsurvey-qrcode.png";
    a.click();
  };

  return (
    <div className={styles["qr-modal-overlay"]}>
      <div className={styles["qr-modal-content"]} role="dialog" aria-modal="true">
        <h2>QR Code da Pesquisa</h2>

        <div ref={qrRef} aria-label="QR code gerado">
          <QRCodeCanvas value={qrUrl} size={220} includeMargin />
        </div>

        <p style={{ marginTop: "1rem" }}>
          <a href={qrUrl} target="_blank" rel="noopener noreferrer">
            Ir para página de resposta
          </a>
        </p>

        <div style={{ display: "flex", gap: 8, marginTop: 8 }}>
          <button onClick={handleDownload} className={styles["btn-download"]}>Baixar PNG</button>
          <button onClick={onClose} className={styles["btn-fechar"]}>Fechar</button>
        </div>
      </div>
    </div>
  );
};

export default ModalQRCode;
