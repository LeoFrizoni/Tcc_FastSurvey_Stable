// import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import React, { Suspense, lazy } from 'react';
import PrivateRoute from '../components/utilities/PrivateRoute';

const Login = lazy(() => import('../pages/login/login'));
const HomePage = lazy(() => import('../pages/home/home'));
const Perfil = lazy(() => import('../pages/perfil/perfil'));
const SobreNos = lazy(() => import('../pages/sobrenos/SobreNos'));
const CriarPesquisa = lazy(() => import('../pages/createPesquisa/createPesquisa'));
const ResultadoPesquisa = lazy(() => import('../pages/minhasPesquisas/resultadosPesquisa'));
const EditarPesquisa = lazy(() => import('../pages/minhasPesquisas/editarPesquisa'));
const Admin = lazy(() => import('../pages/admin/Admin'));
const ResponderPesquisa = lazy(() => import('../pages/responderPesquisa/ResponderPesquisa'));

// Mobile Components
const MobileLogin = lazy(() => import('../pages/mobile/MobileLogin'));
const MobileHome = lazy(() => import('../pages/mobile/MobileHome'));
const MobilePerfil = lazy(() => import('../pages/mobile/MobilePerfil'));
const MobileSobreNos = lazy(() => import('../pages/mobile/MobileSobreNos'));
const MobileCreatePesquisa = lazy(() => import('../pages/mobile/MobileCreatePesquisa'));
const MobileResultadosPesquisa = lazy(() => import('../pages/mobile/MobileResultadosPesquisa'));
const MobileEditarPesquisa = lazy(() => import('../pages/mobile/MobileEditarPesquisa'));
const MobileAdmin = lazy(() => import('../pages/mobile/MobileAdmin'));
const MobileResponderPesquisa = lazy(() => import('../pages/mobile/MobileResponderPesquisa'));
const MobileMinhasPesquisas = lazy(() => import('../pages/mobile/MobileMinhasPesquisas'));

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
      <Suspense fallback={<div>Carregando...</div>}>
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
      </Suspense>
    </Router>
  );
};

export default AppRoutes;
