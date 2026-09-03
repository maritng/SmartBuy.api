-- ¿El bot de esta cadena ya corrió bien en la ventana vigente? El orquestador
-- pasa el inicio de la ventana (ej. hoy 07:00 hora argentina, en UTC): si hay
-- captura 'ok' desde ese momento, no vuelve a disparar hasta la próxima ventana.
SELECT id
FROM captura
WHERE cadena_id = @cadenaid
  AND estado = 'ok'
  AND fecha_inicio >= CAST(@desde AS timestamptz)
ORDER BY id DESC
LIMIT 1
