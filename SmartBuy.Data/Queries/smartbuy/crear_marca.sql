-- Alta idempotente: si la marca ya existe (mismo nombre) devuelve su id en vez
-- de fallar. El DO UPDATE "inocuo" es necesario para que RETURNING devuelva la
-- fila también en el caso de conflicto.
INSERT INTO marca (nombre)
VALUES (@nombre)
ON CONFLICT (nombre) DO UPDATE SET nombre = excluded.nombre
RETURNING id
