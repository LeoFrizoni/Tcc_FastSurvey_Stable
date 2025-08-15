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

const PrivateRouteAdmin = ({ children }) => {
  const tipoUsuarioId = parseInt(localStorage.getItem('tipousuarioid'), 10);
  const token = localStorage.getItem('token');

  if (!token) {
    return <Navigate to="/login" />;
  }

  return tipoUsuarioId === 15 ? children : <Navigate to="/home" />;
};

const AppRoutes = () => {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />

        <Route
          path="/home"
          element={
            <PrivateRoute>
              <HomePage />
            </PrivateRoute>
          }
        />
        <Route
          path="/perfil"
          element={
            <PrivateRoute>
              <Perfil />
            </PrivateRoute>
          }
        />
        <Route
          path="/sobre-nos"
          element={
            <PrivateRoute>
              <SobreNos />
            </PrivateRoute>
          }
        />
        <Route
          path="/criar"
          element={
            <PrivateRoute>
              <CriarPesquisa />
            </PrivateRoute>
          }
        />
        <Route
          path="/minhas-pesquisas/editar/:id"
          element={
            <PrivateRoute>
              <EditarPesquisa />
            </PrivateRoute>
          }
        />
        <Route
          path="/minhas-pesquisas/resultado/:id"
          element={
            <PrivateRoute>
              <ResultadoPesquisa />
            </PrivateRoute>
          }
        />

        
        <Route path="/responder/:id" element={<ResponderPesquisa />} />

        <Route
          path="/admin"
          element={
            <PrivateRouteAdmin>
              <Admin />
            </PrivateRouteAdmin>
          }
        />

        <Route path="/" element={<Navigate to="/login" />} />
      </Routes>
    </Router>
  );
};

export default AppRoutes;
