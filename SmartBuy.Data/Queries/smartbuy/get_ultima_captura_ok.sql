-- ¿El bot de esta cadena ya corrió bien hoy? El orquestador lo consulta en
-- cada tick: si hay captura 'ok' del día, no vuelve a disparar.
SELECT id
FROM captura
WHERE cadena_id = @cadenaid
  AND estado = 'ok'
  AND fecha_inicio >= CURRENT_DATE
ORDER BY id DESC
LIMIT 1
