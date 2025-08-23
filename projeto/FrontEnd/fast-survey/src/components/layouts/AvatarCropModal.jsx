// src/components/layouts/AvatarCropModal.jsx
import React, { useCallback, useEffect, useState } from 'react';
import Cropper from 'react-easy-crop';

/* Helpers de imagem */
const createImage = (url) =>
  new Promise((resolve, reject) => {
    const img = new Image();
    img.addEventListener('load', () => resolve(img));
    img.addEventListener('error', (e) => reject(e));
    img.setAttribute('crossOrigin', 'anonymous');
    img.src = url;
  });

const toRad = (deg) => (deg * Math.PI) / 180;

function getRotatedSize(width, height, rotation) {
  const rotRad = toRad(rotation);
  return {
    width: Math.abs(Math.cos(rotRad) * width) + Math.abs(Math.sin(rotRad) * height),
    height: Math.abs(Math.sin(rotRad) * width) + Math.abs(Math.cos(rotRad) * height),
  };
}

async function getCroppedImg(imageSrc, pixelCrop, rotation = 0) {
  const image = await createImage(imageSrc);
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');

  const { width: bW, height: bH } = getRotatedSize(image.width, image.height, rotation);
  canvas.width = bW;
  canvas.height = bH;

  ctx.translate(bW / 2, bH / 2);
  ctx.rotate(toRad(rotation));
  ctx.translate(-image.width / 2, -image.height / 2);
  ctx.drawImage(image, 0, 0);

  const data = ctx.getImageData(pixelCrop.x, pixelCrop.y, pixelCrop.width, pixelCrop.height);

  canvas.width = pixelCrop.width;
  canvas.height = pixelCrop.height;
  ctx.putImageData(data, 0, 0);

  return new Promise((resolve) => {
    canvas.toBlob((blob) => resolve(blob), 'image/jpeg', 0.92);
  });
}

/* Modal */
export default function AvatarCropModal({ isOpen, file, onClose, onConfirm }) {
  const [imageUrl, setImageUrl] = useState(null);
  const [crop, setCrop] = useState({ x: 0, y: 0 });
  const [zoom, setZoom] = useState(1);
  const [rotation, setRotation] = useState(0);
  const [croppedAreaPixels, setCroppedAreaPixels] = useState(null);
  const [working, setWorking] = useState(false);

  useEffect(() => {
    if (!file) return setImageUrl(null);
    const url = typeof file === 'string' ? file : URL.createObjectURL(file);
    setImageUrl(url);
    return () => {
      if (url && typeof file !== 'string') URL.revokeObjectURL(url);
    };
  }, [file]);

  const onCropComplete = useCallback((_, pixels) => setCroppedAreaPixels(pixels), []);

  const handleConfirm = useCallback(async () => {
    if (!imageUrl || !croppedAreaPixels) return;
    setWorking(true);
    try {
      const blob = await getCroppedImg(imageUrl, croppedAreaPixels, rotation);
      // envia também os metadados de crop para o back (se quiser aproveitar depois)
      onConfirm?.(blob, {
        x: croppedAreaPixels.x,
        y: croppedAreaPixels.y,
        w: croppedAreaPixels.width,
        h: croppedAreaPixels.height,
        rotate: rotation,
        scale: zoom,
      });
    } finally {
      setWorking(false);
    }
  }, [imageUrl, croppedAreaPixels, rotation, zoom, onConfirm]);

  if (!isOpen) return null;

  return (
    <div className="cropOverlay" role="dialog" aria-modal="true" aria-label="Recortar avatar">
      <div className="cropModal">
        <div className="cropHeader">
          <h3>Defina seu avatar</h3>
          <button className="cropClose" onClick={() => !working && onClose?.()} aria-label="Fechar">×</button>
        </div>

        <div className="cropBody">
          {imageUrl && (
            <div className="cropStage">
              <Cropper
                image={imageUrl}
                crop={crop}
                zoom={zoom}
                rotation={rotation}
                aspect={1}
                cropShape="round"
                showGrid={false}
                onCropChange={setCrop}
                onZoomChange={setZoom}
                onRotationChange={setRotation}
                onCropComplete={onCropComplete}
                objectFit="contain"
              />
            </div>
          )}
          <div className="cropControls">
            <label>
              Zoom
              <input type="range" min={1} max={3} step={0.01} value={zoom} onChange={(e) => setZoom(Number(e.target.value))} />
            </label>
            <label>
              Rotação
              <input type="range" min={0} max={360} step={1} value={rotation} onChange={(e) => setRotation(Number(e.target.value))} />
            </label>
          </div>
        </div>

        <div className="cropFooter">
          <button className="btnGhost" onClick={() => !working && onClose?.()} disabled={working}>Cancelar</button>
          <button className="btnPrimary" onClick={handleConfirm} disabled={working || !imageUrl}>
            {working ? 'Processando…' : 'Salvar'}
          </button>
        </div>
      </div>

      <style>{`
        .cropOverlay { position: fixed; inset: 0; background: rgba(0,0,0,.7); display:flex; justify-content:center; align-items:center; z-index:1000; }
        .cropModal { width:min(92vw,720px); background:#fff; border-radius:16px; box-shadow:0 20px 60px rgba(0,0,0,.5); color:#333; display:flex; flex-direction:column; overflow:hidden; }
        .cropHeader { padding:16px; background:#f7f7fb; border-bottom:1px solid #e0e0e0; display:flex; justify-content:space-between; align-items:center; }
        .cropBody { padding:16px; background:#fff; }
        .cropStage { position:relative; width:100%; height:380px; background:#f0f0f0; border-radius:12px; overflow:hidden; border:1px solid #e0e0e0; margin-bottom:16px; }
        .cropControls { display:flex; gap:16px; margin-top:16px; }
        .cropControls label { display:flex; flex-direction:column; font-size:14px; color:#333; }
        .cropFooter { padding:16px; background:#f7f7fb; border-top:1px solid #e0e0e0; display:flex; justify-content:flex-end; gap:8px; }
        .btnPrimary { background:#4caf50; color:#fff; border:none; padding:10px 16px; border-radius:8px; cursor:pointer; }
        .btnPrimary:hover { background:#45a049; }
        .btnGhost { background:transparent; color:#333; border:1px solid #e0e0e0; padding:10px 16px; border-radius:8px; cursor:pointer; }
        .btnGhost:hover { background:#f0f0f0; }
      `}</style>
    </div>
  );
}
