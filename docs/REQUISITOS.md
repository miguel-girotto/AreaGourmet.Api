# Roteiro de apresentação e critérios do PDF

Prazo indicado no PDF: 27/11/2026. Notas individuais; todos os integrantes devem apresentar.

| Critério | Valor | Como demonstrar |
|---|---:|---|
| Três CRUDs, sem movimento | 1,0 | Clientes, Produtos, Usuários: criar, listar/buscar, editar e inativar/reativar |
| CSS/framework CSS | 0,5 | CSS próprio responsivo em wwwroot/css/site.css |
| Movimento | 1,0 | Finalizar pedido com itens e baixa de estoque |
| Validação | 1,0 | Rejeitar e-mail duplicado, telefone inválido, preço negativo e delivery sem endereço |
| Página principal e menu | 0,5 | Painel de vendas e sidebar por perfil |
| Autenticação/autorização | 1,0 | Login como Caixa e tentativa de abrir /Usuarios ou /Relatorios |
| Geração de dados | 1,0 | Seed de clientes, produtos e vendas históricas em banco vazio |
| Filtros nos CRUDs | 1,0 | Buscar nome/telefone, nome/categoria e nome/e-mail |
| Múltiplas tabelas e totalização | 1,0 | Relatório com Pedidos + Clientes + Usuários + Itens |
| Agrupamento | 0,5 | Totais por pagamento no relatório |
| Pivot | 0,5 | Categorias nas linhas, meses nas colunas e totais |
| GitHub durante desenvolvimento | 1,0 | Histórico real de commits e contribuições; depende do grupo |

A soma da tabela é 10,0. O PDF prevê desconto de 0,5 em GitHub se apenas a versão final for enviada.

## Decisões de escopo

A imagem é referência de composição e cores, não especificação literal de dados. O catálogo começa em Todos, as categorias filtram corretamente, as descrições dos itens correspondem à categoria e a comanda nasce vazia. O número da comanda é atribuído ao concluir a venda. Ilustrações SVG substituem fotografias. O CRUD usa inativação em vez de remoção física para preservar vendas.

O estoque é simples por produto. O projeto não modela ingredientes ou consumo de embalagens compartilhadas. Não há cancelamento/estorno, fechamento de caixa, descontos, pagamentos reais nem integração fiscal; esses recursos não constam dos requisitos entregues.

## Exemplos para demonstrar

1. Faça login como administrador e cadastre um usuário Caixa.
2. Filtre os três cadastros, altere um produto e cadastre dois endereços para uma cliente.
3. Venda duas marmitas, um suco e um adicional por delivery; confira taxa e recibo.
4. Altere o preço de um produto e abra o recibo anterior: preço preservado.
5. Tente vender mais unidades do que há no estoque: operação recusada.
6. Filtre relatórios incluindo os quatro meses do seed e explique LINQ, agrupamento e pivot.
7. Entre como Caixa e demonstre o bloqueio dos recursos administrativos.

Os agrupamentos e pivot são calculados com LINQ em memória após consulta EF com Includes e filtro de período no banco. O limite de período é dois anos; grandes volumes exigem agregação no banco e paginação do relatório.
