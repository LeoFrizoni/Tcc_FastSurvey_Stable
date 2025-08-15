
import React from 'react';
import TopNavbar from '../../components/layouts/TopNavBar';
import { Rocket, Target, Users, Lightbulb } from 'lucide-react';
import './sobrenos.css';

// Array de membros da equipe para facilitar manutenção e escalabilidade
const equipe = [
  {
    nome: 'Leonardo Lawall',
    avatar: 'LF',
    funcao: 'Desenvolvedor Full Stack / UX/UI Designer',
    aria: 'Avatar de Leonardo Lawall'
  },
  {
    nome: 'Lucas Arisio',
    avatar: 'LA',
    funcao: 'UX/UI Designer / Documentacao',
    aria: 'Avatar de Lucas Arisio'
  },
  {
    nome: 'Marco Ryan',
    avatar: 'MR',
    funcao: 'Manutenção e gerenciamento do servidor / Desenvolvimento da IA',
    aria: 'Avatar de Marco Ryan'
  },
  {
    nome: 'Rafael Chiareli',
    avatar: 'MR',
    funcao: 'Coordenador do Projeto',
    aria: 'Avatar de Rafael Chiareli'
  }
];

// Componente principal da página Sobre Nós
const SobreNos = () => {
  return (
    <>
      <TopNavbar />
      <div className="sobre-nos-container">
        <div className="conteudo-limitado">
          {/* Seção sobre o projeto */}
          <div className="secao">
            <h1><Rocket size={24} /> Sobre o FastSurvey</h1>
            <p>
              O <strong>FastSurvey</strong> é uma plataforma desenvolvida com o propósito de simplificar e agilizar a criação
              de pesquisas online. Criamos um ambiente acessível e intuitivo para quem deseja coletar dados com eficiência e elegância.
            </p>
          </div>

          {/* Seção missão */}
          <div className="secao">
            <h2><Target size={20} /> Nossa Missão</h2>
            <p>
              Tornar o processo de criação de pesquisas tão simples quanto escrever um post. Acreditamos na tecnologia como ponte para decisões melhores.
            </p>
          </div>

          {/* Seção equipe */}
          <div className="secao">
            <h2><Users size={20} /> Equipe</h2>
            <div className="cards-equipe">
              {equipe.map((membro, idx) => (
                <div className="card-membro" key={membro.nome}>
                  <div className="avatar" aria-label={membro.aria}>{membro.avatar}</div>
                  <div>
                    <h3>{membro.nome}</h3>
                    <p>{membro.funcao}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Seção tecnologias */}
          <div className="secao">
            <h2><Lightbulb size={20} /> Tecnologias Utilizadas</h2>
            <p>
              ReactJS, Node.js, C# com Entity Framework, SQL Server, Lucide Icons, e muito amor por código limpo e usabilidade.
            </p>
          </div>
        </div>
      </div>
    </>
  );
};

export default SobreNos;