-- Categoría de captura: la ruta/categoría del sitio de la cadena de donde el
-- bot trajo la publicación, normalizada por config a etiquetas canónicas
-- (bebidas, almacen, lacteos, limpieza, perfumeria). Es la categorización
-- GRUESA que regala el propio súper: habilita la evolución de precios por tipo
-- de producto sin esperar la curación fina del catálogo (producto.categoria_id,
-- que sigue siendo la taxonomía propia). Se actualiza en cada captura vía el
-- upsert de ingesta; las publicaciones existentes se completan solas con la
-- próxima corrida de los bots.
ALTER TABLE publicacion ADD COLUMN IF NOT EXISTS categoria_captura text;

COMMENT ON COLUMN publicacion.categoria_captura IS
    'Categoría del sitio de la cadena de donde el bot capturó la publicación, normalizada por config (bebidas/almacen/lacteos/limpieza/perfumeria). Categorización gruesa para análisis por tipo; la fina es producto.categoria_id.';
