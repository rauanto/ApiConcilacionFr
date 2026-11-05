create
    definer = raul@`%` procedure sp_obtener_saldos_contables()
BEGIN

    DECLARE v_cta_base     INT DEFAULT 1018;

    -- Parámetros para el Bloque 1
    DECLARE v_b1_scta1     INT DEFAULT 5;
    DECLARE v_b1_scta2     INT DEFAULT 16;
    DECLARE v_b1_scta3     INT DEFAULT 63;
    DECLARE v_b1_scta4     INT DEFAULT 109;

    -- Parámetros compartidos para los Bloques 2 y 3
    DECLARE v_b23_scta1    INT DEFAULT 5;
    DECLARE v_b23_scta2    INT DEFAULT 16;
    DECLARE v_b23_scta3    INT DEFAULT 62;
    DECLARE v_b23_scta4    INT DEFAULT 104;

    -- Parámetros para el Bloque 4
    DECLARE v_b4_scta1     INT DEFAULT 2;




    -- BLOQUE 1: Cuenta con SCTA5 y Etapa 3
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5) AS CUENTA,
           C.C_DESCRIPCION,
           (
               (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
               (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
           ) AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           3 AS ETAPA,
           P2.P_NOMBRE AS GRUPO
    FROM Pcontabil_2026.Catalogo C
    LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA5
    LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA   = v_cta_base
      AND C.C_SCTA1 = v_b1_scta1
      AND C.C_SCTA2 = v_b1_scta2
      AND C.C_SCTA3 = v_b1_scta3
      AND C.C_SCTA4 = v_b1_scta4
      AND C.C_SCTA5 > 0
    HAVING SALDO > 0

    UNION ALL

    -- BLOQUE 2: Cuenta con SCTA6 > 1 y Etapa 3
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5, '-', C_SCTA6, '-', C_SCTA7) AS CUENTA,
           C.C_DESCRIPCION,
           (
               (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
               (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
           ) AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           3 AS ETAPA,
           P2.P_NOMBRE AS GRUPO
    FROM Pcontabil_2026.Catalogo C
    LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA6
    LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA   = v_cta_base
      AND C.C_SCTA1 = v_b23_scta1
      AND C.C_SCTA2 = v_b23_scta2
      AND C.C_SCTA3 = v_b23_scta3
      AND C.C_SCTA4 = v_b23_scta4
      AND C.C_SCTA6 > 1
    HAVING SALDO > 0

    UNION ALL

    -- BLOQUE 3: Cuenta con SCTA7 > 0 y Etapa 3
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5, '-', C_SCTA6, '-', C_SCTA7) AS CUENTA,
           C.C_DESCRIPCION,
           (
               (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
               (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
           ) AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           3 AS ETAPA,
           P2.P_NOMBRE AS GRUPO
    FROM Pcontabil_2026.Catalogo C
    LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA7
    LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA   = v_cta_base
      AND C.C_SCTA1 = v_b23_scta1
      AND C.C_SCTA2 = v_b23_scta2
      AND C.C_SCTA3 = v_b23_scta3
      AND C.C_SCTA4 = v_b23_scta4
      AND C.C_SCTA7 > 0
    HAVING SALDO > 0

    UNION ALL

    -- BLOQUE 4: Cuenta con SCTA1 = @B4_SCTA1, Etapa 2 y Redondeo a 2 decimales
    SELECT CONCAT(C_CTA, '-', C_SCTA1, '-', C_SCTA2, '-', C_SCTA3, '-', C_SCTA4, '-', C_SCTA5, '-', C_SCTA6, '-', C_SCTA7) AS CUENTA,
           C.C_DESCRIPCION,
           ROUND(
               (
                   (C_DEBE_MES0 + C_DEBE_MES1 + C_DEBE_MES2 + C_DEBE_MES3 + C_DEBE_MES4 +
                    C_DEBE_MES5 + C_DEBE_MES6 + C_DEBE_MES7 + C_DEBE_MES8 + C_DEBE_MES9) -
                   (C_HABER_MES0 + C_HABER_MES1 + C_HABER_MES2 + C_HABER_MES3 + C_HABER_MES4 +
                    C_HABER_MES5 + C_HABER_MES6 + C_HABER_MES7 + C_HABER_MES8 + C_HABER_MES9)
               ), 2
           ) AS SALDO,
           S.S_CLAVE,
           S.S_GRUPO,
           2 AS ETAPA,
           P2.P_NOMBRE AS GRUPO
    FROM Pcontabil_2026.Catalogo C
    LEFT JOIN creditos_fincrece.Socios S ON S.S_CLAVE = C.C_SCTA7
    LEFT JOIN creditos_fincrece.Pobla P2 ON P2.P_CLAVE = S.S_GRUPO
    WHERE C.C_CTA   = v_cta_base
      AND C.C_SCTA1 = v_b4_scta1
      AND C.C_SCTA7 > 0
    HAVING SALDO != 0;

END;

