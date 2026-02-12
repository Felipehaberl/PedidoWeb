using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using PedidoWeb.Data;
using PedidoWeb.Modelos;
using ServiceReference1;
using System.ServiceModel;
using System.Globalization;

namespace PedidoWeb.Servicos
{
    public class IntegracaoCesService : IIntegracaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IntegracaoCesService> _logger;
        private readonly XNamespace _ns = "http://www.cessistemas.com.br/wsces";

        public IntegracaoCesService(ApplicationDbContext context, ILogger<IntegracaoCesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<servicosSoapClient> ObterClientAsync()
        {
            var empresa = await _context.Empresas.FirstOrDefaultAsync();
            if (empresa == null || string.IsNullOrEmpty(empresa.WebServiceUrl))
            {
                throw new Exception("URL do WebService não configurada no cadastro da empresa.");
            }

            var endpoint = new EndpointAddress(empresa.WebServiceUrl);
            var binding = new BasicHttpBinding();

            // Configurações para suportar HTTPS e mensagens grandes
            if (endpoint.Uri.Scheme == "https")
            {
                binding.Security.Mode = BasicHttpSecurityMode.Transport;
            }

            binding.MaxReceivedMessageSize = 2147483647; // 2GB (Máximo)
            binding.ReaderQuotas.MaxStringContentLength = 2147483647;
            binding.ReaderQuotas.MaxDepth = 32;

            return new servicosSoapClient(binding, endpoint);
        }

        public async Task<bool> EnviarPedidoAsync(Pedido pedido)
        {
            try
            {
                var client = await ObterClientAsync();
                var xmlPedido = GerarXmlPedido(pedido);

                var response = await client.AtualizarPedidoHandelAsync(xmlPedido);
                _logger.LogInformation("Pedido {Id} integrado. Retorno: {Resultado}", pedido.Id, response.Body.AtualizarPedidoHandelResult);

                return true; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha na integração do pedido {PedidoId}", pedido.Id);
                return false;
            }
        }

        public async Task ImportarClientesAsync()
        {
            try
            {
                var client = await ObterClientAsync();
                
                var xmlReq = $@"<ConsultaClienteNomeReq xmlns=""{_ns.NamespaceName}"">
                                    <nome>%</nome>
                                    <atualiza>01/01/2000</atualiza>
                                 </ConsultaClienteNomeReq>";

                var response = await client.ConsultarClientePorNomeAsync(xmlReq);
                var xmlString = response.Body.ConsultarClientePorNomeResult;

                if (string.IsNullOrWhiteSpace(xmlString)) 
                {
                    _logger.LogWarning("WebService retornou XML vazio para Clientes.");
                    return;
                }

                // Sanitização de caracteres inválidos no XML (ex: & solto e caracteres de controle como 0x1F)
                xmlString = System.Text.RegularExpressions.Regex.Replace(xmlString, "&(?!(amp|apos|quot|lt|gt|#[0-9]+);)", "&amp;");
                xmlString = System.Text.RegularExpressions.Regex.Replace(xmlString, "[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

                XDocument doc;
                try {
                    doc = XDocument.Parse(xmlString);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Erro de parse XML. Conteúdo parcial: {Conteudo}", xmlString.Substring(0, Math.Min(xmlString.Length, 1000)));
                    throw new Exception($"O ERP retornou dados com formatação inválida: {ex.Message}");
                }

                // O C&S às vezes retorna sem namespace nos elementos internos
                var clientesXml = doc.Descendants().Where(e => e.Name.LocalName == "cliente");

                foreach (var xml in clientesXml)
                {
                    var integracaoId = xml.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(integracaoId)) continue;

                    var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IntegracaoId == integracaoId);
                    bool novo = false;
                    if (cliente == null)
                    {
                        cliente = new Cliente { IntegracaoId = integracaoId };
                        novo = true;
                    }

                    cliente.Nome = xml.Element(xml.Name.Namespace + "nomeFantasia")?.Value ?? xml.Element(xml.Name.Namespace + "razaoSocial")?.Value ?? "SEM NOME";
                    cliente.CpfCnpj = xml.Element(xml.Name.Namespace + "clienteId")?.Value ?? integracaoId;
                    cliente.Email = xml.Element(xml.Name.Namespace + "email")?.Value;
                    
                    var enderecoXml = xml.Element(xml.Name.Namespace + "endereco");
                    if (enderecoXml != null)
                    {
                        var ddd = enderecoXml.Element(xml.Name.Namespace + "DDD")?.Value;
                        var fone = enderecoXml.Element(xml.Name.Namespace + "fone")?.Value;
                        cliente.Telefone = !string.IsNullOrEmpty(ddd) ? $"({ddd}) {fone}" : fone;
                        
                        cliente.Endereco = $"{enderecoXml.Element(xml.Name.Namespace + "logra")?.Value}, {enderecoXml.Element(xml.Name.Namespace + "nrRes")?.Value} - {enderecoXml.Element(xml.Name.Namespace + "bairro")?.Value} - {enderecoXml.Element(xml.Name.Namespace + "UF")?.Value}";
                    }

                    if (novo) _context.Clientes.Add(cliente);
                    else _context.Clientes.Update(cliente);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao importar clientes");
                throw;
            }
        }

        public async Task ImportarProdutosAsync()
        {
            try
            {
                var client = await ObterClientAsync();
                var empresa = await _context.Empresas.FirstOrDefaultAsync();

                var xmlReq = $@"<ConsultaProdutosNomeReq xmlns=""{_ns.NamespaceName}"">
                                    <empresaId>{empresa?.Cnpj}</empresaId>
                                    <nome>%</nome>
                                    <grupos></grupos>
                                    <linha>0</linha>
                                    <atualiza></atualiza>
                                    <liberadoWeb>S</liberadoWeb>
                                    <liberadoOlist>N</liberadoOlist>
                                    <liberadoMercadoLivre>N</liberadoMercadoLivre>
                                 </ConsultaProdutosNomeReq>";

                var response = await client.ConsultarProdutosPorNomeAsync(xmlReq);
                var xmlString = response.Body.ConsultarProdutosPorNomeResult;

                if (string.IsNullOrWhiteSpace(xmlString)) return;

                // Sanitização de caracteres inválidos no XML
                xmlString = System.Text.RegularExpressions.Regex.Replace(xmlString, "&(?!(amp|apos|quot|lt|gt|#[0-9]+);)", "&amp;");
                xmlString = System.Text.RegularExpressions.Regex.Replace(xmlString, "[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

                XDocument doc;
                try {
                    doc = XDocument.Parse(xmlString);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Erro de parse XML. Conteúdo parcial: {Conteudo}", xmlString.Substring(0, Math.Min(xmlString.Length, 1000)));
                    throw new Exception($"O ERP retornou dados com formatação inválida: {ex.Message}");
                }

                var produtosXml = doc.Descendants().Where(e => e.Name.LocalName == "prod");

                foreach (var xml in produtosXml)
                {
                    var integracaoId = xml.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(integracaoId)) continue;

                    var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.ProdutoIdIntegracao == integracaoId);
                    bool novo = false;
                    if (produto == null)
                    {
                        produto = new Produto { ProdutoIdIntegracao = integracaoId };
                        novo = true;
                    }

                    var desc = xml.Element(xml.Name.Namespace + "nome")?.Value ?? "SEM DESCRIÇÃO";
                    if (desc.Length > 200) desc = desc.Substring(0, 200);
                    produto.Descricao = desc;
                    
                    var cod = xml.Element(xml.Name.Namespace + "codFab")?.Value ?? integracaoId;
                    if (cod.Length > 20) cod = cod.Substring(0, 20);
                    produto.Codigo = cod;
                    
                    var codOri = xml.Element(xml.Name.Namespace + "codOriginal")?.Value;
                    if (codOri != null && codOri.Length > 50) codOri = codOri.Substring(0, 50);
                    produto.CodigoOriginal = codOri;

                    var codFab = xml.Element(xml.Name.Namespace + "codFab")?.Value;
                    if (codFab != null && codFab.Length > 50) codFab = codFab.Substring(0, 50);
                    produto.CodigoFabrica = codFab;

                    var codBarras = xml.Element(xml.Name.Namespace + "gtin")?.Value;
                    if (codBarras != null && codBarras.Length > 50) codBarras = codBarras.Substring(0, 50);
                    produto.CodigoBarras = codBarras;

                    var descVenda = xml.Element(xml.Name.Namespace + "desccaract")?.Value;
                    if (descVenda != null && descVenda.Length > 500) descVenda = descVenda.Substring(0, 500);
                    produto.DescricaoVenda = descVenda;

                    if (decimal.TryParse(xml.Element(xml.Name.Namespace + "precoAtacado")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preco))
                        produto.Preco = preco;

                    if (decimal.TryParse(xml.Element(xml.Name.Namespace + "estq")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal estoque))
                        produto.Estoque = estoque;

                    if (novo) _context.Produtos.Add(produto);
                    else _context.Produtos.Update(produto);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                var erroDetalhado = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError(dbEx, "Erro ao salvar produtos no banco de dados (EF Core)");
                throw new Exception($"Erro de banco de dados ao salvar produtos: {erroDetalhado}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao importar produtos");
                throw;
            }
        }

        public async Task ImportarCondicoesPagamentoAsync()
        {
            try
            {
                var client = await ObterClientAsync();
                var empresa = await _context.Empresas.FirstOrDefaultAsync();
                
                var xmlReq = $@"<ConsultaCondicoesDescricaoReq xmlns=""{_ns.NamespaceName}"">
                                    <descricao>%</descricao>
                                    <empresaId>{empresa?.Cnpj}</empresaId>
                                    <tipos>BO','CH','CC</tipos>
                                 </ConsultaCondicoesDescricaoReq>";

                var response = await client.ConsultarCondicoesPorDescricaoAsync(xmlReq);
                var xmlString = response.Body.ConsultarCondicoesPorDescricaoResult;

                if (string.IsNullOrWhiteSpace(xmlString)) return;

                // Sanitização de caracteres inválidos no XML
                xmlString = System.Text.RegularExpressions.Regex.Replace(xmlString, "&(?!(amp|apos|quot|lt|gt|#[0-9]+);)", "&amp;");
                xmlString = System.Text.RegularExpressions.Regex.Replace(xmlString, "[\x00-\x08\x0B\x0C\x0E-\x1F]", "");

                XDocument doc;
                try {
                    doc = XDocument.Parse(xmlString);
                } catch (Exception ex) {
                    _logger.LogError(ex, "Erro de parse XML. Conteúdo parcial: {Conteudo}", xmlString.Substring(0, Math.Min(xmlString.Length, 1000)));
                    throw new Exception($"O ERP retornou dados com formatação inválida: {ex.Message}");
                }

                // O C&S às vezes retorna sem namespace nos elementos internos
                var condicoesXml = doc.Descendants().Where(e => e.Name.LocalName == "condicao");

                int novos = 0;
                int atualizados = 0;

                foreach (var xml in condicoesXml)
                {
                    // Tenta obter ID pelo atributo 'id' (novo formato) ou elemento 'condicaoId' (antigo)
                    var integracaoId = xml.Attribute("id")?.Value ?? xml.Element(xml.Name.Namespace + "condicaoId")?.Value;
                    
                    if (string.IsNullOrEmpty(integracaoId)) continue;

                    var condicao = await _context.CondicoesPagamento.FirstOrDefaultAsync(c => c.IntegracaoId == integracaoId);
                    bool novo = false;
                    if (condicao == null)
                    {
                        condicao = new CondicaoPagamento { IntegracaoId = integracaoId };
                        novo = true;
                    }

                    // Tenta obter Descrição por 'desc' (novo formato) ou 'descricao' (antigo)
                    condicao.Descricao = xml.Element(xml.Name.Namespace + "desc")?.Value ?? xml.Element(xml.Name.Namespace + "descricao")?.Value ?? "SEM DESCRIÇÃO";
                    
                    // Lógica para contar parcelas
                    var prazosNode = xml.Element(xml.Name.Namespace + "prazos");
                    if (prazosNode != null)
                    {
                        var qtd = prazosNode.Elements(xml.Name.Namespace + "prazo").Count();
                        condicao.QuantidadeParcelas = qtd > 0 ? qtd : 1;
                    }
                    else if (int.TryParse(xml.Element(xml.Name.Namespace + "parcelas")?.Value, out int parcelas))
                    {
                        condicao.QuantidadeParcelas = parcelas;
                    }
                    else
                    {
                        condicao.QuantidadeParcelas = 1;
                    }

                    if (condicao.QuantidadeParcelas > 1) condicao.IntervaloDias = 30;

                    if (novo) 
                    {
                        _context.CondicoesPagamento.Add(condicao);
                        novos++;
                    }
                    else 
                    {
                        _context.CondicoesPagamento.Update(condicao);
                        atualizados++;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Importação de condições finalizada. Novos: {Novos}, Atualizados: {Atualizados}", novos, atualizados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao importar condições de pagamento");
                throw;
            }
        }

        private string GerarXmlPedido(Pedido pedido)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<Pedido>");
            sb.AppendLine($"  <Id>{pedido.Id}</Id>");
            sb.AppendLine($"  <Data>{pedido.Data:s}</Data>");
            sb.AppendLine($"  <ClienteId>{pedido.ClienteId}</ClienteId>");
            sb.AppendLine($"  <ValorTotal>{pedido.ValorTotal.ToString("F2", CultureInfo.InvariantCulture)}</ValorTotal>");
            sb.AppendLine("  <Itens>");
            foreach (var item in pedido.Itens)
            {
                sb.AppendLine("    <Item>");
                sb.AppendLine($"      <ProdutoId>{item.ProdutoId}</ProdutoId>");
                sb.AppendLine($"      <Quantidade>{item.Quantidade.ToString("F2", CultureInfo.InvariantCulture)}</Quantidade>");
                sb.AppendLine($"      <ValorUnitario>{item.ValorUnitario.ToString("F2", CultureInfo.InvariantCulture)}</ValorUnitario>");
                sb.AppendLine("    </Item>");
            }
            sb.AppendLine("  </Itens>");
            sb.AppendLine("</Pedido>");
            return sb.ToString();
        }
    }
}
