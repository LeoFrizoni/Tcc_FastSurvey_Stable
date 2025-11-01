// src/components/LazyComponents.js
import { lazy } from 'react';

// Lazy loading para páginas principais
export const HomePage = lazy(() => import('../pages/home/home'));
export const Login = lazy(() => import('../pages/login/login'));
export const CreatePesquisa = lazy(() => import('../pages/createPesquisa/createPesquisa'));
export const ResponderPesquisa = lazy(() => import('../pages/responderPesquisa/responder-pesquisa'));
export const ResultadosPesquisa = lazy(() => import('../pages/minhasPesquisas/resultadosPesquisa'));
export const Admin = lazy(() => import('../pages/admin/Admin'));
export const SobreNos = lazy(() => import('../pages/sobrenos/sobre-nos'));
export const InteractivePlayer = lazy(() => import('../pages/interactive/InteractivePlayer'));
export const ResetPassword = lazy(() => import('../pages/resetPassword/resetPassword'));

// Mobile components
export const MobileHome = lazy(() => import('../pages/mobile/mobileHome/mobileHome'));
export const MobileLogin = lazy(() => import('../pages/mobile/mobileLogin/mobileLogin'));
