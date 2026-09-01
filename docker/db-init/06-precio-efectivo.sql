-- ============================================================================
-- SmartBuy - Precio efectivo computado (ofertas por cantidad).
--
-- precio_efectivo = lo que realmente pagás por unidad comprada:
--   min(precio de oferta directa, precio calculado desde la promo por cantidad
--   tipo "2x1" / "70% la 2da unidad" que OfertaCalculator reconoce en
--   tipo_oferta). Se computa AL INGERIR; para el histórico existe
--   POST /api/Ingesta/RecalcularOfertas (re-ejecutable cuando el parser
--   aprenda patrones nuevos: el crudo nunca se pierde).
-- Las lecturas usan COALESCE(precio_efectivo, LEAST(lista, oferta)): las filas
-- históricas sin computar siguen funcionando.
--
-- Rollback:
--   (recrear la vista sin la columna) y alter table precio drop column precio_efectivo;
-- ============================================================================

alter table precio add column if not exists precio_efectivo numeric(12,2);

comment on column precio.precio_efectivo is 'Precio final por unidad comprada: min(oferta directa, promo por cantidad computada por OfertaCalculator). NULL en filas aún no recalculadas.';

-- La vista suma la columna al final (CREATE OR REPLACE lo permite).
create or replace view precio_vigente as
select distinct on (p.publicacion_id)
       p.publicacion_id,
       p.fecha,
       p.precio_lista,
       p.precio_oferta,
       p.tipo_oferta,
       p.captura_id,
       p.precio_efectivo
from precio p
order by p.publicacion_id, p.fecha desc, p.id desc;
