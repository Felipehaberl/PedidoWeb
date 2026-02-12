START TRANSACTION;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260206234533_empresa', '6.0.36');

COMMIT;

START TRANSACTION;

ALTER TABLE `Empresas` ADD `WebServiceUrl` varchar(500) CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260207003048_AdicionarWebServiceUrlEmpresa', '6.0.36');

COMMIT;

