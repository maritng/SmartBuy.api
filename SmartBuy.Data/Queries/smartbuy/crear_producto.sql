-- Alta en el catálogo maestro. El UNIQUE de producto.ean rechaza el duplicado
-- con 23505: ProductoServices lo traduce a "ya existe un producto con ese EAN"
-- (única defensa real contra dos altas simultáneas).
INSERT INTO producto (nombre, marca_id, categoria_id, contenido_valor, contenido_unidad, ean)
VALUES (@nombre,
        CAST(@marcaid AS bigint),
        CAST(@categoriaid AS bigint),
        CAST(@contenidovalor AS numeric),
        CAST(@contenidounidad AS text),
        CAST(@ean AS text))
RETURNING id
