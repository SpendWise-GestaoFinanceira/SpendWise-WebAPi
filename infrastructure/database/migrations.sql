CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Usuarios" (
    "Id" uuid NOT NULL,
    "Nome" character varying(200) NOT NULL,
    "Email" character varying(255) NOT NULL,
    "PasswordHash" character varying(500) NOT NULL,
    "RendaMensal" numeric(18,2) NOT NULL,
    "IsAtivo" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Usuarios" PRIMARY KEY ("Id")
);

CREATE TABLE "Categorias" (
    "Id" uuid NOT NULL,
    "Nome" character varying(100) NOT NULL,
    "Descricao" character varying(500),
    "Tipo" integer NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "IsAtiva" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Categorias" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Categorias_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transacoes" (
    "Id" uuid NOT NULL,
    "Descricao" character varying(200) NOT NULL,
    "Valor" numeric(18,2) NOT NULL,
    "Moeda" character varying(3) NOT NULL DEFAULT 'BRL',
    "DataTransacao" timestamp with time zone NOT NULL,
    "Tipo" integer NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "CategoriaId" uuid NOT NULL,
    "Observacoes" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Transacoes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Transacoes_Categorias_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES "Categorias" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Transacoes_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_Categorias_Usuario_Nome" ON "Categorias" ("UsuarioId", "Nome");

CREATE INDEX "IX_Transacoes_CategoriaId" ON "Transacoes" ("CategoriaId");

CREATE INDEX "IX_Transacoes_DataTransacao" ON "Transacoes" ("DataTransacao");

CREATE INDEX "IX_Transacoes_UsuarioId" ON "Transacoes" ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250825022453_InitialCreate', '9.0.8');

CREATE UNIQUE INDEX "IX_Usuarios_Email" ON "Usuarios" ("Email");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250825024541_UpdateWithEmailIndex', '9.0.8');

ALTER TABLE "Usuarios" RENAME COLUMN "PasswordHash" TO "Senha";

ALTER TABLE "Usuarios" ADD "PasswordResetToken" character varying(100);

ALTER TABLE "Usuarios" ADD "PasswordResetTokenExpiry" timestamp with time zone;

ALTER TABLE "Categorias" ADD "LimiteMoeda" character varying(3) DEFAULT 'BRL';

ALTER TABLE "Categorias" ADD "LimiteValor" numeric(18,2);

ALTER TABLE "Categorias" ADD "Prioridade" integer NOT NULL DEFAULT 0;

CREATE TABLE "FechamentosMensais" (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "UsuarioId" uuid NOT NULL,
    "AnoMes" character varying(7) NOT NULL,
    "DataFechamento" timestamp with time zone NOT NULL,
    "Status" integer NOT NULL,
    "TotalReceitas" numeric(18,2) NOT NULL,
    "TotalDespesas" numeric(18,2) NOT NULL,
    "SaldoFinal" numeric(18,2) NOT NULL,
    "Observacoes" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    CONSTRAINT "PK_FechamentosMensais" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FechamentosMensais_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);
COMMENT ON COLUMN "FechamentosMensais"."AnoMes" IS 'Formato YYYY-MM';

CREATE TABLE orcamentos_mensais (
    id uuid NOT NULL,
    usuario_id uuid NOT NULL,
    ano_mes character varying(7) NOT NULL,
    "Valor" numeric(14,2) NOT NULL,
    "Moeda" character varying(3) NOT NULL DEFAULT 'BRL',
    criado_em timestamp with time zone NOT NULL,
    atualizado_em timestamp with time zone,
    CONSTRAINT "PK_orcamentos_mensais" PRIMARY KEY (id),
    CONSTRAINT ck_orcamento_anomes_formato CHECK (ano_mes ~ '^[0-9]{4}-(0[1-9]|1[0-2])$'),
    CONSTRAINT ck_orcamento_valor_positivo CHECK (valor >= 0),
    CONSTRAINT "FK_orcamentos_mensais_Usuarios_usuario_id" FOREIGN KEY (usuario_id) REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_FechamentosMensais_AnoMes" ON "FechamentosMensais" ("AnoMes");

CREATE INDEX "IX_FechamentosMensais_Status" ON "FechamentosMensais" ("Status");

CREATE UNIQUE INDEX "IX_FechamentosMensais_Usuario_AnoMes" ON "FechamentosMensais" ("UsuarioId", "AnoMes");

CREATE INDEX "IX_FechamentosMensais_UsuarioId" ON "FechamentosMensais" ("UsuarioId");

CREATE INDEX ix_orcamento_anomes ON orcamentos_mensais (ano_mes);

CREATE UNIQUE INDEX "IX_OrcamentosMensais_Usuario_Periodo" ON orcamentos_mensais (usuario_id, ano_mes);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250903020103_AddPasswordResetFields', '9.0.8');

CREATE TABLE "Metas" (
    "Id" uuid NOT NULL,
    "Nome" character varying(100) NOT NULL,
    "Descricao" character varying(500) NOT NULL,
    "ValorObjetivo" numeric(18,2) NOT NULL,
    "ValorObjetivo_Moeda" text NOT NULL,
    "Prazo" date NOT NULL,
    "ValorAtual" numeric(18,2) NOT NULL,
    "ValorAtual_Moeda" text NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "IsAtiva" boolean NOT NULL DEFAULT TRUE,
    "DataAlcancada" date,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Metas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Metas_Usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Metas_DataAlcancada" ON "Metas" ("DataAlcancada");

CREATE INDEX "IX_Metas_UsuarioId" ON "Metas" ("UsuarioId");

CREATE INDEX "IX_Metas_UsuarioId_IsAtiva" ON "Metas" ("UsuarioId", "IsAtiva");

CREATE INDEX "IX_Metas_UsuarioId_Prazo" ON "Metas" ("UsuarioId", "Prazo");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250903130317_CreateMetasTable', '9.0.8');

COMMIT;

