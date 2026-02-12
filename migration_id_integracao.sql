START TRANSACTION;

ALTER TABLE `CondicoesPagamento` ADD `IntegracaoId` longtext CHARACTER SET utf8mb4 NULL;

ALTER TABLE `Clientes` ADD `IntegracaoId` longtext CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260209003939_AdicionarIdIntegracaoModelos', '6.0.36');

COMMIT;

