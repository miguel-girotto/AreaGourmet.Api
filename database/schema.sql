CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE TABLE `Clientes` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Telefone` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Ativo` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Clientes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE TABLE `Produtos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Categoria` longtext CHARACTER SET utf8mb4 NOT NULL,
        `PrecoUnitario` decimal(12,2) NOT NULL,
        `EstoqueInsumo` int NOT NULL,
        `ControlaEstoque` tinyint(1) NOT NULL,
        `Ativo` tinyint(1) NOT NULL,
        `Versao` int NOT NULL,
        `Imagem` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Produtos` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE TABLE `Usuarios` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
        `SenhaHash` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Perfil` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Ativo` tinyint(1) NOT NULL,
        `SecurityStamp` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Usuarios` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE TABLE `Enderecos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ClienteId` int NOT NULL,
        `Logradouro` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Numero` varchar(15) CHARACTER SET utf8mb4 NOT NULL,
        `Bairro` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        `Cidade` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        `CEP` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Ativo` tinyint(1) NOT NULL,
        CONSTRAINT `PK_Enderecos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Enderecos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE TABLE `Pedidos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ChaveOperacao` char(36) COLLATE ascii_general_ci NOT NULL,
        `DataHora` datetime(6) NOT NULL,
        `TipoAtendimento` longtext CHARACTER SET utf8mb4 NOT NULL,
        `MesaNumero` int NULL,
        `ClienteId` int NULL,
        `EnderecoId` int NULL,
        `EnderecoEntrega` longtext CHARACTER SET utf8mb4 NOT NULL,
        `UsuarioId` int NOT NULL,
        `TaxaEntrega` decimal(12,2) NOT NULL,
        `ValorTotal` decimal(12,2) NOT NULL,
        `FormatoPagamento` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_Pedidos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Pedidos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Pedidos_Enderecos_EnderecoId` FOREIGN KEY (`EnderecoId`) REFERENCES `Enderecos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_Pedidos_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE TABLE `ItensPedido` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `PedidoId` int NOT NULL,
        `ProdutoId` int NOT NULL,
        `Quantidade` int NOT NULL,
        `PrecoPraticado` decimal(12,2) NOT NULL,
        `NomeProduto` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CategoriaPraticada` longtext CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_ItensPedido` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ItensPedido_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ItensPedido_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Clientes_Telefone` ON `Clientes` (`Telefone`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE INDEX `IX_Enderecos_ClienteId` ON `Enderecos` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE INDEX `IX_ItensPedido_PedidoId` ON `ItensPedido` (`PedidoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE INDEX `IX_ItensPedido_ProdutoId` ON `ItensPedido` (`ProdutoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Pedidos_ChaveOperacao` ON `Pedidos` (`ChaveOperacao`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE INDEX `IX_Pedidos_ClienteId` ON `Pedidos` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE INDEX `IX_Pedidos_DataHora` ON `Pedidos` (`DataHora`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE INDEX `IX_Pedidos_EnderecoId` ON `Pedidos` (`EnderecoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE INDEX `IX_Pedidos_UsuarioId` ON `Pedidos` (`UsuarioId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    CREATE UNIQUE INDEX `IX_Usuarios_Email` ON `Usuarios` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261106135236_InitialMySql') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261106135236_InitialMySql', '8.0.13');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

