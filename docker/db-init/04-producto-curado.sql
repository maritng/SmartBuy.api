-- ============================================================================
-- SmartBuy - Marca de curación del catálogo.
--
-- Los productos creados a mano por el ABM nacen curados (default true). Los
-- generados masivamente desde pendientes por EAN (GenerarDesdePendientes)
-- nacen curado = false: tienen identidad real (EAN) y nombre provisorio (texto
-- de góndola), pero les falta marca, categoría y contenido normalizado. La
-- "cola de curación" del FE (y a futuro el LLM) trabaja sobre curado = false.
--
-- Rollback:
--   alter table producto drop column curado;
-- ============================================================================

alter table producto add column if not exists curado boolean not null default true;

comment on column producto.curado is 'false = generado masivamente desde pendientes (nombre provisorio, sin marca/categoría/contenido): pendiente de curación.';
