-- Alta de lista. El unique (usuario_id, nombre) rechaza el duplicado con
-- 23505: ListaServices lo traduce a "ya tenés una lista con ese nombre".
INSERT INTO lista (usuario_id, nombre)
VALUES (@usuarioid, @nombre)
RETURNING id
