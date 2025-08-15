-- Criar o banco de dados
CREATE DATABASE FastSurveyDb;
GO

-- Usar o banco de dados
USE FastSurveyDb;
GO

-- Criar a tabela de usuários
CREATE TABLE Usuarios (
    Id INT IDENTITY PRIMARY KEY,
    Nome NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    SenhaHash NVARCHAR(255) NOT NULL,
    DataCriacao DATETIME DEFAULT GETDATE()
);
GO

-- Criar a tabela de pesquisas
CREATE TABLE TipoPesquisa (
	Id INT IDENTITY PRIMARY KEY,
	Nome VARCHAR (200),
	Desabilitada BIT DEFAULT 0
)

CREATE TABLE Pesquisas (
    Id INT IDENTITY PRIMARY KEY,
    Titulo NVARCHAR(200) NOT NULL,
    Descricao NVARCHAR(500),
    DataCriacao DATETIME DEFAULT GETDATE(),
    UsuarioId INT NOT NULL,
	TipoPesquisaId INT NOT NULL,
	FOREIGN KEY(TipoPesquisaId) REFERENCES TipoPesquisa(Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
GO


CREATE TABLE TipoPergunta (
	Id INT IDENTITY PRIMARY KEY,
	Nome VARCHAR(200),
	Desabilitada BIT DEFAULT 0
);

-- Criar a tabela de perguntas
CREATE TABLE Perguntas (
    Id INT IDENTITY PRIMARY KEY,
    Texto NVARCHAR(500) NOT NULL,
    PesquisaId INT NOT NULL,
	TipoPerguntaId INT NOT NULL,
	FOREIGN KEY (TipoPerguntaId) REFERENCES TipoPergunta(Id),
    FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id)
);
GO

-- Criar a tabela de respostas
CREATE TABLE Respostas (
    Id INT IDENTITY PRIMARY KEY,
    PerguntaId INT NOT NULL,
    UsuarioId INT NULL,  -- Permite NULL para evitar erro de cascata
    TextoResposta NVARCHAR(1000),
    DataResposta DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PerguntaId) REFERENCES Perguntas(Id),
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) -- Evita erro de ciclo
);
GO

-- Tabelas adicionais para cada tipo de pesquisa, caso seja necessário adicionar campos específicos para cada tipo

-- Pesquisa de Campo (Exemplo de tabela adicional)
CREATE TABLE PesquisaDeCampo (
    Id INT IDENTITY PRIMARY KEY,
    PesquisaId INT NOT NULL,
    Localizacao NVARCHAR(200),  -- Exemplo de campo específico para Pesquisa de Campo
    DataPesquisa DATETIME,
    FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id)
);
GO

-- Pesquisa Científica (Exemplo de tabela adicional)
CREATE TABLE PesquisaCientifica (
    Id INT IDENTITY PRIMARY KEY,
    PesquisaId INT NOT NULL,
    AreaPesquisa NVARCHAR(200),  -- Exemplo de campo específico para Pesquisa Científica
    FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id) 
);
GO

-- Prova/Teste (Exemplo de tabela adicional)
CREATE TABLE Prova (
    Id INT IDENTITY PRIMARY KEY,
    PesquisaId INT NOT NULL,
    Duracao INT,  -- Exemplo de campo para duração de Prova/Teste em minutos
    FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id)
);
GO

-- Pesquisa de Satisfação (Exemplo de tabela adicional)
CREATE TABLE PesquisaDeSatisfacao (
    Id INT IDENTITY PRIMARY KEY,
    PesquisaId INT NOT NULL,
    EscalaSatisfacao NVARCHAR(50),  -- Exemplo de campo específico para Pesquisa de Satisfação
    FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id) 
);
GO

-- Pesquisa de Mercado (Exemplo de tabela adicional)
CREATE TABLE PesquisaDeMercado (
    Id INT IDENTITY PRIMARY KEY,
    PesquisaId INT NOT NULL,
    SegmentoMercado NVARCHAR(200),  -- Exemplo de campo específico para Pesquisa de Mercado
    FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id)  
);
GO

-- Levantamento de Dados Estatísticos (Exemplo de tabela adicional)
CREATE TABLE LevantamentoDadosEstatisticos (
    Id INT IDENTITY PRIMARY KEY,
    PesquisaId INT NOT NULL,
    PeriodoColeta NVARCHAR(200),  -- Exemplo de campo específico para Levantamento de Dados Estatísticos
    FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id)  
);
GO


CREATE TABLE Anexos(
	Id INT IDENTITY PRIMARY KEY,
	Nome VARCHAR(200),
	Extensao VARCHAR(10),
	PesquisaId INT NOT NULL,
	 FOREIGN KEY (PesquisaId) REFERENCES Pesquisas(Id) 
);
