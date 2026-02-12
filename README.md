<<<<<<< HEAD
# 📦 PedidoWeb - Sistema de Força de Vendas Integrado

Sistema web desenvolvido em ASP.NET Core MVC para gestão de pedidos de venda, com foco em força de vendas externa e integração direta com ERP via SOAP (C&S Sistemas).

## 🚀 Visão Geral

O **PedidoWeb** é uma solução completa para emitir pedidos de forma ágil, segura e integrada. Ele permite que vendedores e representantes consultem produtos, visualizem estoques em tempo real (via integração), cadastrem clientes e emitam pedidos que são automaticamente sincronizados com o ERP da empresa.

## 🛠 Tecnologias Utilizadas

- **Framework**: .NET 6 / ASP.NET Core MVC
- **Linguagem**: C#
- **Banco de Dados**: SQL Server (Entity Framework Core)
- **Frontend**: Razor Views, Bootstrap 5, jQuery, DataTables
- **Autenticação**: ASP.NET Core Identity
- **Integração**: WCF (SOAP) para comunicação com ERP C&S

## ✨ Funcionalidades Principais

### 1. 🔐 Controle de Acesso e Segurança
- **Autenticação Segura**: Login e registro de usuários.
- **Perfis de Acesso (Roles)**:
  - **Administrador**: Acesso total ao sistema, configurações e todos os pedidos.
  - **Vendedor/Representante**: Acesso restrito apenas aos clientes vinculados à sua carteira.
- **Carteira de Clientes**: Vinculação dinâmica de usuários a clientes específicos.

### 2. 🛒 Gestão de Pedidos
- **Emissão de Pedidos**: Interface intuitiva para lançamento de novas vendas.
- **Rascunho Automático**: Salvamento local (browser) para evitar perda de dados durante a digitação.
- **Busca Inteligente de Produtos**: Pesquisa via AJAX por Descrição, Código Interno, Código de Barras ou Referência de Fábrica.
- **Validação de Estoque**: Impede a venda de itens sem saldo (configurável).
- **Ciclo de Vida**:
  - *Aberto*: Em digitação.
  - *Autorizado*: Pronto para envio ao ERP.
  - *Integrado*: Sucesso na sincronização.
- **Impressão**: Geração de espelho do pedido para conferência ou PDF.

### 3. 🔄 Integração ERP (C&S Sistemas)
O sistema possui um motor robusto de sincronização bidirecional:
- **Importação**:
  - Clientes (com sanitização de dados e endereços).
  - Produtos (Estoque, Preços, Códigos).
  - Condições de Pagamento.
- **Exportação**:
  - Envio automático de pedidos autorizados para o ERP.
- **Tratamento de Falhas**: Logs detalhados e sanitização de XML (correção de caracteres inválidos `&`, `0x1F`, etc.).

### 4. ⚙️ Configurações e Cadastros
- **Minha Empresa**: Configuração de dados fiscais (CNPJ, Razão Social), Logomarca e parâmetros de integração.
  - **Parâmetros Gerais**: Nova guia para ativar validação de estoque e modo de integração.
  - **Modo Integração**: Quando ativo, bloqueia a criação/edição manual de Clientes, Produtos e Condições de Pagamento, forçando a importação via ERP.
- **Condições de Pagamento**: Gestão de prazos e parcelas importadas.
- **Produtos**: Visualização de catálogo com preços e saldos.

## 📦 Instalação e Configuração

1. **Clonar o Repositório**:
   ```bash
   git clone https://github.com/seu-usuario/PedidoWeb.git
   ```

2. **Configurar Banco de Dados**:
   - Ajuste a Connection String no arquivo `appsettings.json`.
   - Execute as migrations:
     ```bash
     dotnet ef database update
     ```

3. **Configuração Inicial**:
   - Inicie a aplicação.
   - Acesse `Cadastros > Minha Empresa`.
   - Preencha o CNPJ e a URL do WebService do ERP.
   - Execute as rotinas de importação inicial (Clientes, Produtos, Condições).

## 📄 Estrutura do Projeto

- `/Controllers`: Lógica de controle e fluxo de dados.
- `/Models`: Classes de domínio e ViewModels.
- `/Views`: Interfaces de usuário (Razor).
- `/Services`: Camada de integração (`IntegracaoCesService`) e lógica de negócio complexa.
- `/Data`: Contexto do banco de dados e migrações

---
*Desenvolvido com foco em performance e integridade de dados.*
=======
# PedidoWeb
Sistema para pedidos web utilizando integração com o sistema C&amp;S
>>>>>>> a073548e95ad87c1b1db9015b417f8654056b1d5

