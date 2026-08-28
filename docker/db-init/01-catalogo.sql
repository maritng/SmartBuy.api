-- ============================================================================
-- SmartBuy - Esquema inicial: catálogo maestro, publicaciones y precios.
--
-- Principios del modelo:
--   1. "producto" (catálogo maestro) está separado de "publicacion" (lo que
--      cada cadena publica). Los precios cuelgan de la publicación, nunca del
--      producto: un matching mal hecho se corrige re-apuntando la publicación
--      sin tocar el histórico.
--   2. "precio" es append-only: cada captura inserta filas nuevas, nada se
--      actualiza. El histórico queda gratis.
--   3. El EAN es el ancla del matching automático; el matching por nombre
--      queda registrado con su grado de confianza.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Catálogo maestro
-- ----------------------------------------------------------------------------

create table marca (
    id              bigint generated always as identity primary key,
    nombre          text not null unique,
    fecha_creacion  timestamptz not null default now()
);
comment on table marca is 'Marcas del catálogo maestro (Coca-Cola, La Serenísima, ...).';

create table categoria (
    id              bigint generated always as identity primary key,
    nombre          text not null,
    padre_id        bigint references categoria (id),
    fecha_creacion  timestamptz not null default now(),
    unique nulls not distinct (nombre, padre_id)
);
comment on table categoria is 'Categorías de producto. padre_id permite jerarquía (Bebidas > Gaseosas); en null es categoría raíz.';

create table producto (
    id                  bigint generated always as identity primary key,
    nombre              text not null,
    marca_id            bigint references marca (id),
    categoria_id        bigint references categoria (id),
    contenido_valor     numeric(10,3),
    contenido_unidad    text check (contenido_unidad in ('L', 'ml', 'kg', 'g', 'un')),
    ean                 text unique,
    fecha_creacion      timestamptz not null default now(),
    fecha_modificacion  timestamptz,
    constraint producto_contenido_completo
        check ((contenido_valor is null) = (contenido_unidad is null))
);
comment on table producto is 'Catálogo maestro: el producto tal como existe en el mundo, independiente de cómo lo publique cada cadena.';
comment on column producto.contenido_valor is 'Junto con contenido_unidad permite comparar precio por unidad entre presentaciones distintas.';
comment on column producto.ean is 'Código de barras. Único cuando existe; nullable para productos sin código (ej. verdulería).';

-- ----------------------------------------------------------------------------
-- Lado de los supermercados
-- ----------------------------------------------------------------------------

create table cadena (
    id              bigint generated always as identity primary key,
    nombre          text not null unique,
    sitio_web       text,
    fecha_creacion  timestamptz not null default now()
);
comment on table cadena is 'Cadenas de supermercados de las que se capturan precios (Carrefour, Coto, Día, ...).';

create table publicacion (
    id                  bigint generated always as identity primary key,
    cadena_id           bigint not null references cadena (id),
    codigo_externo      text not null,
    nombre_publicado    text not null,
    ean_publicado       text,
    url                 text,
    producto_id         bigint references producto (id),
    estado_matching     text not null default 'pendiente'
                        check (estado_matching in ('pendiente', 'auto_ean', 'auto_nombre', 'manual', 'descartada')),
    confianza           numeric(4,3) check (confianza between 0 and 1),
    fecha_creacion      timestamptz not null default now(),
    fecha_modificacion  timestamptz,
    unique (cadena_id, codigo_externo),
    -- Una publicación matcheada apunta a un producto; una pendiente o descartada, no.
    constraint publicacion_matching_coherente
        check ((producto_id is not null) = (estado_matching in ('auto_ean', 'auto_nombre', 'manual')))
);
comment on table publicacion is 'Lo que una cadena publica en su tienda: el texto crudo y su vínculo (matching) con el producto maestro.';
comment on column publicacion.codigo_externo is 'SKU o id del producto en el sitio de la cadena. Clave de upsert junto con cadena_id.';
comment on column publicacion.nombre_publicado is 'Nombre tal cual lo publica la cadena, sin normalizar. Nunca se pisa: es la evidencia del matching.';
comment on column publicacion.confianza is 'Confianza 0-1 del matching. Relevante para estado auto_nombre; en auto_ean es implícitamente 1.';

create index publicacion_producto_idx on publicacion (producto_id) where producto_id is not null;
create index publicacion_ean_idx on publicacion (ean_publicado) where ean_publicado is not null;
create index publicacion_pendientes_idx on publicacion (cadena_id) where estado_matching = 'pendiente';

-- ----------------------------------------------------------------------------
-- Capturas y precios
-- ----------------------------------------------------------------------------

create table captura (
    id              bigint generated always as identity primary key,
    cadena_id       bigint not null references cadena (id),
    fuente          text not null check (fuente in ('web', 'mail', 'api', 'manual')),
    estado          text not null default 'en_proceso'
                    check (estado in ('en_proceso', 'ok', 'error')),
    fecha_inicio    timestamptz not null default now(),
    fecha_fin       timestamptz,
    cant_items      integer,
    error_detalle   text
);
comment on table captura is 'Una corrida de recolección de precios (un bot, un mail procesado, una carga manual). Da trazabilidad a cada precio.';

-- Particionada por mes: el volumen esperado (varias cadenas x decenas de miles
-- de publicaciones x 1 captura diaria) crece de a millones de filas por mes.
create table precio (
    id              bigint generated always as identity,
    publicacion_id  bigint not null references publicacion (id),
    captura_id      bigint not null references captura (id),
    fecha           date not null,
    precio_lista    numeric(12,2) not null check (precio_lista >= 0),
    precio_oferta   numeric(12,2) check (precio_oferta >= 0),
    tipo_oferta     text,
    primary key (id, fecha)
) partition by range (fecha);
comment on table precio is 'Histórico de precios, append-only: una fila por publicación y captura. La oferta se guarda cruda (tipo_oferta) y el precio efectivo se calcula al leer.';
comment on column precio.tipo_oferta is 'Texto crudo de la promo ("2x1", "70% 2da unidad", "precio con cuenta"). El dato crudo nunca miente; la fórmula de normalización puede mejorar.';

create index precio_publicacion_fecha_idx on precio (publicacion_id, fecha desc);

-- Particiones mensuales hasta fin de 2027 + default como red de seguridad.
do $$
declare
    mes date := date '2026-08-01';
begin
    while mes < date '2028-01-01' loop
        execute format(
            'create table precio_%s partition of precio for values from (%L) to (%L)',
            to_char(mes, 'YYYY_MM'), mes, mes + interval '1 month'
        );
        mes := mes + interval '1 month';
    end loop;
end $$;
create table precio_default partition of precio default;
comment on table precio_default is 'Red de seguridad: recibe filas fuera del rango de particiones creadas. Debería estar siempre vacía; si tiene filas, faltan particiones.';

-- ----------------------------------------------------------------------------
-- Vista de conveniencia: el último precio conocido de cada publicación.
-- Es la base de la consulta central de la app (mejor lugar por producto).
-- ----------------------------------------------------------------------------
create view precio_vigente as
select distinct on (p.publicacion_id)
       p.publicacion_id,
       p.fecha,
       p.precio_lista,
       p.precio_oferta,
       p.tipo_oferta,
       p.captura_id
from precio p
order by p.publicacion_id, p.fecha desc, p.id desc;
comment on view precio_vigente is 'Último precio conocido por publicación. Consulta central: producto -> publicaciones matcheadas -> precio_vigente -> mínimo.';
