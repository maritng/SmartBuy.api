-- ============================================================================
-- SmartBuy - Baja lógica de producto.
--
-- Un producto con publicaciones matcheadas tiene FKs apuntándole y el histórico
-- de precios se apoya en él: nunca se borra físico. "activo = false" lo saca de
-- listados y recomendaciones conservando toda la trazabilidad.
--
-- Rollback:
--   alter table producto drop column activo;
-- ============================================================================

alter table producto add column if not exists activo boolean not null default true;

comment on column producto.activo is 'Baja lógica: en false el producto no aparece en listados ni recomendaciones, pero conserva matchings e histórico.';
