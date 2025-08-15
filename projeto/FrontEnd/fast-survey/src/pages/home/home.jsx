import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, User, LogOut, Info } from 'lucide-react';
import axios from 'axios';
import './home.css';

const HomePage = () => {
  const navigate = useNavigate();
  const [pesquisas, setPesquisas] = useState([]);

  useEffect(() => {
  const buscarPesquisas = async () => {
    const loginId = parseInt(localStorage.getItem("userId"));
    const token = localStorage.getItem("token");

    if (!loginId || !token) {
      console.warn("Usuário não autenticado.");
      return;
    }

    try {
      const response = await axios.get(`http://localhost:5062/api/pesquisas/usuario/${loginId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      console.log("🧪 Dados recebidos do backend:", response.data); // 👈 ADICIONE ISSO
      setPesquisas(response.data);
    } catch (error) {
      console.error("Erro ao buscar pesquisas:", error);
    }
  };

  buscarPesquisas();
}, []);


  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  return (
    <div className="dashboard-container">
      {/* Sidebar */}
      <aside className="sidebar">
        <div className="logo">FastSurvey</div>
        <nav>
          <ul>
            <li onClick={() => navigate("/perfil")}>
              <User size={18} /> Perfil
            </li>
            <li onClick={() => navigate("/sobre-nos")}>
              <Info size={18} /> Sobre Nós
            </li>
            <li onClick={handleLogout} aria-label="Sair">
              <LogOut size={18} /> Sair
            </li>
          </ul>
        </nav>
        <footer>2025 FastSurvey. Todos os direitos reservados.</footer>
      </aside>

      {/* Conteúdo Principal */}
      <main className="main-content">
        <div className="titulo-header">
          <h2>Minhas Pesquisas</h2>
          <button className="botao-criar" onClick={() => navigate("/criar")}>
            <Plus size={16} /> Criar Pesquisa
          </button>
        </div>

        {pesquisas.length === 0 ? (
          <div className="no-data">
            Nenhuma pesquisa encontrada. Clique em <strong>"Criar Pesquisa"</strong> para começar!
          </div>
        ) : (
          <div className="cards-wrapper">
            {pesquisas.map((p) => (
              <div
                key={p.pesquisaid}
                className="card-pesquisa"
                onClick={() => navigate(`/minhas-pesquisas/resultado/${p.pesquisaid}`)}
              >
                <h3>{p.titulo}</h3>
                <p>{p.descricao}</p>
                <p className="tag">{p.tipoPesquisa?.descricao || "Tipo desconhecido"}</p>
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
};

export default HomePage;
