dotnet publish .\ApiConcilacionFr.csproj --configuration Release --output .\publish

## Reporte de saldos contables por etapa

Los reportes PDF y Excel aceptan `mes` (0 a 12), `etapa` (`1`, `2`, `3` o
`todas`), uno o varios `S_GRUPO` y `indice` (opcional, por defecto `false`).
Si se omite `S_GRUPO`, se incluyen todos los grupos. En PDF cada grupo comienza
en una página nueva y `indice=true` agrega un índice con la página inicial de
cada grupo. En Excel cada grupo se genera en una hoja separada y `indice=true`
agrega una hoja índice.

```text
GET /api/ReporteContabilidad/reportes-contabilidad/pdf?mes=3&etapa=todas&S_GRUPO=12&S_GRUPO=18&indice=true
GET /api/ReporteContabilidad/reportes-contabilidad/excel?mes=3&etapa=todas&S_GRUPO=12&S_GRUPO=18&indice=true
```