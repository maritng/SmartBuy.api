-- Login: la cuenta por email (ya normalizado a minúsculas por el servicio).
-- El hash viaja solo hasta AuthServices para verificar; nunca sale en DTOs.
SELECT id,
       email,
       nombre,
       password_hash,
       activo
FROM usuario
WHERE email = @email
