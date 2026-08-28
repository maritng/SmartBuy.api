# Scripts de inicialización de la base

Todo `.sql` o `.sh` que pongas en esta carpeta se ejecuta en orden alfabético
la **primera vez** que se crea el contenedor de Postgres (volumen vacío).

Si ya levantaste la base y querés que se re-ejecuten desde cero, borrá el volumen:

```
docker compose down -v
docker compose up -d
```

Ejemplo: `01-esquema.sql`, `02-datos-prueba.sql`.

Nota: la base se llama `SmartBuy` con mayúsculas, así que en psql hay que
citarla: `\c "SmartBuy"`. Desde .NET/DBeaver no hace falta nada especial,
se pone el nombre tal cual.
