create
    definer = root@localhost procedure sp_reporte_cartera_ejecutivo(IN p_id_usuario int, IN p_rol varchar(50))
BEGIN
    -- 1. Normalizamos
    SET p_rol = IFNULL(TRIM(p_rol), '');

    -- 2. Limpiamos tablas temporales
    DROP TEMPORARY TABLE IF EXISTS TmpPrestamosActivos;
    DROP TEMPORARY TABLE IF EXISTS TmpCobrosFiltrados;
    DROP TEMPORARY TABLE IF EXISTS TmpAmortizaFiltrado;

    -- 3. Creamos la tabla temporal 1: Traemos el nombre del EJECUTIVO
    CREATE TEMPORARY TABLE TmpPrestamosActivos AS
    SELECT
        Socios.S_CLAVE,
        Prestamo.PQ_CLAVE,
        Prestamo.PQ_IMPORTE,
        -- Validamos: si no hay usuario cruzado, le ponemos 'No identificado'
        IFNULL(u.NombreUsuario, 'Sin Agrupar') AS ejecutivo_asignado
    FROM Pobla
    INNER JOIN Socios ON Socios.S_GRUPO = Pobla.P_CLAVE
    INNER JOIN Prestamo ON Prestamo.S_CLAVE = Socios.S_CLAVE

    -- Cambiamos a LEFT JOIN para incluir a los clientes que no tienen grupo o usuario asignado
    LEFT JOIN autentificacion.grupo_asignado ga ON ga.S_GRUPO = Socios.S_GRUPO
    LEFT JOIN autentificacion.Usuarios u ON ga.usuario_id = u.Id

    WHERE Pobla.P_CLAVE BETWEEN 990000 AND 994999
      AND Prestamo.PQ_FECHA_LIQUIDACION = '9999-12-31'
      AND Prestamo.PQ_DOCUMENTO_RENOVO = 0

      -- FILTRO SEGUN ROL
      AND (
          p_rol = 'Admin'
          OR
          -- Si no es Admin, forzosamente debe ver solo los suyos (ignorando los 'No identificados')
          (p_rol != 'Admin' AND ga.usuario_id = p_id_usuario)
      );

    ALTER TABLE TmpPrestamosActivos ADD INDEX idx_pq_clave (PQ_CLAVE);
    ALTER TABLE TmpPrestamosActivos ADD INDEX idx_ejecutivo (ejecutivo_asignado);


    -- 4. Creamos la tabla temporal 2: Cobros filtrados
    CREATE TEMPORARY TABLE TmpCobrosFiltrados AS
    SELECT
        C.PQ_CLAVE,
        SUM(C.CO_IMPORTE) AS total_cobrado
    FROM Cobros C
    INNER JOIN TmpPrestamosActivos PA ON C.PQ_CLAVE = PA.PQ_CLAVE
    GROUP BY C.PQ_CLAVE;

    ALTER TABLE TmpCobrosFiltrados ADD INDEX idx_cobro_pq (PQ_CLAVE);


    -- 5. Creamos la tabla temporal 3: Amortizaciones vencidas filtradas
    CREATE TEMPORARY TABLE TmpAmortizaFiltrado AS
    SELECT
        A.PQ_CLAVE,
        SUM(A.A_IMPORTE) AS capital_vencido
    FROM Amortiza A
    INNER JOIN TmpPrestamosActivos PA ON A.PQ_CLAVE = PA.PQ_CLAVE
    WHERE A.A_FECHA_LIQUIDACION = '9999-12-31'
      AND A.A_FECHA_VENCIMIENTO <= CURDATE()
    GROUP BY A.PQ_CLAVE;

    ALTER TABLE TmpAmortizaFiltrado ADD INDEX idx_amortiza_pq (PQ_CLAVE);


    -- 6. Consulta final: Ensamblamos y agrupamos general por EJECUTIVO
    SELECT
        PA.ejecutivo_asignado,
        COUNT(DISTINCT PA.S_CLAVE) AS total_clientes,
        COUNT(DISTINCT PA.PQ_CLAVE) AS total_prestamos,
        SUM(PA.PQ_IMPORTE) AS monto_colocado,
        SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)) AS saldo_cartera,
        SUM(IFNULL(A.capital_vencido, 0)) AS capital_vencido,
        SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)) - SUM(IFNULL(A.capital_vencido, 0)) AS saldo_final,

        -- Cálculo de calidad asegurando que no se divida entre 0
        (
            (SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)) - SUM(IFNULL(A.capital_vencido, 0)))
            /
            NULLIF(SUM(PA.PQ_IMPORTE - IFNULL(C.total_cobrado, 0)), 0)
        ) * 100 AS porcentaje_calidad

    FROM TmpPrestamosActivos PA
    LEFT JOIN TmpCobrosFiltrados C ON PA.PQ_CLAVE = C.PQ_CLAVE
    LEFT JOIN TmpAmortizaFiltrado A ON PA.PQ_CLAVE = A.PQ_CLAVE

    -- El GROUP BY agrupará correctamente todos los nulos bajo la etiqueta 'No identificado'
    GROUP BY PA.ejecutivo_asignado;

    -- 7. Limpiamos la memoria
    DROP TEMPORARY TABLE IF EXISTS TmpPrestamosActivos;
    DROP TEMPORARY TABLE IF EXISTS TmpCobrosFiltrados;
    DROP TEMPORARY TABLE IF EXISTS TmpAmortizaFiltrado;

END;

