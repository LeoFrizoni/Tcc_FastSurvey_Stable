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

// Interactive Session
const InteractiveHost = lazy(() => import('../pages/interactive/InteractiveHost'));
const InteractivePlayer = lazy(() => import('../pages/interactive/InteractivePlayer'));
const InteractiveJoin = lazy(() => import('../pages/interactive/InteractiveJoin'));

// Analytics
const AnalyticsDashboard = lazy(() => import('../pages/analytics/AnalyticsDashboard'));

const PrivateRouteAdmin = ({ children }) => {
  let token = localStorage.getItem('token');
  let tipoUsuarioId = Number(localStorage.getItem('tipousuarioid'));

  if (!token || !tipoUsuarioId) {
    token = sessionStorage.getItem('token');
    tipoUsuarioId = Number(sessionStorage.getItem('tipousuarioid'));
  }

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (tipoUsuarioId === 15) {
    return children;
  }

  return <Navigate to="/home" replace />;
};

const RootRedirect = () => {
  const token = localStorage.getItem('token');
  const tipoUsuarioId = Number(localStorage.getItem('tipousuarioid'));
  
  // Se tem token válido e é admin, redireciona para /admin
  if (token && tipoUsuarioId === 15) {
    return <Navigate to="/admin" replace />;
  }
  
  // Se tem token válido mas não é admin, redireciona para /home
  if (token && tipoUsuarioId !== 15) {
    return <Navigate to="/home" replace />;
  }
  
  // Se não tem token, vai para login
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
    const [shouldRedirect, setShouldRedirect] = React.useState(null);
    
    React.useEffect(() => {
      clearInvalidAuth(); // Limpa tokens inválidos
      
      // Verificar primeiro localStorage, depois sessionStorage
      let token = localStorage.getItem('token');
      let tipoUsuarioId = Number(localStorage.getItem('tipousuarioid'));
      
      if (!token || !tipoUsuarioId) {
        token = sessionStorage.getItem('token');
        tipoUsuarioId = Number(sessionStorage.getItem('tipousuarioid'));
      }
      
      // Se tem token válido e é admin, redireciona para /admin
      if (token && tipoUsuarioId === 15) {
        console.log('🔍 LoginGuard - Usuário admin logado, redirecionando para /admin');
        setShouldRedirect('/admin');
        return;
      }
      
      // Se tem token válido mas não é admin, redireciona para /home
      if (token && tipoUsuarioId !== 15 && token) {
        console.log('🔍 LoginGuard - Usuário logado, redirecionando para /home');
        setShouldRedirect('/home');
        return;
      }
      
      // Se não tem token, mostra página de login
      console.log('🔍 LoginGuard - Usuário não logado, mostrando página de login');
      setShouldRedirect(null);
    }, []);
    
    if (shouldRedirect) {
      return <Navigate to={shouldRedirect} replace />;
    }
    
    return LoginElement;
  };

  // Função para limpar tokens inválidos
  const clearInvalidAuth = () => {
    // Verificar localStorage
    let token = localStorage.getItem('token');
    if (token) {
      try {
        const parts = token.split('.');
        if (parts.length === 3) {
          const payload = parts[1];
          const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
          const parsed = JSON.parse(decoded);
          
          // Se token expirado, limpa localStorage
          if (parsed?.exp && Date.now() / 1000 > parsed.exp) {
            logout();
            return;
          }
        }
      } catch (error) {
        logout();
        return;
      }
    }
    
    // Verificar sessionStorage
    token = sessionStorage.getItem('token');
    if (token) {
      try {
        const parts = token.split('.');
        if (parts.length === 3) {
          const payload = parts[1];
          const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
          const parsed = JSON.parse(decoded);
          
          // Se token expirado, limpa sessionStorage
          if (parsed?.exp && Date.now() / 1000 > parsed.exp) {
            sessionStorage.removeItem('token');
            sessionStorage.removeItem('userId');
            sessionStorage.removeItem('tipousuarioid');
          }
        }
      } catch (error) {
        sessionStorage.removeItem('token');
        sessionStorage.removeItem('userId');
        sessionStorage.removeItem('tipousuarioid');
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

        {/* Público por ID (conforme backend público) */}
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

        {/* Interactive Session */}
        <Route
          path="/interactive/host/:sessionId"
          element={
            <PrivateRoute>
              <InteractiveHost />
            </PrivateRoute>
          }
        />
        <Route path="/interactive/player/:sessionId" element={<InteractivePlayer />} />
        <Route path="/interactive/join/:accessCode" element={<InteractiveJoin />} />

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
