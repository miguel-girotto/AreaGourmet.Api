# Matriz de conformidade

A implementação usa o pedido do aluno como instrução de trabalho, o PDF Frameworks como referência de avaliação, o pré-projeto como contexto e a imagem como referência visual. A existência de arquivos não foi usada como prova isolada de funcionamento.

| Critério oficial | Pontos | Tela / implementação | Evidência executada |
|---|---:|---|---|
| CRUD de três classes (fora movimento) | 1,0 | Cadastros: Clientes, Produtos, Categorias; `CadastrosController`, views tipadas | Criação, edição, detalhes, desativação e filtros com persistência MySQL |
| CSS / framework CSS | 0,5 | `wwwroot/css/site.css`, layout, componentes e SVGs locais | Chromium desktop/mobile; sem overflow a 390px |
| Tabela e funcionalidades de movimento | 1,0 | `MovimentacaoEstoque`, `Operacoes`, Estoque/Movimentações | Entrada, saída, saldo insuficiente, baixa, embalagem e estorno |
| Validação de formulários | 1,0 | `ViewModels/Forms.cs`, controllers e serviço | Preço/quantidade negativos, mesa ausente, Delivery inválido, pagamento inválido, período invertido |
| Principal / menu | 0,5 | Painel administrativo e Vendas, `_Layout` por perfil | Páginas integradas, menu e navegação exercitados |
| Autenticação / autorização | 1,0 | Cookies, `SessaoEvents`, políticas, bootstrap e contas | Login válido/inválido, CSRF, acesso direto negado, último admin, revogação e logout |
| Geração de dados | 1,0 | Painel → Gerar dados (`DadosDemo`) | Execução repetida sem duplicar ou repor saldos existentes |
| Filtros em páginas CRUD | 1,0 | Nome/telefone, situação, categoria; filtros LINQ antes de `ToListAsync` | Pesquisa e inativos conferidos por HTTP e no banco |
| Múltiplas tabelas e totalização | 1,0 | Relatórios, pedidos/operador/cliente/itens/produtos/categoria | Total da tela comparado com consulta MySQL independente |
| Agrupamento | 0,5 | Relatórios → Vendas por pagamento, LINQ `GroupBy`/`Sum`/`Count` | Executado no MySQL e renderizado no navegador |
| Pivot | 0,5 | Relatórios → Vendas por dia e modalidade | Somas condicionais reais, filtros e página validados; tradução SQL corrigida durante os testes |
| Uso de GitHub ao longo do desenvolvimento | 1,0 | Histórico incremental preservado e novos commits descritivos | Commits locais reais; veja estado do envio remoto no relatório de entrega |

Não se atribui automaticamente nota acadêmica. O PDF requer apresentação por todos os integrantes e informa avaliação individual. O histórico anterior foi preservado; commits locais não substituem comprovação de envio ao GitHub.

## Requisitos de negócio

| Requisito | Implementação / verificação |
|---|---|
| Administrador e Caixa no mesmo sistema | Políticas nos controllers, claims e verificação da conta por requisição |
| Clientes com múltiplos endereços | Cadastro/edição, busca por telefone e seleção na venda; vínculo validado |
| Mesa / Retirada / Delivery | Quantidades, pagamentos e campos por modalidade; três vendas testadas |
| Cálculo e histórico | Preços do banco, totais no servidor, snapshot de preço/nome/endereço/cliente |
| Operador e auditoria | ID vindo da sessão, sem aceitar usuário escolhido pelo navegador |
| Idempotência | Chave única e trava transacional; reenvio sequencial e concorrente verificados |
| Estoque integrado | Consumo direto e associado, baixa na finalização, concorrência pela última unidade |
| Cancelamento | Administrador, estorno exato e uma única vez, preservação do histórico |
| Integridade | FK RESTRICT, desativação lógica, rollback de pedido e de todas as baixas |
| Demonstração segura | Dados fictícios sob ação administrativa em Development; nenhum usuário padrão |

## Limitações e próximos passos reais

- Escolher login/senha e criar o primeiro administrador do banco principal com o procedimento local do README; não foi inventada uma senha fixa.
- Fotos comerciais, homologação com o restaurante, checkout bancário/PIX real e impressão fiscal não fazem parte da implementação validada. As imagens são ilustrações demonstrativas locais.
- A migração e os testes foram feitos em Linux/MySQL 8.4.13. O passo a passo Windows está documentado, mas não foi executado em Windows.
- Não foi executado teste de carga amplo, auditoria externa de segurança ou restauração em nova tarefa após publicação da nuvem.
- Preservação de dados antigos não permite recuperar textos históricos que não existiam nas tabelas originais. Valores originais são mantidos.
- A migration antiga de insumos requer revisão específica se ainda estiver pendente em outro banco com dados; não foi alterada para esconder esse risco.
