import React, { useEffect, useRef, useState } from 'react';
import TopNavbar from '../../components/layouts/TopNavBar';
import { Rocket, Target, Users, Lightbulb, X, ExternalLink, Shield, Zap, BarChart3, Smartphone } from 'lucide-react';
import styles from './sobrenos.module.css';

/** Links reais por membro */
const equipe = [
  {
    nome: 'Leonardo Lawall',
    avatar: 'LL',
    funcao: 'Desenvolvedor Full Stack & UX/UI Designer',
    descricao: 'Especialista em desenvolvimento frontend e backend, responsável pela arquitetura da aplicação e design de interface',
    aria: 'Avatar de Leonardo Lawall',
    linkedin: 'https://www.linkedin.com/in/leonardo-frizoni-lawall',
    github: 'https://github.com/LeoFrizoni',
  },
  {
    nome: 'Lucas Arisio',
    avatar: 'LA',
    funcao: 'UX/UI Designer & Documentação Técnica',
    descricao: 'Focado na experiência do usuário, design de interface e elaboração da documentação técnica do projeto',
    aria: 'Avatar de Lucas Arisio',
    linkedin: 'https://www.linkedin.com/in/lucas-arísio-müller',
    github: 'https://github.com/muller-lcs',
  },
  {
    nome: 'Marco Ryan',
    avatar: 'MR',
    funcao: 'DevOps & Desenvolvimento de IA',
    descricao: 'Responsável pela infraestrutura, manutenção de servidores e implementação de funcionalidades de inteligência artificial',
    aria: 'Avatar de Marco Ryan',
    linkedin: 'https://www.linkedin.com/in/marcoryanmarassimarques',
    github: 'https://github.com/VonElfin',
  },
  {
    nome: 'Rafael Chiareli',
    avatar: 'RC',
    funcao: 'Coordenador do Projeto',
    descricao: 'Lidera o desenvolvimento do projeto, define requisitos e garante a qualidade da entrega final',
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
          <p className={styles.membroDescricao}>{membro.descricao}</p>
          <div className={styles.modalLinks}>
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
              O <strong>FastSurvey</strong> é uma plataforma inovadora desenvolvida para democratizar a criação e gestão de pesquisas online. 
              Nossa solução oferece uma experiência intuitiva e profissional, permitindo que organizações e indivíduos coletem dados 
              de forma eficiente, analisem resultados através de visualizações avançadas e tomem decisões baseadas em evidências 
              com máxima confiabilidade e agilidade.
            </p>
          </div>

          <div className={styles['secao']}>
            <h2><Target size={20} /> Nossa Missão</h2>
            <p>
              Transformar a coleta de dados em um processo simples, rápido e confiável. Buscamos capacitar nossos usuários 
              através de tecnologia de ponta, oferecendo ferramentas que facilitem a tomada de decisões estratégicas 
              baseadas em insights valiosos e análises precisas.
            </p>
          </div>

          <div className={styles['secao']}>
            <h2><Zap size={20} /> Diferenciais da Plataforma</h2>
            <div className={styles['diferenciais-grid']}>
              <div className={styles['diferencial-card']}>
                <Shield size={24} className={styles['diferencial-icon']} />
                <h3>Segurança Avançada</h3>
                <p>Autenticação JWT, criptografia de dados e controle de acesso granular para proteger suas informações</p>
              </div>
              <div className={styles['diferencial-card']}>
                <BarChart3 size={24} className={styles['diferencial-icon']} />
                <h3>Visualizações Inteligentes</h3>
                <p>Gráficos interativos e relatórios detalhados para análise profunda dos resultados coletados</p>
              </div>
              <div className={styles['diferencial-card']}>
                <Smartphone size={24} className={styles['diferencial-icon']} />
                <h3>Design Responsivo</h3>
                <p>Interface adaptável que funciona perfeitamente em desktop, tablet e dispositivos móveis</p>
              </div>
              <div className={styles['diferencial-card']}>
                <Zap size={24} className={styles['diferencial-icon']} />
                <h3>Performance Otimizada</h3>
                <p>Arquitetura escalável que garante resposta rápida mesmo com alto volume de dados</p>
              </div>
            </div>
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
            <h2><Lightbulb size={20} /> Arquitetura e Tecnologias</h2>
            <p>
              Desenvolvemos o FastSurvey utilizando tecnologias modernas e robustas, garantindo alta performance, 
              segurança e escalabilidade para atender às demandas de nossos usuários:
            </p>
            <ul>
              <li><strong>Frontend:</strong> React.js com hooks avançados, CSS Modules para estilização modular, e Lucide Icons para interface consistente</li>
              <li><strong>Backend:</strong> ASP.NET Core com C#, Entity Framework para ORM, e arquitetura em camadas para manutenibilidade</li>
              <li><strong>Banco de Dados:</strong> Microsoft SQL Server com otimizações de performance e integridade referencial</li>
              <li><strong>APIs e Integrações:</strong> RESTful APIs com autenticação JWT, OAuth 2.0 para login social, e documentação OpenAPI</li>
              <li><strong>Funcionalidades Avançadas:</strong> Geração dinâmica de QR Codes, sistema de notificações em tempo real, visualizações gráficas interativas, e exportação de dados em múltiplos formatos</li>
            </ul>
          </div>

        </div>
      </div>

      <PerfilModal open={aberto} onClose={fecharPerfil} membro={selecionado} />
    </>
  );
};

export default SobreNos;
