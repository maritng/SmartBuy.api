-- Alta de cuenta. El unique de email rechaza el duplicado con 23505:
-- AuthServices lo traduce a "ya existe una cuenta con ese email".
INSERT INTO usuario (email, nombre, password_hash)
VALUES (@email, @nombre, @passwordhash)
RETURNING id
