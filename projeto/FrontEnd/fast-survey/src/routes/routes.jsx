 // src/routes/routes.jsx
import React, { Suspense, lazy } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { logout } from '../utils/auth';
import PrivateRoute from '../components/utilities/PrivateRoute';

const Login = lazy(() => import('../pages/login/login'));
const ResetPassword = lazy(() => import('../pages/resetPassword/resetPassword'));
const HomePage = lazy(() => import('../pages/home/home'));
const Perfil = lazy(() => import('../pages/perfil/perfil'));
const SobreNos = lazy(() => import('../pages/sobrenos/sobre-nos'));
const CriarPesquisa = lazy(() => import('../pages/createPesquisa/createPesquisa'));
const ResultadoPesquisa = lazy(() => import('../pages/minhasPesquisas/resultadosPesquisa'));
const EditarPesquisa = lazy(() => import('../pages/minhasPesquisas/editarPesquisa'));
const Admin = lazy(() => import('../pages/admin/Admin'));
const ResponderPesquisa = lazy(() => import('../pages/responderPesquisa/responder-pesquisa'));

// Mobile
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

// Game
const GameHost = lazy(() => import('../pages/game/GameHost'));
const GamePlayer = lazy(() => import('../pages/game/GamePlayer'));
const GameJoin = lazy(() => import('../pages/game/GameJoin'));

// Analytics
const AnalyticsDashboard = lazy(() => import('../pages/analytics/AnalyticsDashboard'));

const PrivateRouteAdmin = ({ children }) => {
  const token = localStorage.getItem('token');
  const tipoUsuarioId = Number(localStorage.getItem('tipousuarioid'));
  
  console.log('🔍 PrivateRouteAdmin - Token:', token ? 'existe' : 'não existe');
  console.log('🔍 PrivateRouteAdmin - TipoUsuarioId:', tipoUsuarioId);
  console.log('🔍 PrivateRouteAdmin - TipoUsuarioId === 15:', tipoUsuarioId === 15);
  
  if (!token) {
    console.log('🔍 PrivateRouteAdmin - Sem token, redirecionando para /login');
    return <Navigate to="/login" replace />;
  }
  
  if (tipoUsuarioId === 15) {
    console.log('🔍 PrivateRouteAdmin - Usuário é admin, renderizando children');
    return children;
  } else {
    console.log('🔍 PrivateRouteAdmin - Usuário não é admin (tipo:', tipoUsuarioId, '), redirecionando para /home');
    return <Navigate to="/home" replace />;
  }
};

const RootRedirect = () => {
  console.log('🔍 RootRedirect - Verificando redirecionamento da rota raiz');
  
  const token = localStorage.getItem('token');
  const tipoUsuarioId = Number(localStorage.getItem('tipousuarioid'));
  
  // Se tem token válido e é admin, redireciona para /admin
  if (token && tipoUsuarioId === 15) {
    console.log('🔍 RootRedirect - Usuário admin logado, redirecionando para /admin');
    return <Navigate to="/admin" replace />;
  }
  
  // Se tem token válido mas não é admin, redireciona para /home
  if (token && tipoUsuarioId !== 15) {
    console.log('🔍 RootRedirect - Usuário logado, redirecionando para /home');
    return <Navigate to="/home" replace />;
  }
  
  // Se não tem token, vai para login
  console.log('🔍 RootRedirect - Usuário não logado, redirecionando para /login');
  return <Navigate to="/login" replace />;
};

function isMobile() {
  return /Mobi|Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
    window.navigator.userAgent
  );
}

const AppRoutes = () => {
  const mobile = isMobile();

  const LoginElement = mobile ? <MobileLogin /> : <Login />;
  const HomeElement = mobile ? <MobileHome /> : <HomePage />;

  // Verificar se usuário já está logado e redirecionar adequadamente
  const LoginGuard = () => {
    console.log('🔍 LoginGuard - Verificando se usuário já está logado');
    clearInvalidAuth(); // Limpa tokens inválidos
    
    const token = localStorage.getItem('token');
    const tipoUsuarioId = Number(localStorage.getItem('tipousuarioid'));
    
    // Se tem token válido e é admin, redireciona para /admin
    if (token && tipoUsuarioId === 15) {
      console.log('🔍 LoginGuard - Usuário admin logado, redirecionando para /admin');
      return <Navigate to="/admin" replace />;
    }
    
    // Se tem token válido mas não é admin, redireciona para /home
    if (token && tipoUsuarioId !== 15) {
      console.log('🔍 LoginGuard - Usuário logado, redirecionando para /home');
      return <Navigate to="/home" replace />;
    }
    
    // Se não tem token, mostra página de login
    console.log('🔍 LoginGuard - Usuário não logado, mostrando página de login');
    
    return LoginElement;
  };

  // Função para limpar localStorage se necessário
  const clearInvalidAuth = () => {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const parts = token.split('.');
        if (parts.length === 3) {
          const payload = parts[1];
          const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
          const parsed = JSON.parse(decoded);
          
          // Se token expirado, limpa localStorage
          if (parsed?.exp && Date.now() / 1000 > parsed.exp) {
            console.log('🔍 Limpando localStorage - token expirado');
            logout();
          }
        }
      } catch (error) {
        console.log('🔍 Limpando localStorage - token inválido');
        logout();
      }
    }
  };

  return (
    <Suspense fallback={<div>Carregando...</div>}>
      <Routes>
        <Route path="/login" element={<LoginGuard />} />

        <Route path="/reset-password" element={<ResetPassword />} />

        <Route
          path="/home"
          element={
            <PrivateRoute>
              {HomeElement}
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
              {
                mobile
                  ? <MobileMinhasPesquisas />
                  // TODO: quando houver a página desktop de listagem, importe aqui:
                  // : <MinhasPesquisas />
                  : <ResultadoPesquisa /> // placeholder atual
              }
            </PrivateRoute>
          }
        />

        {/* Público por slug (conforme backend público) */}
        <Route
          path="/responder/:slug"
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

        {/* Game */}
        <Route
          path="/game/host/:sessionId"
          element={
            <PrivateRoute>
              <GameHost />
            </PrivateRoute>
          }
        />
        <Route path="/game/player/:sessionId" element={<GamePlayer />} />
        <Route path="/game/join/:accessCode" element={<GameJoin />} />

        {/* Analytics */}
        <Route
          path="/analytics"
          element={
            <PrivateRoute>
              <AnalyticsDashboard />
            </PrivateRoute>
          }
        />

        {/* Redirecionamentos */}
        <Route path="/" element={<RootRedirect />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </Suspense>
  );
};

export default AppRoutes;
