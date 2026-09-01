-- Paso 2 del guardado: carga los ítems. @items viaja como CSV de pares
-- "productoId:cantidad" (ej. "2:4,7:3"). La FK a producto rechaza ids
-- inexistentes con 23503.
INSERT INTO lista_item (lista_id, producto_id, cantidad)
SELECT @listaid,
       split_part(par, ':', 1)::bigint,
       split_part(par, ':', 2)::int
FROM unnest(string_to_array(CAST(@items AS text), ',')) AS par
WHERE CAST(@items AS text) IS NOT NULL
  AND CAST(@items AS text) <> ''
RETURNING id
