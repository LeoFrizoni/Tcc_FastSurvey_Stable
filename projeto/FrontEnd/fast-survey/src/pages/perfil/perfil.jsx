// src/pages/perfil/Perfil.jsx
import React, { useEffect, useMemo, useState, useRef } from 'react';
import TopNavbar from '../../components/layouts/TopNavBar';
import { User, Tag, BadgeCheck, Pencil, Save, X, Image as ImgIcon, Trash2, CheckCircle } from 'lucide-react';
import styles from './perfil.module.css';
import api from '../../lib/api';
import AvatarCropModal from '../../components/layouts/AvatarCropModal';
import '../../components/layouts/global.css';
import { checkUsernameAvailability } from '../../helpers/validateEmail';

const EP = {
  getPerfil: '/api/Login/Perfil',
  putNome: '/api/Login/Nome',
  uploadAvatar: '/api/Avatar',
  deleteAvatar: '/api/Avatar',
};

const mapTipoUsuario = { 13: 'Usuário Gratuito', 14: 'Usuário Premium', 15: 'Administrador' };

/** Lê avatar a partir de vários formatos possíveis do backend. */
function resolveAvatarUrlLike(raw, baseURL) {
  if (!raw) return null;

  const avatarObj = raw.avatar ?? raw.Avatar ?? null;
  const bag = avatarObj || raw;

  const urlRaw =
    bag?.Url ??
    bag?.url ??
    bag?.storageUrl ??
    bag?.storageurl ??
    bag?.avatarUrl ??
    bag?.avatarurl ??
    bag?.fotoUrl ??
    bag?.fotourl ??
    bag?.foto ??
    null;

  if (!urlRaw) return null;

  const abs = /^https?:\/\//i.test(urlRaw)
    ? String(urlRaw)
    : (String(baseURL || '').replace(/\/+$/, '') + '/' + String(urlRaw).replace(/^\/+/, ''));

  const versao =
    bag?.Versao ??
    bag?.versao ??
    bag?.version ??
    Date.now();

  return abs + (abs.includes('?') ? `&v=${versao}` : `?v=${versao}`);
}

export default function Perfil() {
  const [usuario, setUsuario] = useState({ nome: '', tipoUsuarioId: '', status: 'Ativo' });
  const [avatarUrl, setAvatarUrl] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editando, setEditando] = useState(false);
  const [novoNome, setNovoNome] = useState('');
  const [salvando, setSalvando] = useState(false);
  
  // Validação de usuário
  const [validandoUsuario, setValidandoUsuario] = useState(false);
  const [usuarioDisponivel, setUsuarioDisponivel] = useState(null);
  const [usuarioMensagem, setUsuarioMensagem] = useState("");

  // avatar cropper
  const [showCropper, setShowCropper] = useState(false);
  const [selectedFile, setSelectedFile] = useState(null);

  // modal exclusão
  const [showDelete, setShowDelete] = useState(false);
  
  // notificação de sucesso
  const [showSuccess, setShowSuccess] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');

  const baseURL = api?.defaults?.baseURL || '';

  // Função para mostrar notificação de sucesso
  const showSuccessNotification = (message) => {
    setSuccessMessage(message);
    setShowSuccess(true);
    setTimeout(() => {
      setShowSuccess(false);
    }, 3000);
  };

  useEffect(() => {
    const carregar = async () => {
      try {
        const r = await api.get(EP.getPerfil);
        const d = r?.data || {};
        setUsuario({
          nome: d.usuario || d.nome || '',
          tipoUsuarioId: d.tipoUsuarioId ?? d.tipousuarioid ?? d.tipo ?? '',
          status: d.status || 'Ativo',
        });
                 setNovoNome(d.usuario || d.nome || '');
         setAvatarUrl(resolveAvatarUrlLike(d, baseURL));
      } catch (e) {
        console.error('Erro ao buscar perfil:', e);
      } finally {
        setLoading(false);
      }
    };
    carregar();
  }, [baseURL]);

  // Cleanup do debounce quando componente for desmontado
  useEffect(() => {
    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current);
      }
    };
  }, []);

  const iniciais = useMemo(() => {
    const n = (usuario.nome || '').trim();
    if (!n) return '??';
    const letters = n.split(/\s+/).map(p => p[0]).filter(Boolean).slice(0, 2);
    return letters.join('').toUpperCase();
  }, [usuario.nome]);

  const tipoUsuarioLabel = mapTipoUsuario[usuario.tipoUsuarioId] || usuario.tipoUsuarioId || '—';

  const iniciarEdicao = () => setEditando(true);
  const cancelarEdicao = () => { 
    setEditando(false); 
    setNovoNome(usuario.nome || ''); 
    setUsuarioDisponivel(null);
    setUsuarioMensagem("");
  };

  // Validação de nome de usuário em tempo real
  const validarNomeUsuario = async (nome) => {
    if (!nome || nome.length < 3) {
      setUsuarioDisponivel(null);
      setUsuarioMensagem("");
      return;
    }

    // Se o nome não mudou, não precisa validar
    if (nome === usuario.nome) {
      setUsuarioDisponivel(true);
      setUsuarioMensagem("Nome atual");
      return;
    }

    setValidandoUsuario(true);
    try {
      const resultado = await checkUsernameAvailability(nome, api);
      setUsuarioDisponivel(resultado.available);
      setUsuarioMensagem(resultado.message);
    } catch (error) {
      console.error('Erro na validação:', error);
      setUsuarioDisponivel(false);
      setUsuarioMensagem("Erro ao verificar disponibilidade");
    } finally {
      setValidandoUsuario(false);
    }
  };

  // Debounce para validação de usuário
  const debounceRef = useRef(null);
  const handleNomeChange = (value) => {
    setNovoNome(value);
    
    // Limpa o timeout anterior
    if (debounceRef.current) {
      clearTimeout(debounceRef.current);
    }
    
    // Define novo timeout para validar após 500ms
    debounceRef.current = setTimeout(() => {
      validarNomeUsuario(value);
    }, 500);
  };

  const salvar = async () => {
    const n1 = (novoNome || '').trim();
    const n0 = (usuario.nome || '').trim();
    if (!n1 || n1 === n0) { setEditando(false); return; }
    if (usuarioDisponivel === false) {
      alert("Nome de usuário já está em uso. Escolha outro nome.");
      return;
    }
    if (validandoUsuario) {
      alert("Aguarde a validação do nome de usuário.");
      return;
    }
    setSalvando(true);
    try {
                    await api.put(EP.putNome, { novoNome: n1 });
       setUsuario(u => ({ ...u, nome: n1 }));
       setEditando(false);
       showSuccessNotification('Nome atualizado com sucesso!');
    } catch (e) {
      console.error('Erro ao salvar nome:', e);
      alert('Não foi possível salvar o nome agora.');
    } finally {
      setSalvando(false);
    }
  };

  // abrir seletor => modal
  const escolherArquivo = () => {
    const el = document.createElement('input');
    el.type = 'file';
    el.accept = 'image/*';
    el.onchange = (e) => {
      const f = e.target.files?.[0];
      if (!f) return;
      setSelectedFile(f);
      setShowCropper(true);
    };
    el.click();
  };

  // recebe Blob do modal, envia para API
  const onConfirmCrop = async (blob, cropMeta) => {
    setShowCropper(false);
    if (!blob) return;

    const form = new FormData();
    const name = selectedFile?.name?.replace(/\.[^.]+$/, '') || 'avatar';
    const file = new File([blob], `${name}.jpg`, { type: 'image/jpeg' });

    // backend espera "File" com F maiúsculo
    form.append('File', file);

    if (cropMeta) {
      if (Number.isFinite(cropMeta.x)) form.append('X', String(Math.round(cropMeta.x)));
      if (Number.isFinite(cropMeta.y)) form.append('Y', String(Math.round(cropMeta.y)));
      if (Number.isFinite(cropMeta.w)) form.append('W', String(Math.round(cropMeta.w)));
      if (Number.isFinite(cropMeta.h)) form.append('H', String(Math.round(cropMeta.h)));
      if (Number.isFinite(cropMeta.rotate)) form.append('Rotate', String(cropMeta.rotate));
      if (Number.isFinite(cropMeta.scale)) form.append('Scale', String(cropMeta.scale));
    }

    try {
      const r = await api.post(EP.uploadAvatar, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });

      const uploaded = r?.data?.Url ?? r?.data?.url ?? r?.data?.storageurl ?? r?.data?.storageUrl ?? null;

      if (uploaded) {
        const abs = /^https?:\/\//i.test(uploaded)
          ? uploaded
          : (String(baseURL).replace(/\/+$/, '') + '/' + String(uploaded).replace(/^\/+/, ''));
        const v = r?.data?.Versao ?? r?.data?.versao ?? Date.now();
        setAvatarUrl(abs + (abs.includes('?') ? `&v=${v}` : `?v=${v}`));
      }

             // Recarrega o perfil para sincronizar
       try {
         const r2 = await api.get(EP.getPerfil);
         const resolved = resolveAvatarUrlLike(r2?.data || {}, baseURL);
         if (resolved) setAvatarUrl(resolved);
       } catch {}
       
       showSuccessNotification('Avatar atualizado com sucesso!');
    } catch (err) {
      console.error('Erro ao enviar avatar:', err);
      alert('Falha ao enviar avatar.');
    } finally {
      setSelectedFile(null);
    }
  };

  const removerAvatar = async () => {
    setShowDelete(false);
    try {
      await api.delete(EP.deleteAvatar);
      setAvatarUrl(null);
      showSuccessNotification('Avatar removido com sucesso!');
    } catch (e) {
      console.error('Erro ao remover avatar:', e);
      alert('Não foi possível remover o avatar agora.');
    }
  };

  return (
    <>
      <TopNavbar />
      <div className={styles.page}>
        <header className={styles.header}>
          <h1 className={styles.title}>Meu Perfil</h1>
          <p className={styles.subtitle}>Visualize, edite seu nome e altere seu avatar.</p>
        </header>

        <section className={styles.center}>
          <div className={styles.card}>
            {/* NOVO: cabeçalho do card contendo avatar + ações */}
            <div className={styles.cardHeader}>
              <div className={styles.avatarBox} aria-label="Avatar do usuário">
                {avatarUrl ? (
                  <img key={avatarUrl} src={avatarUrl} alt="Avatar" className={styles.avatarImg} />
                ) : (
                  <span className={styles.avatarInitials}>{iniciais}</span>
                )}
              </div>

              <div className={styles.avatarActions}>
                <button className={styles.btnSecondary} onClick={escolherArquivo} title="Alterar avatar">
                  <ImgIcon size={16} />
                  <span>Alterar avatar</span>
                </button>
                                 {avatarUrl && (
                   <button className={styles.btnDanger} onClick={() => setShowDelete(true)} title="Remover avatar">
                     <Trash2 size={16} />
                     <span>Remover</span>
                   </button>
                 )}
              </div>
            </div>

            <ul className={styles.info}>
              <li>
                <User size={18} /><strong>Nome:</strong>
                {editando ? (
                  <div>
                    <input
                      className={styles.input}
                      type="text"
                      value={novoNome}
                      onChange={(e) => handleNomeChange(e.target.value)}
                      maxLength={80}
                      autoFocus
                    />
                    {editando && novoNome && (
                      <div style={{ marginTop: '5px' }}>
                        {validandoUsuario && (
                          <p style={{ color: "orange", fontSize: "12px", margin: "3px 0" }}>
                            ⏳ Verificando disponibilidade...
                          </p>
                        )}
                        {!validandoUsuario && usuarioDisponivel === true && (
                          <p style={{ color: "green", fontSize: "12px", margin: "3px 0" }}>
                            ✓ {usuarioMensagem}
                          </p>
                        )}
                        {!validandoUsuario && usuarioDisponivel === false && (
                          <p style={{ color: "red", fontSize: "12px", margin: "3px 0" }}>
                            ✗ {usuarioMensagem}
                          </p>
                        )}
                      </div>
                    )}
                  </div>
                ) : (
                  <span>{loading ? '…' : (usuario.nome || '—')}</span>
                )}
              </li>

              <li>
                <Tag size={18} /><strong>Tipo de Usuário:</strong>
                <span>{loading ? '…' : tipoUsuarioLabel}</span>
              </li>

              <li>
                <BadgeCheck size={18} /><strong>Status:</strong>
                <span>{loading ? '…' : (usuario.status || '—')}</span>
              </li>
            </ul>

            <div className={styles.actions}>
              {editando ? (
                <>
                  <button className={styles.btnPrimary} onClick={salvar} disabled={salvando}>
                    <Save size={16} />
                    <span>{salvando ? 'Salvando…' : 'Salvar'}</span>
                  </button>
                  <button className={styles.btnGhost} onClick={cancelarEdicao} disabled={salvando}>
                    <X size={16} />
                    <span>Cancelar</span>
                  </button>
                </>
              ) : (
                <button className={styles.btnSecondary} onClick={iniciarEdicao}>
                  <Pencil size={16} />
                  <span>Editar</span>
                </button>
              )}
            </div>
          </div>
        </section>
      </div>

      {/* Modal de Crop */}
      <AvatarCropModal
        isOpen={showCropper}
        file={selectedFile}
        onClose={() => { setShowCropper(false); setSelectedFile(null); }}
        onConfirm={onConfirmCrop}
      />

             {/* Modal de exclusão simples */}
       {showDelete && (
         <div className={styles.cropOverlay} role="dialog" aria-modal="true" aria-label="Remover avatar">
           <div className={styles.confirmModal}>
             <div className={styles.confirmHeader}>
               <h3>Remover avatar</h3>
               <button className={styles.confirmClose} onClick={() => setShowDelete(false)} aria-label="Fechar">×</button>
             </div>
             <div className={styles.confirmBody}>
               <p>Tem certeza que deseja remover seu avatar?</p>
             </div>
             <div className={styles.confirmFooter}>
               <button className={styles.btnSecondary} onClick={() => setShowDelete(false)}>Cancelar</button>
               <button className={styles.btnDanger} onClick={removerAvatar}>Remover</button>
             </div>
           </div>
         </div>
       )}

       {/* Notificação de sucesso */}
       {showSuccess && (
         <div className={styles.successNotification}>
           <CheckCircle size={18} />
           <span>{successMessage}</span>
         </div>
       )}
     </>
   );
 }
