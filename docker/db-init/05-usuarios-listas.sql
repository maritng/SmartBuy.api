-- ============================================================================
-- SmartBuy - Usuarios y listas guardadas (roadmap post-MVP, paso 1).
--
--   * usuario: login por email; password_hash lleva PBKDF2 con salt por
--     usuario (formato iteraciones.salt.hash) — NUNCA texto plano.
--   * usuario_cadena: preferencia "mis cadenas" del usuario (sin filas =
--     todas las cadenas).
--   * lista / lista_item: las listas guardadas. El borrado de una lista es
--     físico y arrastra sus ítems (cascade): no hay historial que preservar.
--
-- Rollback:
--   drop table lista_item; drop table lista;
--   drop table usuario_cadena; drop table usuario;
-- ============================================================================

create table usuario (
    id              bigint generated always as identity primary key,
    email           text not null unique,
    nombre          text not null,
    password_hash   text not null,
    activo          boolean not null default true,
    fecha_creacion  timestamptz not null default now(),
    ultimo_acceso   timestamptz
);
comment on table usuario is 'Cuentas de la app. El email se guarda normalizado en minúsculas (lo garantiza AuthServices).';
comment on column usuario.password_hash is 'PBKDF2-SHA256 con salt por usuario, formato "iteraciones.saltBase64.hashBase64". Jamás texto plano.';

create table usuario_cadena (
    usuario_id  bigint not null references usuario (id),
    cadena_id   bigint not null references cadena (id),
    primary key (usuario_id, cadena_id)
);
comment on table usuario_cadena is 'Preferencia "mis cadenas" del usuario. Sin filas = todas las cadenas.';

create table lista (
    id                  bigint generated always as identity primary key,
    usuario_id          bigint not null references usuario (id),
    nombre              text not null,
    fecha_creacion      timestamptz not null default now(),
    fecha_modificacion  timestamptz,
    unique (usuario_id, nombre)
);
comment on table lista is 'Listas guardadas ("Compra mensual", "Asado del finde"). El unique por usuario+nombre rechaza duplicados con 23505.';

create table lista_item (
    id          bigint generated always as identity primary key,
    lista_id    bigint not null references lista (id) on delete cascade,
    producto_id bigint not null references producto (id),
    cantidad    integer not null check (cantidad between 1 and 999),
    unique (lista_id, producto_id)
);

create index lista_usuario_idx on lista (usuario_id);
create index lista_item_lista_idx on lista_item (lista_id);
