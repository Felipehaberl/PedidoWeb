START TRANSACTION;

CREATE TABLE `Clientes` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `CpfCnpj` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `Telefone` longtext CHARACTER SET utf8mb4 NULL,
    `Endereco` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_Clientes` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `CondicoesPagamento` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Descricao` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `QuantidadeParcelas` int NOT NULL,
    `IntervaloDias` int NOT NULL,
    CONSTRAINT `PK_CondicoesPagamento` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Produtos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `Descricao` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Preco` decimal(18,2) NOT NULL,
    `Estoque` decimal(18,2) NOT NULL,
    CONSTRAINT `PK_Produtos` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Pedidos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Data` datetime(6) NOT NULL,
    `ClienteId` int NOT NULL,
    `CondicaoPagamentoId` int NOT NULL,
    `ValorTotal` decimal(18,2) NOT NULL,
    `Status` int NOT NULL,
    CONSTRAINT `PK_Pedidos` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Pedidos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Pedidos_CondicoesPagamento_CondicaoPagamentoId` FOREIGN KEY (`CondicaoPagamentoId`) REFERENCES `CondicoesPagamento` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `ItensPedido` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PedidoId` int NOT NULL,
    `ProdutoId` int NOT NULL,
    `Quantidade` decimal(18,2) NOT NULL,
    `ValorUnitario` decimal(18,2) NOT NULL,
    `ValorTotal` decimal(18,2) NOT NULL,
    CONSTRAINT `PK_ItensPedido` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_ItensPedido_Pedidos_PedidoId` FOREIGN KEY (`PedidoId`) REFERENCES `Pedidos` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_ItensPedido_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_ItensPedido_PedidoId` ON `ItensPedido` (`PedidoId`);

CREATE INDEX `IX_ItensPedido_ProdutoId` ON `ItensPedido` (`ProdutoId`);

CREATE INDEX `IX_Pedidos_ClienteId` ON `Pedidos` (`ClienteId`);

CREATE INDEX `IX_Pedidos_CondicaoPagamentoId` ON `Pedidos` (`CondicaoPagamentoId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260206043024_AdicionarEntidadesPedidoWeb', '6.0.36');

COMMIT;

START TRANSACTION;

ALTER TABLE `Produtos` ADD `CodigoBarras` varchar(50) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Produtos` ADD `CodigoFabrica` varchar(50) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Produtos` ADD `CodigoOriginal` varchar(50) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Produtos` ADD `DescricaoVenda` varchar(500) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Produtos` ADD `ProdutoIdIntegracao` longtext CHARACTER SET utf8mb4 NULL;

CREATE TABLE `Empresas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Cnpj` varchar(18) CHARACTER SET utf8mb4 NOT NULL,
    `RazaoSocial` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `NomeFantasia` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `LogomarcaPath` varchar(500) CHARACTER SET utf8mb4 NULL,
    `ModoIntegracao` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Empresas` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `UsuariosClientes` (
    `UsuarioId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClienteId` int NOT NULL,
    CONSTRAINT `PK_UsuariosClientes` PRIMARY KEY (`UsuarioId`, `ClienteId`),
    CONSTRAINT `FK_UsuariosClientes_AspNetUsers_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UsuariosClientes_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_UsuariosClientes_ClienteId` ON `UsuariosClientes` (`ClienteId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260206234119_AdicionarEmpresa', '6.0.36');

COMMIT;

