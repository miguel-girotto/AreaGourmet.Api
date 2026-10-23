# Regras, persistência e consultas

## Perfis e sessão

`Program.cs` configura cookie HttpOnly, SameSite=Lax, oito horas, CSRF global para ações MVC e cabeçalho `X-CSRF-TOKEN` para a API de pedidos. Sessões são revalidadas contra `Usuarios` a cada requisição (`SessaoEvents`). O servidor revoga cookies de contas desativadas ou com versão de sessão diferente. Login tem limite de dez tentativas por minuto/IP. Não há endpoint público para criar administrador.

Administrador gerencia cadastros, usuários, preços, estoque, consumos, cancelamentos, relatórios e auditoria. Caixa consulta cardápio, clientes e pedidos e realiza vendas. Cadastro/edição/desativação de clientes e seus endereços dependem da permissão `PodeCadastrarClientes`. Controllers e políticas bloqueiam URLs e POSTs diretos; ocultar menu é apenas apresentação. A rota `/api/health/database` exige sessão e retorna 401 para anônimos.

`Usuario.Senha` mantém o nome antigo da coluna e passa a armazenar exclusivamente hashes nas operações novas. Contas legadas em texto puro exigem redefinição administrativa; nunca são aceitas por comparação textual. A criação do primeiro administrador é um comando local com variáveis temporárias. Nenhum usuário foi criado automaticamente no banco principal.

## Cadastros

Clientes, Produtos e Categorias usam desativação, sem exclusão física. Telefone novo é normalizado para 10/11 dígitos com DDD e verificado contra duplicidade. Formulários numéricos são interpretados com ponto decimal do padrão HTML, independentemente da cultura do Windows; entradas ambíguas com separador de milhar são rejeitadas. Produtos têm preço positivo com duas casas, categoria ativa e opção `DisponivelVenda`: insumos podem existir sem aparecer no cardápio. Categoria inativa retira seus produtos das vendas. Os filtros são condições LINQ aplicadas antes de materializar consultas EF; não apenas filtros de HTML.

Endereços de um cliente podem ser criados e editados. Um endereço selecionado em Delivery deve pertencer ao cliente ativo. Edição posterior não muda o texto de endereço congelado na venda. Os relacionamentos foram alterados de cascata para `RESTRICT`, preservando registros referenciados.

## Pedidos e status

`Operacoes.CriarPedido` busca preço e disponibilidade no banco, agrupa produtos repetidos, valida quantidades (1–999 por produto), modalidade e pagamento, recalcula subtotais e total e associa o operador a partir da sessão. Valores de total enviados pelo navegador são ignorados.

- Mesa: número entre 1 e 999, sem taxa ou endereço de entrega.
- Retirada: cliente opcional, sem taxa/endereço de entrega.
- Delivery: cliente ativo, endereço do próprio cliente e taxa entre zero e 9.999,99, com duas casas. A taxa é informada pelo operador autorizado e validada no servidor; não há cálculo por distância.
- Pagamentos aceitos: PIX, Cartão, Dinheiro. São registros operacionais, sem integração bancária.

É possível **Salvar comanda aberta** (`ABERTO`) ou **Finalizar pedido** diretamente (`FINALIZADO`). Comandas abertas podem ir para `EM PREPARO`; Delivery em preparo pode ir para `SAIU PARA ENTREGA`. De qualquer etapa não cancelada/encerrada, finalizar confirma venda e baixa estoque. Pedidos finalizados não reabrem. Somente Administrador cancela. Cancelamentos preservam itens/valores e, se já houve baixa, geram movimentos de estorno.

Cada novo pedido possui chave de operação GUID única. Reenviar a mesma chave pelo mesmo operador retorna o mesmo pedido, sem novo estoque ou receita. Uma chave pertencente a outro operador é rejeitada. A interface desabilita o envio enquanto aguarda; falhas de conexão orientam consultar o histórico. Ao retornar à tela de vendas uma nova chave é gerada.

Preço unitário, subtotal, nome do produto, cliente e endereço da venda são snapshots. Mudar o cardápio ou cadastro não os altera. Registros anteriores à nova migration têm campos históricos opcionais: não é possível reconstruir com certeza informações antigas que nunca foram salvas. Seus valores antigos não são recalculados.

## Estoque e transações

A tabela existente `EstoqueBebidas` foi preservada; apesar do nome, também pode controlar embalagens e outros insumos. Não há tabela redundante substituindo o saldo existente.

1. Um produto com saldo próprio consome **uma unidade do saldo por unidade vendida**.
2. `Consumos` associa um produto vendido a outros saldos, com fator de consumo (até três casas decimais). Ex.: uma marmita consome uma embalagem. Essas associações são adicionais ao saldo próprio e não recursivas.
3. Produto sem saldo próprio nem associação não gera movimento. Isso é explícito: cadastre uma entrada para controlar saldo próprio ou configure **Estoque → Configurar consumo**. A disponibilidade completa é revalidada no fechamento.
4. Saldo mínimo é configurável nas entradas/saídas; a tela destaca necessidade de reposição. Não é reserva de estoque: comandas abertas não diminuem saldo. A finalização pode ser rejeitada se o saldo mudou.
5. A carga demo cria saldos iniciais apenas quando ainda não existem; não repõe o que já foi consumido. Configura embalagem para as marmitas de demonstração.
6. Cancelamento devolve exatamente as quantidades das movimentações originais `VENDA`, mesmo se as regras de consumo foram alteradas depois. Não apaga nem reescreve movimentos anteriores.

`Operacoes.Transacao` abre transação `ReadCommitted` e atualiza a linha de controle `ControleOperacoes(Id=1)`, adquirindo trava de escrita no MySQL antes das leituras/regras. Assim, pedidos, saldo, consumos, usuários e carga demo são serializados entre instâncias da aplicação. A chave única é outra barreira contra duplicação. Pedido/itens/baixas/auditoria são confirmados juntos; falhas revertem tudo. O escopo de serialização é deliberadamente amplo, adequado à demonstração e a baixo volume; deve ser medido antes de uso com alta concorrência. Evite alterações diretas por SQL em paralelo à aplicação.

## Relatórios acadêmicos

`RelatoriosController.Index` filtra pedidos `FINALIZADO` e período UTC. Pedidos abertos/cancelados não são receita. Total de pedidos inclui taxa de entrega; total dos produtos usa os subtotais dos itens e exclui taxas.

- Múltiplas tabelas: `Pedido` → `Usuario`/`Cliente`; `ItemPedido` → `Pedido`/`Produto`/`Categoria`, por navegações EF traduzidas em joins. Consultas mostram operador, cliente, quantidade e valores.
- Agrupamento: `GroupBy(FormaPagamento)` e `Count`/`Sum` executados no MySQL. As telas também agregam as linhas persistidas por modalidade e produto.
- Pivot: `GroupBy(DataHora.Date)` e três somas condicionais (`Mesa`, `Retirada`, `Delivery`) executadas em SQL. A projeção anônima mantém a tradução pelo Pomelo; só depois os resultados vão para `PivotLinha`. Uma modalidade sem vendas na data soma zero. Uma data sem nenhuma venda não gera linha.
- Produto/categoria: filtram os pedidos que contêm os itens; a seção de produtos mostra somente os itens filtrados. Totais dos pedidos continuam incluindo todo o pedido e suas taxas. Essa distinção está indicada na tela.
- Categorias são as atuais do cadastro; nomes dos itens usam snapshots quando disponíveis. Para análise de categorias históricas seria necessário um snapshot adicional. Não há previsão de receita nem valores simulados.

## Migrations e backup

As quatro migrations anteriores permanecem intactas. `GestaoIntegrada` é incremental: novas colunas opcionais ou com valores iniciais compatíveis, tabelas de movimentos/consumo/auditoria/controle, índices e restrições. Clientes/categorias/produtos existentes mantêm disponibilidade inicial, em vez de ficarem todos inativos. Permissão de clientes para usuários antigos nasce restrita até revisão do administrador.

Foi criado backup do banco local antes da evolução, fora do checkout. A aplicação não migra esquemas automaticamente. DDL do MySQL pode fazer commits implícitos: revisão prévia e backup continuam necessários. Não rode `database update 0`, não apague migrations e não restaure backup sobre dados novos sem um procedimento específico.
