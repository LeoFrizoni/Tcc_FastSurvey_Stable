import React, { useState, useEffect, useMemo } from "react";
import axios from "axios";
import styles from "./modal-criar-pesquisa.module.css";
import { API_BASE_URL } from "../../config";

export default function ModalCriarPesquisa({ onConfirm }) {
  const [titulo, setTitulo] = useState("");
  const [descricao, setDescricao] = useState("");
  const [tipoSelecionado, setTipoSelecionado] = useState("");
  const [expiracaoModo, setExpiracaoModo] = useState("sem_limite"); // "sem_limite" | "com_tempo"
  const [horasValidade, setHorasValidade] = useState(1);

  const [tipos, setTipos] = useState([]);
  const [erros, setErros] = useState({});
  const [carregandoTipos, setCarregandoTipos] = useState(false);

  useEffect(() => {
    const controller = new AbortController();

    async function carregarTipos() {
      try {
        setCarregandoTipos(true);
        const url = `${API_BASE_URL}/api/TipoPesquisa/ListarTipoPesquisa`;
        const res = await axios.get(url, { signal: controller.signal });
        setTipos(res?.data ?? []);
      } catch (err) {
        if (axios.isCancel(err)) return;
        console.error("Erro ao buscar tipos:", err);
        setTipos([]);
      } finally {
        setCarregandoTipos(false);
      }
    }
    carregarTipos();

    return () => controller.abort();
  }, []);

  const itensTipos = useMemo(() => {
    return (tipos ?? [])
      .map((t) => {
        const id = t?.TipoPesquisaId ?? t?.tipopesquisaid ?? t?.tipoPesquisaId ?? t?.id ?? t?.Id ?? null;
        const nome =
          t?.tipopesquisa1 ??
          t?.Tipopesquisa1 ??
          t?.tipopesquisa ??
          t?.tipoPesquisa ??
          t?.Nome ??
          t?.nome ??
          "Sem nome";
        const desabilitado = t?.desabilitado ?? t?.Desabilitado ?? false;
        return { id, nome, desabilitado };
      })
      .filter((t) => t.id != null && t.desabilitado !== true);
  }, [tipos]);

  const validar = () => {
    const novos = {};
    if (!titulo?.trim()) novos.titulo = "Título é obrigatório.";
    if (!descricao?.trim()) novos.descricao = "Descrição é obrigatória.";
    if (!tipoSelecionado) novos.tipo = "Selecione o tipo de pesquisa.";

    if (expiracaoModo === "com_tempo") {
      const n = Number(horasValidade);
      if (!Number.isFinite(n) || n <= 0) {
        novos.horasValidade = "Informe um número de horas válido.";
      }
    }
    setErros(novos);
    return Object.keys(novos).length === 0;
  };

  const handleConfirm = () => {
    if (!validar()) return;

    const payload = {
      titulo: titulo.trim(),
      descricao: descricao.trim(),
      tipo: parseInt(tipoSelecionado, 10),
      // Expiração:
      expiraSemLimite: expiracaoModo === "sem_limite",
      horasValidade: expiracaoModo === "com_tempo" ? parseInt(horasValidade, 10) : null
    };

    // Observação:
    // No componente pai (ou service de criação), mapeie para o contrato do backend:
    // - Se expiraSemLimite === true => TempoExpiracaoHoras = null
    // - Se expiraSemLimite === false => TempoExpiracaoHoras = horasValidade
    // E, se houver opção de QR Code, é um bom ponto para incluir (ex.: gerarQRCode: true/false).

    onConfirm?.(payload);
  };

  return (
    <div className={styles["modal-overlay"]}>
      <div className={styles["modal-criar"]} role="dialog" aria-modal="true" aria-labelledby="titulo-modal">
        <h2 id="titulo-modal">Nova Pesquisa</h2>

        <div className={styles["form-group"]}>
          <label htmlFor="titulo">Título</label>
          <input
            id="titulo"
            type="text"
            value={titulo}
            onChange={(e) => setTitulo(e.target.value)}
            placeholder="Digite o título da pesquisa"
            style={erros.titulo ? { borderColor: "#e74c3c" } : {}}
          />
          {erros.titulo && <span className={styles.erro}>{erros.titulo}</span>}
        </div>

        <div className={styles["form-group"]}>
          <label htmlFor="descricao">Descrição</label>
          <textarea
            id="descricao"
            value={descricao}
            onChange={(e) => setDescricao(e.target.value)}
            placeholder="Descreva a pesquisa brevemente"
            style={erros.descricao ? { borderColor: "#e74c3c" } : {}}
                      />
            {erros.descricao && <span className={styles.erro}>{erros.descricao}</span>}
          </div>

          <div className={styles["form-group"]}>
          <label htmlFor="tipo">Tipo de Pesquisa</label>
          <select
            id="tipo"
            value={tipoSelecionado}
            onChange={(e) => setTipoSelecionado(e.target.value)}
            style={erros.tipo ? { borderColor: "#e74c3c" } : {}}
            disabled={carregandoTipos}
          >
            <option value="">{carregandoTipos ? "Carregando..." : "Selecione um tipo"}</option>
            {itensTipos.map((t) => (
              <option key={t.id} value={t.id}>{t.nome}</option>
            ))}
          </select>
          {erros.tipo && <span className={styles.erro}>{erros.tipo}</span>}
        </div>

        <div className={styles["form-group"]}>
          <label>Disponibilidade</label>
          <div className={styles["radio-row"]}>
            <label>
              <input
                type="radio"
                name="expiracao"
                value="sem_limite"
                checked={expiracaoModo === "sem_limite"}
                onChange={() => setExpiracaoModo("sem_limite")}
              />
              <span className={styles["radio-icon"]}>∞</span>
              <span className={styles["radio-text"]}>Sem expiração</span>
            </label>
            <label>
              <input
                type="radio"
                name="expiracao"
                value="com_tempo"
                checked={expiracaoModo === "com_tempo"}
                onChange={() => setExpiracaoModo("com_tempo")}
              />
              <span className={styles["radio-icon"]}>⏰</span>
              <span className={styles["radio-text"]}>Com tempo limite</span>
            </label>
          </div>
        </div>

        {expiracaoModo === "com_tempo" && (
          <div className={styles["form-group"]}>
            <label htmlFor="horas">Validade</label>
            <div className={styles["horas-input-container"]}>
              <input
                id="horas"
                type="number"
                min="1"
                value={horasValidade}
                onChange={(e) => setHorasValidade(e.target.value)}
                placeholder="Ex.: 24"
                style={erros.horasValidade ? { borderColor: "#e74c3c" } : {}}
              />
            </div>
            {erros.horasValidade && <span className={styles.erro}>{erros.horasValidade}</span>}
          </div>
        )}

        <button onClick={handleConfirm} disabled={carregandoTipos}>
          {carregandoTipos ? "Carregando..." : "Começar"}
        </button>
      </div>
    </div>
  );
}
