// src/routes/routes.jsx
import React, { Suspense, lazy } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import PrivateRoute from '../components/utilities/PrivateRoute';

const Login = lazy(() => import('../pages/login/login'));
const HomePage = lazy(() => import('../pages/home/home'));
const Perfil = lazy(() => import('../pages/perfil/perfil'));
const SobreNos = lazy(() => import('../pages/sobrenos/sobre-nos'));
const CriarPesquisa = lazy(() => import('../pages/createPesquisa/createPesquisa'));
const ResultadoPesquisa = lazy(() => import('../pages/minhasPesquisas/resultadosPesquisa'));
const EditarPesquisa = lazy(() => import('../pages/minhasPesquisas/editarPesquisa'));
const Admin = lazy(() => import('../pages/admin/admin'));
const ResponderPesquisa = lazy(() => import('../pages/responderPesquisa/responder-pesquisa'));

// Mobile Components
const MobileLogin = lazy(() => import('../pages/mobile/mobile-login'));
const MobileHome = lazy(() => import('../pages/mobile/mobile-home'));
const MobilePerfil = lazy(() => import('../pages/mobile/mobile-perfil'));
const MobileSobreNos = lazy(() => import('../pages/mobile/mobile-sobre-nos'));
const MobileCreatePesquisa = lazy(() => import('../pages/mobile/mobile-create-pesquisa'));
const MobileResultadosPesquisa = lazy(() => import('../pages/mobile/mobile-resultados-pesquisa'));
const MobileEditarPesquisa = lazy(() => import('../pages/mobile/mobile-editar-pesquisa'));
const MobileAdmin = lazy(() => import('../pages/mobile/mobile-admin'));
const MobileResponderPesquisa = lazy(() => import('../pages/mobile/mobile-responder-pesquisa'));
const MobileMinhasPesquisas = lazy(() => import('../pages/mobile/mobile-minhas-pesquisas'));

// Game Pages
const GameHost = lazy(() => import('../pages/game/GameHost'));
const GamePlayer = lazy(() => import('../pages/game/GamePlayer'));
const GameJoin = lazy(() => import('../pages/game/GameJoin'));

// Analytics Pages
const AnalyticsDashboard = lazy(() => import('../pages/analytics/AnalyticsDashboard'));

const PrivateRouteAdmin = ({ children }) => {
  const tipoUsuarioId = parseInt(localStorage.getItem('tipousuarioid') || localStorage.getItem('TipoUsuarioId'), 10);
  const token = localStorage.getItem('token');
  if (!token) return <Navigate to="/login" replace />;
  return tipoUsuarioId === 15 ? children : <Navigate to="/home" replace />;
};

// Detecta mobile
function isMobile() {
  return /Mobi|Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
    window.navigator.userAgent
  );
}

const AppRoutes = () => {
  const mobile = isMobile();

  return (
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

        <Route
          path="/responder/:id"
          element={mobile ? <MobileResponderPesquisa /> : <ResponderPesquisa />}
        />

        <Route
          path="/admin"
          element={
            <PrivateRouteAdmin>
              {mobile ? <MobileAdmin /> : <Admin />}
            </PrivateRouteAdmin>
          }
        />

        {/* Game Routes */}
        <Route
          path="/game/host/:sessionId"
          element={
            <PrivateRoute>
              <GameHost />
            </PrivateRoute>
          }
        />
        <Route
          path="/game/player/:sessionId"
          element={<GamePlayer />}
        />
        <Route
          path="/game/join/:accessCode"
          element={<GameJoin />}
        />

        {/* Analytics Routes */}
        <Route
          path="/analytics"
          element={
            <PrivateRoute>
              <AnalyticsDashboard />
            </PrivateRoute>
          }
        />

        {/* Redirecionamentos */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </Suspense>
  );
};

export default AppRoutes;
