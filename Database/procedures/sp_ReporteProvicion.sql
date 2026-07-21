DELIMITER $$
CREATE definer = prosigo@`%` procedure sp_ReporteProvicion(IN p_lista_prestamos text)
BEGIN
    /*==============================================================================
    REPORTE DE AMORTIZACIONES QUE TIENEN AL MENOS UN DÍA DENTRO DEL MES DE JULIO
    ----------------------------------------------------------------------------
    Lógica:
    - Se toma la fecha de vencimiento de la amortización actual.
    - Se toma la fecha de vencimiento de la amortización anterior.
    - El periodo real de la amortización es:

            Fecha anterior -----------------> Fecha actual

    - Si ese periodo se cruza con Julio, entonces se incluye.

    Ejemplos:

    17-Jun -------------17-Jul   -> SI
    17-Jul -------------18-Ago   -> SI
    31-Jul -------------31-Ago   -> SI
    01-Ago -------------01-Sep   -> NO

==============================================================================*/
    SELECT
        /*======================
          DATOS DEL GRUPO
        ======================*/
        S.S_GRUPO,
        PB.P_NOMBRE AS GRUPO,

        /*======================
          DATOS DEL PRÉSTAMO
        ======================*/
        P.PQ_CLAVE AS PRESTAMO,
        P.PQ_FECHA_OPERACION AS CREDTITO_OTORGADO,
        P.PQ_FECHA_LIQUIDACION AS CREDITO_LIQUIDADO,

        /*======================
          DATOS DE LA AMORTIZACIÓN
        ======================*/
        A.A_NUMERO AS AMORTIZA_NUMERO,
        A.A_FECHA_INICIO AS AMORTIZA_INICIO,
        A.A_FECHA_VENCIMIENTO AS AMORTIZA_VENCIMIENTO,
        S.S_CLAVE,
        S.S_NOMBRE,
        A.A_NUMERO AS PRIMERA_AMORTIZACION,
        A.A_IMPORTE AS IMPORTE_AMORTIZACION,

        /*======================
          TOTAL ABONADO
        ======================*/
        IFNULL(SUM(CO.CO_IMPORTE),0) AS ABONADO,

        /*======================
          SALDO DE LA AMORTIZACIÓN
        ======================*/
        (A.A_IMPORTE-IFNULL(SUM(CO.CO_IMPORTE),0)) AS SALDO_AMORTIZACION,

        /*======================
          DÍAS DE ATRASO
        ======================*/
        DATEDIFF(CURDATE(), A.A_FECHA_VENCIMIENTO) AS DIAS_ATRASO,

        /*======================
          MESES DE ATRASO
        ======================*/
        FLOOR(DATEDIFF(CURDATE(), A.A_FECHA_VENCIMIENTO)/30) AS MESES_ATRASO,

        /*======================
          ÚLTIMO PAGO
        ======================*/
        (
            SELECT MAX(C2.CO_FECHA_COBRO)
            FROM Cobros C2
            WHERE C2.PQ_CLAVE=P.PQ_CLAVE
        ) AS ULTIMO_PAGO,

        /*======================
          DÍAS SIN PAGAR
        ======================*/
        DATEDIFF(
            CURDATE(),
            (
                SELECT MAX(C2.CO_FECHA_COBRO)
                FROM Cobros C2
                WHERE C2.PQ_CLAVE=P.PQ_CLAVE
            )
        ) AS DIAS_SIN_PAGAR,

        /*======================
          PAGO SOSTENIDO
        ======================*/
        CASE
            WHEN P.PQ_FECHA_OPERACION>=DATE_SUB(CURDATE(),INTERVAL 3 MONTH)
                THEN 'CREDITO RECIENTE'
            WHEN EXISTS (
                SELECT 1
                FROM Cobros C3
                WHERE C3.PQ_CLAVE=P.PQ_CLAVE
                AND C3.CO_IMPORTE>0
                AND C3.CO_FECHA_COBRO>=DATE_SUB(CURDATE(),INTERVAL 3 MONTH)
            ) THEN 'SI'
            ELSE 'NO'
        END AS PAGO_SOSTENIDO

    FROM Prestamo P
    INNER JOIN Socios S ON S.S_CLAVE=P.S_CLAVE
    INNER JOIN Pobla PB ON PB.P_CLAVE=S.S_GRUPO
    INNER JOIN Amortiza A ON A.PQ_CLAVE=P.PQ_CLAVE
    LEFT JOIN Amortiza AP ON AP.PQ_CLAVE=A.PQ_CLAVE AND AP.A_NUMERO=A.A_NUMERO-1
    LEFT JOIN Cobros CO ON CO.PQ_CLAVE=A.PQ_CLAVE AND CO.A_NUMERO=A.A_NUMERO

    WHERE
        /* PERIODO DENTRO DE JULIO */
        (
            (AP.A_FECHA_VENCIMIENTO<='2026-07-31' AND A.A_FECHA_VENCIMIENTO>='2026-07-01')
            OR
            (AP.A_FECHA_VENCIMIENTO IS NULL AND P.PQ_FECHA_OPERACION<='2026-07-31' AND A.A_FECHA_VENCIMIENTO>='2026-07-01')
        )
        /*======================================================
            FILTRO DINÁMICO DE LA LISTA DE PRÉSTAMOS
        ======================================================*/
        AND FIND_IN_SET(P.PQ_CLAVE, p_lista_prestamos) > 0

    GROUP BY
        P.PQ_CLAVE,
        S.S_CLAVE,
        S.S_NOMBRE,
        A.A_NUMERO

    ORDER BY
        S.S_CLAVE,
        S.S_NOMBRE,
        MESES_ATRASO DESC,
        DIAS_ATRASO DESC,
        S.S_GRUPO;
END$$
DELIMITER ;
