import React from 'react';
import { useParams } from 'react-router-dom';
import { Gamepad2 } from 'lucide-react';
import styles from './GamePlayer.module.css';

const GamePlayer = () => {
  const { sessionId } = useParams();

  return (
    <div className={styles.container}>
      <div className={styles.content}>
        <div className={styles.gameIcon}>
          <Gamepad2 size={48} />
        </div>
        <h1>Página do Jogador</h1>
        <p>Sessão ID: {sessionId}</p>
        <p>Esta página será implementada em breve!</p>
        <div className={styles.features}>
          <h3>Funcionalidades planejadas:</h3>
          <ul>
            <li>🎯 Interface de resposta gamificada</li>
            <li>⏱️ Timer visual</li>
            <li>🏆 Ranking em tempo real</li>
            <li>💫 Power-ups e animações</li>
            <li>📱 Design responsivo</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default GamePlayer;
