import MultiplaEscolha from '../components/perguntas/multiplaEscolha';
import Discursiva from '../components/perguntas/discursiva';

export const tipoParaComponente = {
    multiplaEscolha: MultiplaEscolha, discursiva: Discursiva,
};

export function getComponentePorTipo(tipo) {
    return tipoParaComponente[tipo.toLowerCase()] || null;
}