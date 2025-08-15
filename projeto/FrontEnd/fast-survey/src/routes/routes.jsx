import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Login from '../pages/login/login';
import HomePage from '../pages/home/home';
import Perfil from '../pages/perfil/perfil';
import SobreNos from '../pages/sobrenos/SobreNos';
import CriarPesquisa from '../pages/createPesquisa/createPesquisa';
import ResultadoPesquisa from '../pages/minhasPesquisas/resultadosPesquisa';
import EditarPesquisa from '../pages/minhasPesquisas/editarPesquisa';
import Admin from '../pages/admin/Admin';
import PrivateRoute from '../components/utilities/PrivateRoute';
import ResponderPesquisa from '../pages/responderPesquisa/ResponderPesquisa';

// Mobile Components
import MobileLogin from '../pages/mobile/MobileLogin';
import MobileHome from '../pages/mobile/MobileHome';
import MobilePerfil from '../pages/mobile/MobilePerfil';
import MobileSobreNos from '../pages/mobile/MobileSobreNos';
import MobileCreatePesquisa from '../pages/mobile/MobileCreatePesquisa';
import MobileResultadosPesquisa from '../pages/mobile/MobileResultadosPesquisa';
import MobileEditarPesquisa from '../pages/mobile/MobileEditarPesquisa';
import MobileAdmin from '../pages/mobile/MobileAdmin';
import MobileResponderPesquisa from '../pages/mobile/MobileResponderPesquisa';
import MobileMinhasPesquisas from '../pages/mobile/MobileMinhasPesquisas';

const PrivateRouteAdmin = ({ children }) => {
  const tipoUsuarioId = parseInt(localStorage.getItem('tipousuarioid'), 10);
  const token = localStorage.getItem('token');

  if (!token) {
    return <Navigate to="/login" />;
  }

  return tipoUsuarioId === 15 ? children : <Navigate to="/home" />;
};

// Função para detectar mobile
function isMobile() {
  return /Mobi|Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(window.navigator.userAgent);
}

const AppRoutes = () => {
  const mobile = isMobile();
  return (
    <Router>
      <Routes>
        <Route path="/login" element={mobile ? <MobileLogin /> : <Login />} />

        <Route
          path="/home"
          element={
            <PrivateRoute>
              {mobile ? <MobileHome /> : <HomePage />}
            </PrivateRoute>
          }
        />
        <Route
          path="/perfil"
          element={
            <PrivateRoute>
              {mobile ? <MobilePerfil /> : <Perfil />}
            </PrivateRoute>
          }
        />
        <Route
          path="/sobre-nos"
          element={
            <PrivateRoute>
              {mobile ? <MobileSobreNos /> : <SobreNos />}
            </PrivateRoute>
          }
        />
        <Route
          path="/criar"
          element={
            <PrivateRoute>
              {mobile ? <MobileCreatePesquisa /> : <CriarPesquisa />}
            </PrivateRoute>
          }
        />
        <Route
          path="/minhas-pesquisas/editar/:id"
          element={
            <PrivateRoute>
              {mobile ? <MobileEditarPesquisa /> : <EditarPesquisa />}
            </PrivateRoute>
          }
        />
        <Route
          path="/minhas-pesquisas/resultado/:id"
          element={
            <PrivateRoute>
              {mobile ? <MobileResultadosPesquisa /> : <ResultadoPesquisa />}
            </PrivateRoute>
          }
        />
        <Route
          path="/minhas-pesquisas"
          element={
            <PrivateRoute>
              {mobile ? <MobileMinhasPesquisas /> : <ResultadoPesquisa />}
            </PrivateRoute>
          }
        />
        <Route path="/responder/:id" element={mobile ? <MobileResponderPesquisa /> : <ResponderPesquisa />} />
        <Route
          path="/admin"
          element={
            <PrivateRouteAdmin>
              {mobile ? <MobileAdmin /> : <Admin />}
            </PrivateRouteAdmin>
          }
        />
        <Route path="/" element={<Navigate to="/login" />} />
      </Routes>
    </Router>
  );
};

export default AppRoutes;
