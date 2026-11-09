# Validação executada

- .NET SDK 8.0.427, compilação Debug e Release.
- Build sem avisos ou erros.
- Restauração das dependências e geração de migration inicial para MySQL 8.
- Script MySQL idempotente gerado pelo provider Pomelo.
- Testes HTTP reais contra a aplicação iniciada com SQLite em banco temporário:
  - inicialização, seed, login e renderização de todas as views;
  - delivery sem endereço válido e endereço de outro cliente recusados;
  - estoque insuficiente em um item não altera estoque do outro nem grava pedido;
  - quantidade inválida e POST sem antiforgery recusados;
  - preço e operador enviados indevidamente pelo cliente são ignorados;
  - venda com delivery calcula R$ 42,80 (2 × R$ 18,90 + R$ 5,00);
  - repetição da mesma operação retorna o mesmo pedido, sem segunda baixa;
  - alterar preço para R$ 29,90 preserva R$ 18,90 no item histórico e no recibo;
  - criação de cliente, endereço e usuário Caixa;
  - Caixa recebe 403 nas páginas e mutações administrativas e no recibo alheio;
  - relatórios por período, inativação e revogação de sessão de usuário inativo.
- Chromium/Playwright headless, telas desktop 1440 × 900 e celular 390 × 844:
  - login, adicionar produtos e buscar cliente por telefone;
  - selecionar delivery e endereço;
  - reproduzir total da referência: R$ 50,20 + R$ 5,00 = R$ 55,20;
  - aba Bebidas mostra exatamente duas bebidas;
  - ausência de rolagem horizontal no painel em viewport móvel;
  - finalizar pedido e conferir total no recibo;
  - nenhum erro JavaScript observado.

## Limites da validação

Não foi executado um servidor MySQL neste ambiente. A migration e o SQL foram gerados para MySQL, mas a persistência funcional foi exercitada com SQLite. Antes da apresentação, execute com MySQL seguindo o README e repita o roteiro de docs/REQUISITOS.md. Concorrência sob carga, múltiplas instâncias e implantação HTTPS não foram testadas.

## Atualização visual e catálogo — verificações adicionais

- Compilação e repetição do teste de integração existente com o catálogo de 16 produtos.
- Simulação de banco antigo com sete produtos e um produto personalizado: importação dos exemplos ausentes preservou preços, versões, estoque, imagens originais e número de pedidos. Repetição não criou duplicatas.
- Usuário Caixa recebeu 403 ao tentar importar exemplos por POST.
- Valores antigos de imagem `.svg` passaram a exibir fotografias sem modificar a linha no banco.
- Navegação Chromium em 1440 × 1000 e 390 × 844: busca por "AGUA" encontrou "Água Mineral"; filtro Bebidas mostrou cinco opções; busca vazia exibiu mensagem; fotografias carregaram sem erro.
- Venda por delivery manteve o total de R$ 55,20 e abriu o recibo.
- Tela do restaurante, endereço, links de contato e arquivo de créditos acessíveis. Sem erros JavaScript ou rolagem horizontal nas telas móveis verificadas.
- Persistência desta atualização testada em SQLite. MySQL continua dependendo de validação em um servidor real. A atualização não muda o esquema do banco.
