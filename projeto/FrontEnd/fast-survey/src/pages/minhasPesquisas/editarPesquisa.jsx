
import React from 'react';
import { useParams } from 'react-router-dom';

const EditarPesquisa = () => {
  const { id } = useParams();

  return (
    <div>
      <h1>Editar Pesquisa #{id}</h1>
      {/* TODO: Adicionar formulário de edição com dados pré-preenchidos */}
    </div>
  );
};

export default EditarPesquisa;
