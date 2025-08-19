import React, { useEffect, useRef, useState } from 'react';
import TopNavbar from '../../components/layouts/TopNavBar';
import { Rocket, Target, Users, Lightbulb, X, ExternalLink } from 'lucide-react';
import styles from './sobrenos.module.css';

/** Links reais por membro */
const equipe = [
  {
    nome: 'Leonardo Lawall',
    avatar: 'LL',
    funcao: 'Desenvolvedor Full Stack / UX/UI Designer',
    aria: 'Avatar de Leonardo Lawall',
    linkedin: 'https://www.linkedin.com/in/leonardo-frizoni-lawall',
    github: 'https://github.com/LeoFrizoni',
  },
  {
    nome: 'Lucas Arisio',
    avatar: 'LA',
    funcao: 'UX/UI Designer / Documentação',
    aria: 'Avatar de Lucas Arisio',
    linkedin: 'https://www.linkedin.com/in/lucas-arísio-müller',
    github: 'https://github.com/muller-lcs',
  },
  {
    nome: 'Marco Ryan',
    avatar: 'MR',
    funcao: 'Manutenção e gerenciamento do servidor / Desenvolvimento de IA',
    aria: 'Avatar de Marco Ryan',
    linkedin: 'https://www.linkedin.com/in/marcoryanmarassimarques',
    github: 'https://github.com/VonElfin',
  },
  {
    nome: 'Rafael Chiareli',
    avatar: 'RC',
    funcao: 'Coordenador do Projeto',
    aria: 'Avatar de Rafael Chiareli',
    linkedin: 'https://www.linkedin.com/in/rafael-chiareli-6184566b',
    github: 'https://github.com/rafaelchiareli',
  }
];

function PerfilModal({ open, onClose, membro }) {
  const closeBtnRef = useRef(null);

  useEffect(() => {
    if (!open) return;
    closeBtnRef.current?.focus();
    const onKey = (e) => e.key === 'Escape' && onClose();
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open || !membro) return null;

  return (
    <div className={styles.backdrop} onClick={onClose} aria-hidden="true">
      <div
        className={styles.modal}
        role="dialog"
        aria-modal="true"
        aria-labelledby="perfil-titulo"
        aria-describedby="perfil-descricao"
        onClick={(e) => e.stopPropagation()}
      >
        <button
          className={styles.modalClose}
          onClick={onClose}
          aria-label="Fechar modal"
          ref={closeBtnRef}
          type="button"
        >
          <X size={18} />
        </button>

        <div className={styles.modalHeader}>
          <div className={styles.avatarGrande} aria-label={membro.aria}>
            {membro.avatar}
          </div>
          <div>
            <h3 id="perfil-titulo" className={styles.modalTitle}>{membro.nome}</h3>
            <p id="perfil-descricao" className={styles.modalSubtitle}>{membro.funcao}</p>
          </div>
        </div>

        <div className={styles.modalBody}>
          <a
            className={styles.linkBtn}
            href={membro.linkedin}
            target="_blank"
            rel="noopener noreferrer"
            aria-label={`Abrir LinkedIn de ${membro.nome}`}
          >
            LinkedIn <ExternalLink size={16} />
          </a>
          <a
            className={styles.linkBtn}
            href={membro.github}
            target="_blank"
            rel="noopener noreferrer"
            aria-label={`Abrir GitHub de ${membro.nome}`}
          >
            GitHub <ExternalLink size={16} />
          </a>
        </div>
      </div>
    </div>
  );
}

const SobreNos = () => {
  const [aberto, setAberto] = useState(false);
  const [selecionado, setSelecionado] = useState(null);

  const abrirPerfil = (m) => { setSelecionado(m); setAberto(true); };
  const fecharPerfil = () => { setAberto(false); setSelecionado(null); };

  return (
    <>
      <TopNavbar />
      <div className={styles['sobre-nos-container']}>
        <div className={styles['conteudo-limitado']}>

          <div className={styles['secao']}>
            <h1><Rocket size={24} /> Sobre o FastSurvey</h1>
            <p>
              O <strong>FastSurvey</strong> é uma plataforma para simplificar a criação de pesquisas online.
              Buscamos um ambiente intuitivo e moderno para coletar dados e apoiar decisões com rapidez e confiança.
            </p>
          </div>

          <div className={styles['secao']}>
            <h2><Target size={20} /> Nossa Missão</h2>
            <p>
              Tornar a criação de pesquisas tão fácil quanto compartilhar uma ideia.
              Tecnologia como aliada para decisões com clareza, agilidade e acessibilidade.
            </p>
          </div>

          <div className={styles['secao']}>
            <h2><Users size={20} /> Equipe</h2>
            <div className={styles['cards-equipe']}>
              {equipe.map((membro) => (
                <button
                  key={membro.nome}
                  className={styles['card-membro']}
                  onClick={() => abrirPerfil(membro)}
                  aria-label={`Abrir perfil de ${membro.nome}`}
                  type="button"
                >
                  <div className={styles['avatar']} aria-label={membro.aria}>{membro.avatar}</div>
                  <h3>{membro.nome}</h3>
                  <p>{membro.funcao}</p>
                  <span className={styles.cliqueHint}>Clique para saber mais</span>
                </button>
              ))}
            </div>
          </div>

          <div className={styles['secao']}>
            <h2><Lightbulb size={20} /> Tecnologias Utilizadas</h2>
            <p>Nossa stack garante desempenho, segurança e escalabilidade:</p>
            <ul>
              <li><strong>Frontend:</strong> ReactJS, CSS Modules, Lucide Icons</li>
              <li><strong>Backend:</strong> C# (ASP.NET Core) com Entity Framework; Node.js para utilidades</li>
              <li><strong>Banco de Dados:</strong> SQL Server</li>
              <li><strong>Integrações:</strong> APIs RESTful, OAuth 2.0 (Login com Google – planejado)</li>
              <li><strong>Outros:</strong> QR Code dinâmico, Toastify, tema claro/escuro</li>
            </ul>
          </div>

        </div>
      </div>

      <PerfilModal open={aberto} onClose={fecharPerfil} membro={selecionado} />
    </>
  );
};

export default SobreNos;
